using Meridian.Domain.Common;

namespace Meridian.Domain.Accounts;

public sealed record AccountOpenedEvent(Guid AccountId, string OwnerId, string Currency) : DomainEvent;
public sealed record AccountCreditedEvent(Guid AccountId, decimal Amount, decimal NewBalance, string Reference) : DomainEvent;
public sealed record AccountDebitedEvent(Guid AccountId, decimal Amount, decimal NewBalance, string Reference) : DomainEvent;
public sealed record AccountFrozenEvent(Guid AccountId) : DomainEvent;
public sealed record AccountClosedEvent(Guid AccountId) : DomainEvent;
