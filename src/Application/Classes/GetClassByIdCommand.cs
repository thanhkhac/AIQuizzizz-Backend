using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Application.Classes;

public class ClassDetailDto
{
    public Guid ClassId { get; set; }
    public string? Name { get; set; }
    public string? Topic { get; set; }
}
public class GetClassByIdCommand : IRequest<ClassDetailDto>
{
    public required Guid ClassId { get; set; }   
}

public class GetClassByIdCommandValidator : AbstractValidator<GetClassByIdCommand>
{
    public GetClassByIdCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId ko đc rỗng");
    }
}

public class GetClassByIdCommandHandler : IRequestHandler<GetClassByIdCommand, ClassDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public GetClassByIdCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<ClassDetailDto> Handle(GetClassByIdCommand rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);

        await _classService.IsUserInClass(rq.ClassId);
        
        return new ClassDetailDto { ClassId = classById!.Id, Name = classById.Name, Topic = classById.Topic };
    }
}
