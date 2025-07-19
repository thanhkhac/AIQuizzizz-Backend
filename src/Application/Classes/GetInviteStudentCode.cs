using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

public class GetInviteStudentCode : IRequest<string?>
{
    public required Guid ClassId { get; set; }   
}

public class GetInviteStudentCodeValidator : AbstractValidator<GetInviteStudentCode>
{
    public GetInviteStudentCodeValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
    }
}

public class GetInviteStudentCodeHandler : IRequestHandler<GetInviteStudentCode, string?>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService; 
    
    public GetInviteStudentCodeHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<string?> Handle(GetInviteStudentCode rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        await _classService.IsLecturerOrOwnerInClass(rq.ClassId);
        
        var code = await _context.ClassInvitations
            .Where(x => x.ClassId == rq.ClassId && x.IsDeleted == false)
            .OrderByDescending(x => x.TimeEnd)
            .FirstOrDefaultAsync(cancellationToken);
        
        return code != null ? code.Code : null;
    }
}
