using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Application.Classes;

public class UpdateClassCommand : IRequest<Guid>
{
    public required Guid ClassId { get; set; }
    public string? Name { get; set; }
    public string? Topic { get; set; }
}

public class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được trống");
    }
}

public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public UpdateClassCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<Guid> Handle(UpdateClassCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classService
            .GetClassOwnerAccess(rq.ClassId, cancellationToken);
        
        if (!string.IsNullOrWhiteSpace(rq.Name))
            classExists.Name = rq.Name;

        if (!string.IsNullOrWhiteSpace(rq.Topic))
            classExists.Topic = rq.Topic;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return classExists.Id;
    }
}
