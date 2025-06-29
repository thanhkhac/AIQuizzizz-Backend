using System.Reflection;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using System.Linq.Dynamic.Core;

namespace CleanArchitectureBase.Application.Users;

public class AccountDto
{
    public required string Email { get; set; }
    public string? Name { get; set; }
    public long Token { get; set; }
    public bool IsBanned { get; set; }
}

public class GetAllAccountCommand : IRequest<PaginatedList<AccountDto>>
{
    public string? Keyword { get; set; }
    public string? FieldName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class GetAllAccountCommandValidator : AbstractValidator<GetAllAccountCommand>
{
    public GetAllAccountCommandValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
    }
}

public class GetAllAccountCommandHandler : IRequestHandler<GetAllAccountCommand, PaginatedList<AccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    
    public GetAllAccountCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    public async Task<PaginatedList<AccountDto>> Handle(GetAllAccountCommand rq, CancellationToken cancellationToken)
    {
        var admins = await _identityService.GetUsersInRoleAsync();
        
        var user = await _context.DomainUsers
            .Where(x => x.Id == _user.UserId && x.IsDeleted == false && x.IsBanned == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with id {_user.UserId} not found");

        var listUser = _context.DomainUsers
            .Where(x => x.IsDeleted == false && x.Id != user.Id && !admins.Contains(x.Id));

        if (!string.IsNullOrEmpty(rq.Keyword) && !string.IsNullOrEmpty(rq.FieldName))
        {
            var property = typeof(User).GetProperty(rq.FieldName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || property.PropertyType != typeof(string))
                throw new ErrorCodeException(ErrorCodes.FIELD_NAME_NOT_FOUND, "Trường tìm kiếm không hợp lệ hoặc không phải kiểu string");

            string query = $"{rq.FieldName}.ToLower().Contains(@0)";
            listUser = listUser.Where(query, rq.Keyword.ToLower());
        }

        return await PaginatedList<AccountDto>.CreateAsync(
            listUser.Select(u => new AccountDto
            {
                Email = u.Email, Token = u.TokenCount, IsBanned = u.IsBanned, Name = u.FullName
            }).AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
