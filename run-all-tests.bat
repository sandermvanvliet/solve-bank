@echo off

dotnet test -v n --no-build --no-restore .\test\SolveBank.Tests.Unit\SolveBank.Tests.Unit.csproj > test-report.txt
