using FluentAssertions;
using HrSaas.Modules.Employee.Domain.ValueObjects;

namespace HrSaas.Modules.Employee.UnitTests.Domain;

public sealed class EmployeeValueObjectTests
{
    [Theory]
    [InlineData("Engineering")]
    [InlineData("Human Resources")]
    [InlineData("Finance")]
    public void Department_Create_WithValidName_ShouldSucceed(string name)
    {
        var dept = Department.Create(name);
        dept.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Department_Create_WithInvalidName_ShouldThrow(string name)
    {
        var act = () => Department.Create(name);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Department_SameValues_ShouldBeEqual()
    {
        var d1 = Department.Create("Engineering");
        var d2 = Department.Create("Engineering");
        d1.Should().Be(d2);
    }

    [Fact]
    public void Department_DifferentValues_ShouldNotBeEqual()
    {
        var d1 = Department.Create("Engineering");
        var d2 = Department.Create("Finance");
        d1.Should().NotBe(d2);
    }
}
