using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

public class ClassDetailDto
{
    public Guid ClassId { get; set; }
    public string? Name { get; set; }
    public string? Topic { get; set; }
}

[Authorize]
public class GetClassByIdQuery : IRequest<ClassDetailDto>
{
    /// <summary>
    /// Id of the class want to retrieve details
    /// </summary>
    public required Guid ClassId { get; set; }   
}

public class GetClassByIdQueryValidator : AbstractValidator<GetClassByIdQuery>
{
    public GetClassByIdQueryValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId ko đc rỗng");
    }
}

public class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, ClassDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public GetClassByIdQueryHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    /// <summary>
    /// The function retrieves details of a class by its ID and returns the class details
    /// </summary>
    /// <param name="rq">Request contains ClassId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<ClassDetailDto> Handle(GetClassByIdQuery rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy class");

        await _classService.IsUserInClass(rq.ClassId);
        
        return new ClassDetailDto { ClassId = classById.Id, Name = classById.Name, Topic = classById.Topic };
    }
}
