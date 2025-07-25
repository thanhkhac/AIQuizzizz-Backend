using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

public class UpdatePositionDto
{
    public required string Position { get; set; }
    public Guid UserId { get; set; }
}

[Authorize]
public class UpdatePositionCommand : IRequest<UpdatePositionDto>
{
    /// <summary>
    /// Id of the class want to update position
    /// </summary>
    public required Guid ClassId { get; set; }
    /// <summary>
    /// If UserId is not provided, the position of the user who sent the request will be updated
    /// </summary>
    public required Guid UserId { get; set; }
    public required string? Position { get; set; }
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
            .Must(mode => new[] {"Student", "Teacher"}.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Student, Teacher");
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
    
    /// <summary>
    /// The function updates the position of a user in a class and returns the updated position details
    /// </summary>
    /// <param name="rq">Request contains ClassId, UserId, and Position information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<UpdatePositionDto> Handle(UpdatePositionCommand rq, CancellationToken cancellationToken)
    {
        var classUserData = await _context.ClassUsers
            .Include(x => x.Class)
            .Where(cu => cu.ClassId == rq.ClassId &&
                          (cu.UserId == _user.UserId || (cu.UserId == rq.UserId && cu.Class.IsDeleted == false)))
            .Select(cu => new { ClassUser = cu, Class = cu.Class })
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
        
        classUser.ClassUser.ShareMode = Enum.Parse<ClassShareMode>(rq.Position
                                                                   ?? throw new ErrorCodeException(ErrorCodes.PERMISSION_NOT_FOUND, "không tìm thầy role"));
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdatePositionDto { Position = classUser.ClassUser.ShareMode.ToString(), UserId = rq.UserId };
    }
}
