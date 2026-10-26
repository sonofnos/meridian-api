namespace Meridian.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class InsufficientFundsException(Guid accountId, decimal requested, decimal available)
    : DomainException($"Account {accountId} has insufficient funds: requested {requested}, available {available}");

public class AccountNotFoundException(Guid accountId)
    : DomainException($"Account {accountId} was not found.");

public class TransactionNotFoundException(Guid transactionId)
    : DomainException($"Transaction {transactionId} was not found.");

public class DuplicateReferenceException(string reference)
    : DomainException($"A transaction with reference '{reference}' already exists.");
