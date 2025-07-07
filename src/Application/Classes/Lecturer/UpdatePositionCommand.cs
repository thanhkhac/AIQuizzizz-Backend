using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class UpdatePositionDto
{
    public required string Position { get; set; }
    public Guid UserId { get; set; }
}

[Authorize]
public class UpdatePositionCommand : IRequest<UpdatePositionDto>
{
    public required Guid ClassId { get; set; }
    public required Guid UserId { get; set; }
    public required ClassShareMode Position { get; set; }
}

public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không thể trống");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không thể trống");
        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position không thể trống");
    }
}

public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, UpdatePositionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public UpdatePositionCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<UpdatePositionDto> Handle(UpdatePositionCommand rq, CancellationToken cancellationToken)
    {
        var classUserData = await _context.ClassUsers
            .Where(cu => cu.ClassId == rq.ClassId)
            .Join(_context.Classes,
                cu => cu.ClassId,
                c => c.Id,
                (cu, c) => new { ClassUser = cu, Class = c })
            .Where(x => x.ClassUser.UserId == _user.UserId || x.ClassUser.UserId == rq.UserId && x.Class.IsDeleted == false)
            .ToListAsync(cancellationToken);
        
        var isOwner = classUserData.FirstOrDefault(x => x.ClassUser.UserId == _user.UserId && x.ClassUser.ShareMode == ClassShareMode.Owner);
        if (isOwner == null)
            throw new ErrorCodeException(ErrorCodes.ONLY_OWNERS_CAN_UPDATE, "Không tìm thấy lecturer hoặc không phải leacturer của lớp");

        var classExists = classUserData.FirstOrDefault(x => x.Class.Id == rq.ClassId);
        if (classExists == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        var classUser = classUserData.FirstOrDefault(x => x.ClassUser.UserId == rq.UserId && x.ClassUser.ShareMode != ClassShareMode.Owner);
        if (classUser == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_STUDENT_IN_CLASS, "Không tìm thấy student hoặc không phải student của lớp");
        
        classUser.ClassUser.ShareMode = rq.Position;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdatePositionDto { Position = classUser.ClassUser.ShareMode.ToString(), UserId = rq.UserId };
    }
}
