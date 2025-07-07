using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Roles.Queries;
public class GetPermissionGroupsQuery : IRequest<List<PermissionGroupDto>>
{
}

public class PermissionGroupDto
{
    public string Name { get; set; } = null!;
    public List<ClaimDto> Claims { get; set; } = new();
}

public class ClaimDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class GetPermissionGroupsQueryHandler : IRequestHandler<GetPermissionGroupsQuery, List<PermissionGroupDto>>
{
    //Tại sao phải dùng task ở đây?
    public Task<List<PermissionGroupDto>> Handle(GetPermissionGroupsQuery request, CancellationToken cancellationToken)
    {
        var result = PermissionConstants.Groups.All
            .Select(g => new PermissionGroupDto
            {
                Name = g.Name,
                Claims = g.Claims.Select(c => new ClaimDto
                {
                    Code = c.Code,
                    Name = c.Name
                }).ToList()
            })
            .ToList();

        return Task.FromResult(result);
    }
}
