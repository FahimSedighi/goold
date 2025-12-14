using MediatR;

namespace GoldPriceTracker.Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries in CQRS pattern.
/// Queries represent read operations.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

