##########################################################################
# This is the Cake bootstrapper script for PowerShell.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

<#

.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Version
The version that will be passed in from TeamCity.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Experimental
Tells Cake to use the latest Roslyn release.
.PARAMETER WhatIf
Performs a dry run of the build script.
No tasks will be executed.
.PARAMETER Mono
Tells Cake to use the Mono scripting engine.
.PARAMETER SkipToolPackageRestore
Skips restoring of packages.
.PARAMETER ScriptArgs
Remaining arguments are added here.

.LINK
https://cakebuild.net

#>

[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target = "Default",
    [string]$Version = "1.0.0",
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [switch]$Experimental,
    [Alias("DryRun","Noop")]
    [switch]$WhatIf,
    [switch]$Mono,
    [switch]$SkipToolPackageRestore,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs,
		[switch]$Diagnostic
)

[Reflection.Assembly]::LoadWithPartialName("System.Security") | Out-Null

# Hacktastic approach to figure out if we're running PowerShell on Unix
$IsRunningUnix = ($PSVersionTable.Platform -eq "Unix")

function MD5HashFile([string] $filePath)
{
    if ([string]::IsNullOrEmpty($filePath) -or !(Test-Path $filePath -PathType Leaf))
    {
        return $null
    }

    [System.IO.Stream] $file = $null;
    [System.Security.Cryptography.MD5] $md5 = $null;
    try
    {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $file = [System.IO.File]::OpenRead($filePath)
        return [System.BitConverter]::ToString($md5.ComputeHash($file))
    }
    finally
    {
        if ($file -ne $null)
        {
            $file.Dispose()
        }
    }
}

Write-Host "Preparing to run build script..."

if(!$PSScriptRoot){
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$ADDINS_DIR = Join-Path $TOOLS_DIR "Addins"
$MODULES_DIR = Join-Path $TOOLS_DIR "Modules"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
$CAKE_EXE = Join-Path $TOOLS_DIR "Cake/Cake.exe"
$NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
$PACKAGES_CONFIG_MD5 = Join-Path $TOOLS_DIR "packages.config.md5sum"
$ADDINS_PACKAGES_CONFIG = Join-Path $ADDINS_DIR "packages.config"
$MODULES_PACKAGES_CONFIG = Join-Path $MODULES_DIR "packages.config"
$MONO_PATH = ""

# If running on Unix-y platform, check if mono exists
if($IsRunningUnix) {
    $MONO_PATH = (Invoke-Expression "which mono")

    if(!($MONO_PATH)) {
        Write-Error -Message "Could not locate Mono binary"
        Exit 1
    }
}

# Should we use mono?
$UseMono = "";
if($Mono.IsPresent) {
    Write-Verbose -Message "Using the Mono based scripting engine."
    $UseMono = "-mono"
}

# Should we use the new Roslyn?
$UseExperimental = "";
if($Experimental.IsPresent -and !($Mono.IsPresent)) {
    Write-Verbose -Message "Using experimental version of Roslyn."
    $UseExperimental = "-experimental"
}

# Is this a dry run?
$UseDryRun = "";
if($WhatIf.IsPresent) {
    $UseDryRun = "-dryrun"
}

# Make sure tools folder exists
if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    Write-Verbose -Message "Creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

# Make sure that packages.config exist.
if (!(Test-Path $PACKAGES_CONFIG)) {
    Write-Verbose -Message "Downloading packages.config..."
    try { (New-Object System.Net.WebClient).DownloadFile("https://cakebuild.net/download/bootstrapper/packages", $PACKAGES_CONFIG) } catch {
        Throw "Could not download packages.config."
    }
}

# Try find NuGet.exe in path if not exists
if (!(Test-Path $NUGET_EXE)) {
    Write-Verbose -Message "Trying to find nuget.exe in PATH..."
    $existingPaths = $Env:Path -Split ';' | Where-Object { (![string]::IsNullOrEmpty($_)) -and (Test-Path $_ -PathType Container) }
    $NUGET_EXE_IN_PATH = Get-ChildItem -Path $existingPaths -Filter "nuget.exe" | Select -First 1
    if ($NUGET_EXE_IN_PATH -ne $null -and (Test-Path $NUGET_EXE_IN_PATH.FullName)) {
        Write-Verbose -Message "Found in PATH at $($NUGET_EXE_IN_PATH.FullName)."
        $NUGET_EXE = $NUGET_EXE_IN_PATH.FullName
    }
}

# Try download NuGet.exe if not exists
if (!(Test-Path $NUGET_EXE)) {
    Write-Verbose -Message "Downloading NuGet.exe..."
    try {
        (New-Object System.Net.WebClient).DownloadFile($NUGET_URL, $NUGET_EXE)
    } catch {
        Throw "Could not download NuGet.exe."
    }
}

# Save nuget.exe path to environment to be available to child processed
$ENV:NUGET_EXE = $NUGET_EXE

# Restore tools from NuGet?
if(-Not $SkipToolPackageRestore.IsPresent) {
    Push-Location
    Set-Location $TOOLS_DIR

    # Check for changes in packages.config and remove installed tools if true.
    [string] $md5Hash = MD5HashFile($PACKAGES_CONFIG)
    if((!(Test-Path $PACKAGES_CONFIG_MD5)) -Or
      ($md5Hash -ne (Get-Content $PACKAGES_CONFIG_MD5 ))) {
        Write-Verbose -Message "Missing or changed package.config hash..."
        Remove-Item * -Recurse -Exclude packages.config,nuget.exe
    }

    Write-Verbose -Message "Restoring tools from NuGet..."
    if($IsRunningUnix) {
        $NuGetOutput = Invoke-Expression "&`"$MONO_PATH`" `"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$TOOLS_DIR`""
    }
    else {
        $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$TOOLS_DIR`""
    }

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while restoring NuGet tools."
    }
    else
    {
        $md5Hash | Out-File $PACKAGES_CONFIG_MD5 -Encoding "ASCII"
    }
    Write-Verbose -Message ($NuGetOutput | out-string)
    
    Pop-Location
}

# Restore addins from NuGet
if (Test-Path $ADDINS_PACKAGES_CONFIG) {
    Push-Location
    Set-Location $ADDINS_DIR

    Write-Verbose -Message "Restoring addins from NuGet..."
    if($IsRunningUnix) {
        $NuGetOutput = Invoke-Expression "&`"$MONO_PATH`" `"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$ADDINS_DIR`""
    }
    else {
        $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$ADDINS_DIR`""
    }

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while restoring NuGet addins."
    }

    Write-Verbose -Message ($NuGetOutput | out-string)

    Pop-Location
}

# Restore modules from NuGet
if (Test-Path $MODULES_PACKAGES_CONFIG) {
    Push-Location
    Set-Location $MODULES_DIR

    Write-Verbose -Message "Restoring modules from NuGet..."
    if($IsRunningUnix) {
        $NuGetOutput = Invoke-Expression "&`"$MONO_PATH`" `"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$MODULES_DIR`""
    }
    else {
        $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$MODULES_DIR`""
    }

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while restoring NuGet modules."
    }

    Write-Verbose -Message ($NuGetOutput | out-string)

    Pop-Location
}

# Make sure that Cake has been installed.
if (!(Test-Path $CAKE_EXE)) {
    Throw "Could not find Cake.exe at $CAKE_EXE"
}

# Check to see if the version number has been provided by the build agent:
Write-Output "Checking for version number in environment variables..."
if (${env:BUILD_NUMBER} -ne $null)
{
    $Version = ${env:BUILD_NUMBER};
    Write-Output "Found version number in Environment variables! Using version number '$Version'." 
} else 
{
	if (${env:APPVEYOR_BUILD_NUMBER} -ne $null)
	{
    $Version = ${env:APPVEYOR_BUILD_NUMBER};
    Write-Output "Found version number in Environment variables! Using version number '$Version'." 
	} else {
    Write-Output "Version number not found in Environment variables. Defaulting to '$Version'."
	}
}

# Check to see if we need to change the verbosity for Cake:
if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent)
{
    $Verbosity = "--verbosity=Verbose "
} else {
    $Verbosity = "--verbosity=Normal "
}

if ($PSCmdLet.MyInvocation.BoundParameters["Diagnostic"].IsPresent)
{
	$Verbosity = "--verbosity=Diagnostic"
}

# Build our required expression (with all parameters, configuration, etc.)
if($IsRunningUnix) {
    $CakeExpression = "`"$MONO_PATH`" `"$CAKE_EXE`" `"$Script`" -target=`"$Target`" -packVersion=`"$Version`" -configuration=`"$Configuration`" $Verbosity $UseMono $UseDryRun $UseExperimental $ScriptArgs"
}
else {
    $CakeExpression = "`"$CAKE_EXE`" `"$Script`" -target=`"$Target`" -packVersion=`"$Version`" -configuration=`"$Configuration`"  $Verbosity $UseMono $UseDryRun $UseExperimental $ScriptArgs"
}

# Start Cake
Write-Host "About to execute '$CakeExpression'..."
Invoke-Expression "& $CakeExpression"

exit $LASTEXITCODE
