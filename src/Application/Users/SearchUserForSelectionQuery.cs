using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users.Dtos;

namespace CleanArchitectureBase.Application.Users;

public class SearchUserForSelectionQuery : IRequest<List<UserForSelectionDto>>
{
    public string? Email { get; set; }
}

public class SearchUserForSelectionQueryHandler : IRequestHandler<SearchUserForSelectionQuery, List<UserForSelectionDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchUserForSelectionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserForSelectionDto>> Handle(SearchUserForSelectionQuery request, CancellationToken cancellationToken)
    {
        var query = await _context.DomainUsers
            .Where(x => x.Email == request.Email)
            .Select(x => new UserForSelectionDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email
            }).ToListAsync(cancellationToken);

        return query;
    }
}
