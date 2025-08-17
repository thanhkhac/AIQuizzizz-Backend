using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Extensions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class CreateClassCommand : IRequest<Guid>
{
    /// <summary>
    /// Name of the class to be created
    /// </summary>
    public required string Name { get; set; }
    public string? Topic { get; set; }
}

public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tên lớp không được để trống")
            .MaximumLength(200).WithMessage("Tên lớp không được vượt quá 200 ký tự");
            
        RuleFor(v => v.Topic)
            .NullOrNotEmpty()
            .MaximumLength(200).WithMessage("Chủ đề không được vượt quá 200 ký tự");
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
    
    /// <summary>
    /// The function creates a new class and assigns the creator as the owner, returning the class ID
    /// </summary>
    /// <param name="rq">Request contains class Name and Topic information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Guid> Handle(CreateClassCommand rq, CancellationToken cancellationToken)
    {
        var newClass = new Class { Id = Guid.NewGuid(), Name = rq.Name, Topic = rq.Topic};
        
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
