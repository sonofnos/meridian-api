using FluentAssertions;
using Meridian.Application.Accounts.Commands.OpenAccount;
using Meridian.Application.Accounts.Queries.GetAccount;
using Meridian.Application.Transactions.Commands.TransferFunds;
using Meridian.Application.Transactions.Queries.GetTransactionHistory;
using Meridian.Domain.Accounts;
using System.Net;
using System.Net.Http.Json;

namespace Meridian.IntegrationTests;

public class AccountEndpointTests(MeridianWebApplicationFactory factory)
    : IClassFixture<MeridianWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task OpenAccount_ValidRequest_Returns201WithAccountId()
    {
        var cmd = new OpenAccountCommand("owner1", "Alice Mwangi", "KES");

        var response = await _client.PostAsJsonAsync("/api/accounts", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<OpenAccountResult>();
        result!.AccountId.Should().NotBeEmpty();
        result.Currency.Should().Be("KES");
    }

    [Fact]
    public async Task OpenAccount_InvalidCurrency_Returns422()
    {
        var cmd = new OpenAccountCommand("owner1", "Alice", "XYZ");

        var response = await _client.PostAsJsonAsync("/api/accounts", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetAccount_ExistingAccount_ReturnsAccountDto()
    {
        var opened = await OpenAccountAsync("owner2", "Bob Kariuki", "USD");

        var response = await _client.GetAsync($"/api/accounts/{opened.AccountId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<AccountDto>();
        dto!.Id.Should().Be(opened.AccountId);
        dto.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public async Task GetAccount_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FreezeAndUnfreeze_Account_ChangesStatus()
    {
        var opened = await OpenAccountAsync("owner3", "Carol Akinyi", "KES");

        var freeze = await _client.PostAsync($"/api/accounts/{opened.AccountId}/freeze", null);
        freeze.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var frozen = await _client.GetFromJsonAsync<AccountDto>($"/api/accounts/{opened.AccountId}");
        frozen!.Status.Should().Be(AccountStatus.Frozen);

        var unfreeze = await _client.PostAsync($"/api/accounts/{opened.AccountId}/unfreeze", null);
        unfreeze.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var active = await _client.GetFromJsonAsync<AccountDto>($"/api/accounts/{opened.AccountId}");
        active!.Status.Should().Be(AccountStatus.Active);
    }

    private async Task<OpenAccountResult> OpenAccountAsync(string ownerId, string name, string currency)
    {
        var cmd = new OpenAccountCommand(ownerId, name, currency);
        var response = await _client.PostAsJsonAsync("/api/accounts", cmd);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OpenAccountResult>())!;
    }
}
