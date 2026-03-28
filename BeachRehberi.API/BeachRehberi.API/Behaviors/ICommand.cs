using MediatR;

namespace BeachRehberi.API.Behaviors;

public interface IBaseCommand { }
public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand { }

