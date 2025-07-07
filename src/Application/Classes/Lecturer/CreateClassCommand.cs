using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

[Authorize]
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
        var classExists = await _context.Classes
            .Where(x => x.Name == rq.Name && x.CreatedBy.Equals(_user.UserId))
            .FirstOrDefaultAsync(cancellationToken);
        if (classExists != null)
            throw new ErrorCodeException(ErrorCodes.CLASS_ALREADY_EXISTS, "Class đã tồn tại");

        var newClass = new Class { Id = Guid.NewGuid(), Name = rq.Name };
        
        var classUser = new ClassUser
        {
            UserId = _user.UserId!.Value,
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
