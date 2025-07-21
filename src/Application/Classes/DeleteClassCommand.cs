using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class DeleteClassCommand : IRequest<Guid>
{
    /// <summary>
    /// Id of the class want to delete
    /// </summary>
    public required Guid ClassId { get; set; }   
}

public class DeleteClassValidator : AbstractValidator<DeleteClassCommand>
{
    public DeleteClassValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không thể trống");
    }
}

public class DeleteClassHandler : IRequestHandler<DeleteClassCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public DeleteClassHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    /// <summary>
    /// The function soft-deletes a class and returns the class ID
    /// </summary>
    /// <param name="rq">Request contains ClassId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Guid> Handle(DeleteClassCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classService
            .GetClassOwnerAccess(rq.ClassId, cancellationToken);

        classExists.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return classExists.Id;

    }
}
