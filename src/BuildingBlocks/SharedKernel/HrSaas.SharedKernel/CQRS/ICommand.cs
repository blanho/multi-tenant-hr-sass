using MediatR;

namespace HrSaas.SharedKernel.CQRS;

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

public interface ICommand : IRequest<Result> { }

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
