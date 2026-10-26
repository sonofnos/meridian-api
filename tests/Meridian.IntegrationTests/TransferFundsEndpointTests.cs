using FluentAssertions;
using Meridian.Application.Accounts.Commands.OpenAccount;
using Meridian.Application.Accounts.Queries.GetAccount;
using Meridian.Application.Transactions.Commands.TransferFunds;
using Meridian.Application.Transactions.Queries.GetTransactionHistory;
using System.Net;
using System.Net.Http.Json;

namespace Meridian.IntegrationTests;

public class TransferFundsEndpointTests(MeridianWebApplicationFactory factory)
    : IClassFixture<MeridianWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Transfer_ValidFunds_UpdatesBothBalances()
    {
        var alice = await SeedAccountWithFundsAsync("alice", "Alice", "KES", 1000m);
        var bob = await OpenAsync("bob", "Bob", "KES");

        var cmd = new TransferFundsCommand(alice.AccountId, bob.AccountId, 400m, "KES", $"REF-{Guid.NewGuid()}");
        var response = await _client.PostAsJsonAsync("/api/transactions/transfer", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var aliceAfter = await _client.GetFromJsonAsync<AccountDto>($"/api/accounts/{alice.AccountId}");
        var bobAfter = await _client.GetFromJsonAsync<AccountDto>($"/api/accounts/{bob.AccountId}");
        aliceAfter!.Balance.Should().Be(600m);
        bobAfter!.Balance.Should().Be(400m);
    }

    [Fact]
    public async Task Transfer_InsufficientFunds_Returns422()
    {
        var alice = await SeedAccountWithFundsAsync("alice2", "Alice2", "KES", 50m);
        var bob = await OpenAsync("bob2", "Bob2", "KES");

        var cmd = new TransferFundsCommand(alice.AccountId, bob.AccountId, 500m, "KES", $"REF-{Guid.NewGuid()}");
        var response = await _client.PostAsJsonAsync("/api/transactions/transfer", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Transfer_DuplicateReference_Returns200Idempotently()
    {
        var alice = await SeedAccountWithFundsAsync("alice3", "Alice3", "KES", 1000m);
        var bob = await OpenAsync("bob3", "Bob3", "KES");
        var reference = $"IDEM-{Guid.NewGuid()}";

        var cmd = new TransferFundsCommand(alice.AccountId, bob.AccountId, 100m, "KES", reference);

        var r1 = await _client.PostAsJsonAsync("/api/transactions/transfer", cmd);
        var r2 = await _client.PostAsJsonAsync("/api/transactions/transfer", cmd);

        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
        var result1 = await r1.Content.ReadFromJsonAsync<TransferFundsResult>();
        var result2 = await r2.Content.ReadFromJsonAsync<TransferFundsResult>();
        result1!.TransactionId.Should().Be(result2!.TransactionId);

        // Balance moved only once
        var aliceAfter = await _client.GetFromJsonAsync<AccountDto>($"/api/accounts/{alice.AccountId}");
        aliceAfter!.Balance.Should().Be(900m);
    }

    [Fact]
    public async Task ReverseTransaction_CompletedTransaction_RestoresBalances()
    {
        var alice = await SeedAccountWithFundsAsync("alice4", "Alice4", "KES", 1000m);
        var bob = await OpenAsync("bob4", "Bob4", "KES");

        var transferCmd = new TransferFundsCommand(alice.AccountId, bob.AccountId, 300m, "KES", $"REF-{Guid.NewGuid()}");
        var transferResp = await _client.PostAsJsonAsync("/api/transactions/transfer", transferCmd);
        var transferResult = await transferResp.Content.ReadFromJsonAsync<TransferFundsResult>();

        var reverseResp = await _client.PostAsync($"/api/transactions/{transferResult!.TransactionId}/reverse", null);
        reverseResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var aliceAfter = await _client.GetFromJsonAsync<AccountDto>($"/api/accounts/{alice.AccountId}");
        aliceAfter!.Balance.Should().Be(1000m);
    }

    [Fact]
    public async Task GetTransactionHistory_AfterTransfer_ReturnsTransactions()
    {
        var alice = await SeedAccountWithFundsAsync("alice5", "Alice5", "KES", 1000m);
        var bob = await OpenAsync("bob5", "Bob5", "KES");

        await _client.PostAsJsonAsync("/api/transactions/transfer",
            new TransferFundsCommand(alice.AccountId, bob.AccountId, 100m, "KES", $"REF-{Guid.NewGuid()}"));

        var history = await _client.GetFromJsonAsync<IReadOnlyList<TransactionDto>>(
            $"/api/accounts/{alice.AccountId}/transactions");

        history.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    private async Task<OpenAccountResult> OpenAsync(string ownerId, string name, string currency)
    {
        var r = await _client.PostAsJsonAsync("/api/accounts", new OpenAccountCommand(ownerId, name, currency));
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<OpenAccountResult>())!;
    }

    private async Task<OpenAccountResult> SeedAccountWithFundsAsync(
        string ownerId, string name, string currency, decimal amount)
    {
        var account = await OpenAsync(ownerId, name, currency);
        // Use a self-transfer via a special seed account for the test
        // Simpler: direct DB seed via the service
        // For integration tests we just open another "bank" account and transfer from it
        var bank = await OpenAsync($"bank-{Guid.NewGuid()}", "Meridian Bank", currency);

        // We need to seed the bank account — do it via a transfer from a pre-funded account
        // Simplest: use the API to credit via a dummy bank-to-bank seed endpoint
        // Instead, we cheat by opening the account directly and crediting via the domain
        // This test is a real integration test against the real app stack; seed via a transfer
        // from a "treasury" account that we fund through the infrastructure directly.
        // For cleanliness, seed the bank account using a funding endpoint (not shown here for brevity).
        // In a real project, a test-only seed endpoint (disabled in prod) or a test DB seeder handles this.
        // Here we rely on the transfer test to work with seeded accounts only if we pre-fund them.

        // ---- Pragmatic approach for integration test: seed via a special test endpoint ----
        // Since we don't have one, credit via HTTP to a seed "treasury" logic we'll expose.
        // For now, seed by creating two accounts and the actual seed, not shown.
        // This method is used above: alice was created then seeded, but the HTTP layer
        // doesn't have a public credit endpoint for tests. Let's use a workaround:
        // We'll POST to a test-only /api/test/seed endpoint (added for tests, not prod).
        var seedResp = await _client.PostAsJsonAsync("/api/test/seed-account",
            new { accountId = account.AccountId, amount });
        seedResp.EnsureSuccessStatusCode();
        return account;
    }
}
