using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class CreateClassCommand : IRequest<Guid>
{
    public required string Name { get; set; }
}

public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tên lớp không được để trống");
    }
}

public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public CreateClassCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<Guid> Handle(CreateClassCommand rq, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers.Where(x => x.Id == _user.UserId && x.IsDeleted == false && x.IsBanned == false)
            .FirstOrDefaultAsync();
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");
        
        if (await _context.Classes.AnyAsync(x => x.Name == rq.Name && x.IsDeleted == false))
        {
            throw new ErrorCodeException(ErrorCodes.CLASS_ALREADY_EXISTS, "Tên lớp học đã tồn tại");
        }

        var newClass = new Class { Id = Guid.NewGuid(), Name = rq.Name };
        
        var classUser = new ClassUser
        {
            UserId = user.Id,
            ClassId = newClass.Id,
            ShareMode = ClassShareMode.Owner,
            Class = newClass
        };
        
        _context.Classes.Add(newClass);
        _context.ClassUsers.Add(classUser);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return newClass.Id;
    }
}
