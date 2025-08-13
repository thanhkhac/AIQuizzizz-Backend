using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class UpdateClassCommand : IRequest<Guid>
{
    public Guid? ClassId { get; set; }
    public required string Name { get; set; }
    public string? Topic { get; set; }
}

public class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được trống");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name không được trống");
    }
}

public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    private readonly IUser _user;
    
    public UpdateClassCommandHandler(IApplicationDbContext context, IClassService classService, IUser user)
    {
        _context = context;
        _classService = classService;
        _user = user;       
    }
    
    public async Task<Guid> Handle(UpdateClassCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classService
            .GetClassOwnerAccess(rq.ClassId!.Value, cancellationToken);
               
        classExists.Name = rq.Name;

        if (!string.IsNullOrWhiteSpace(rq.Topic))
            classExists.Topic = rq.Topic;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return classExists.Id;
    }
}
