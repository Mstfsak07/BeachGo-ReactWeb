using MediatR;

namespace BeachRehberi.Application.Common;

/// <summary>
/// Base class for all commands
/// </summary>
public abstract class CommandBase : IRequest<Result>
{
    public Guid TenantId { get; set; }
}

/// <summary>
/// Base class for commands that return data
/// </summary>
public abstract class CommandBase<TResponse> : IRequest<Result<TResponse>>
{
    public Guid TenantId { get; set; }
}

/// <summary>
/// Base class for all queries
/// </summary>
public abstract class QueryBase<TResponse> : IRequest<Result<TResponse>>
{
    public Guid TenantId { get; set; }
}