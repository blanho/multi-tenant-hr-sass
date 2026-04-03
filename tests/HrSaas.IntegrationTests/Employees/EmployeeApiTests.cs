using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HrSaas.IntegrationTests.Infrastructure;

namespace HrSaas.IntegrationTests.Employees;

public sealed class EmployeeApiTests(HrSaasWebAppFactory factory) : IClassFixture<HrSaasWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetEmployee_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/employees/00000000-0000-0000-0000-000000000001");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllEmployees_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/employees");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
