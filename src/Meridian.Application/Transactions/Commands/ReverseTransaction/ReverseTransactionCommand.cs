using MediatR;

namespace Meridian.Application.Transactions.Commands.ReverseTransaction;

public sealed record ReverseTransactionCommand(Guid TransactionId) : IRequest;
