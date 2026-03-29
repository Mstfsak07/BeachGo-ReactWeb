using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Beaches.Commands.DeleteBeach;

// ── Command ───────────────────────────────────────────────────────────────────
public record DeleteBeachCommand(int Id) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────
public class DeleteBeachCommandHandler : IRequestHandler<DeleteBeachCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBeachCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteBeachCommand request, CancellationToken cancellationToken)
    {
        var beach = await _unitOfWork.Beaches.GetByIdAsync(request.Id, cancellationToken);

        if (beach is null || beach.IsDeleted)
            throw new NotFoundException("Plaj", request.Id);

        if (!_currentUserService.IsAdmin &&
            beach.TenantId != _currentUserService.TenantId)
            throw new ForbiddenException("Bu plajı silme yetkiniz yok.");

        beach.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
