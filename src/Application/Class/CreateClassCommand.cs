using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Class;

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
    
    public CreateClassCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Guid> Handle(CreateClassCommand rq, CancellationToken cancellationToken)
    {
        if (await _context.Classes.AnyAsync(x => x.Name == rq.Name))
        {
            throw new ErrorCodeException(ErrorCodes.CLASS_ALREADY_EXISTS, "Tên lớp học đã tồn tại");
        }

        var newClass = new Domain.Entities.Class { Id = Guid.NewGuid(), Name = rq.Name };

        var classInvitation = new ClassInvitation
        {
            Id = Guid.NewGuid(),
            ClassId = newClass.Id,
            Code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..12],
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddDays(1),
            IsDeleted = false,
            Class = newClass
        };
        
        _context.Classes.Add(newClass);
        _context.ClassInvitations.Add(classInvitation);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return newClass.Id;
    }
}
