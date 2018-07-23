using FluentAssertions;
using Xunit;

namespace SolveBank.Tests.Unit
{
    public class WhenBootstrappingTheProject
    {
        [Fact]
        public void GivenABooleanTrue_ShouldBeTrueWorks()
        {
            true
                .Should()
                .BeTrue();
        }
    }
}
