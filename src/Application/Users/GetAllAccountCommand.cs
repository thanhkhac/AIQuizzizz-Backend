using System.Reflection;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using System.Linq.Dynamic.Core;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Users;

public class UserForListDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public long Token { get; set; }
    public bool IsBanned { get; set; }
    public string? Role { get; set; }
}

[Authorize(Roles = Domain.Constants.Roles.Administrator)]
public class GetAllAccountCommand : IRequest<PaginatedList<UserForListDto>>
{
    public string? Keyword { get; set; }
    public string? FieldName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public bool? IsBanned { get; set; }
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

public class GetAllAccountCommandHandler : IRequestHandler<GetAllAccountCommand, PaginatedList<UserForListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetAllAccountCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<PaginatedList<UserForListDto>> Handle(GetAllAccountCommand rq, CancellationToken cancellationToken)
    {
        var query = _context.DomainUsers.IgnoreQueryFilters().Where(x => x.IsDeleted == false);

        if (!string.IsNullOrEmpty(rq.Keyword) && !string.IsNullOrEmpty(rq.FieldName))
        {
            var property = typeof(User).GetProperty(rq.FieldName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || property.PropertyType != typeof(string))
                throw new ErrorCodeException(ErrorCodes.FIELD_NAME_NOT_FOUND, "Trường tìm kiếm không hợp lệ hoặc không phải kiểu string");

            string stm = $"{rq.FieldName}.ToLower().Contains(@0)";
            query = query.Where(stm, rq.Keyword.ToLower());
        }

        if (rq.IsBanned != null)
        {
            query = query.Where(x => x.IsBanned == rq.IsBanned);
        }


        var pagedResult = await PaginatedList<UserForListDto>.CreateAsync(
            query.Select(u => new UserForListDto
            {
                Id = u.Id,
                Email = u.Email,
                Token = u.TokenCount,
                IsBanned = u.IsBanned,
                Name = u.FullName
            }),
            rq.PageNumber,
            rq.PageSize
        );

        var userRoles = await _identityService.GetFirstRolesForUsersAsync(pagedResult.Items.Select(x => x.Id));

        foreach (var user in pagedResult.Items)
        {
            if (userRoles.TryGetValue(user.Id, out var role))
            {
                user.Role = role;
            }
        }
        return pagedResult;
    }
}
