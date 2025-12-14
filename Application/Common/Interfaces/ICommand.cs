using MediatR;

namespace GoldPriceTracker.Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands in CQRS pattern.
/// Commands represent write operations.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : IRequest
{
}

