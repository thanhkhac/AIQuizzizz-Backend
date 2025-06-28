using CleanArchitectureBase.Application.Classes.Common;
using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class DeleteClassCommand : IRequest<Guid>
{
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
    private readonly ClassValidationService _classValidationService;
    
    public DeleteClassHandler(IApplicationDbContext context, ClassValidationService classValidationService)
    {
        _context = context;
        _classValidationService = classValidationService;
    }
    
    public async Task<Guid> Handle(DeleteClassCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classValidationService.ValidateClassAccessAsync(rq.ClassId, cancellationToken);

        classExists.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return classExists.Id;

    }
}
