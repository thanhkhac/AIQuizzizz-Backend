using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class GetInviteStudentCodeQuery : IRequest<string?>
{
    /// <summary>
    /// Id of the class want to retrieve the active invitation code
    /// </summary>
    public required Guid ClassId { get; set; }   
}

public class GetInviteStudentCodeQueryValidator : AbstractValidator<GetInviteStudentCodeQuery>
{
    public GetInviteStudentCodeQueryValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
    }
}

public class GetInviteStudentCodeQueryHandler : IRequestHandler<GetInviteStudentCodeQuery, string?>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService; 
    
    public GetInviteStudentCodeQueryHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    /// <summary>
    /// The function retrieves the active invitation code for a class, if it exists, and returns the code
    /// </summary>
    /// <param name="rq">Request contains ClassId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<string?> Handle(GetInviteStudentCodeQuery rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        await _classService.IsLecturerOrOwnerInClass(rq.ClassId);
        
        var code = await _context.ClassInvitations
            .Where(x => x.ClassId == rq.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        
        return code != null ? code.Code : null;
    }
}
