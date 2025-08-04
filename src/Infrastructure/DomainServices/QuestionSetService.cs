using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Application.Tags.Dto;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Infrastructure.DomainServices;

public class QuestionSetService : IQuestionSetService
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public QuestionSetService(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    //Owner có thể edit hoặc được share trong bảng QuestionSetUser với QuestionSetUserShareMode Editable
    public async Task<bool> CanUserEditQuestionSet(Guid userId, Guid questionSetId)
    {
        return await _context.QuestionSetUsers
            .AnyAsync(qsu => qsu.QuestionSetId == questionSetId
                             && qsu.UserId == userId
                             && (qsu.ShareMode == QuestionSetUserShareMode.Owner
                                 || qsu.ShareMode == QuestionSetUserShareMode.Editable));
    }

    //Owner có thể edit hoặc được share trong bảng QuestionSetUser với bất kỳ QuestionSetUserShareMode nào
    //Hoặc là questionSet đó được hiển thị ở chế độ class
    //Hoặc questionSet đó public
    public async Task<bool> CanUserViewQuestionSet(Guid? userId, Guid questionSetId)
    {
        var questionSet = await _context.QuestionSets
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync(qs => qs.Id == questionSetId);

        if (questionSet == null)
            return false;

        return await CanUserViewQuestionSet(userId, questionSet);
    }

    public async Task<bool> CanUserViewQuestionSet(Guid? userId, QuestionSet questionSet)
    {
        // Public question sets can be viewed by anyone
        if (questionSet.VisibilityMode == QuestionSetVisibilityMode.Public)
            return true;

        if (userId == null) return false;

        if (await _identityService.IsInAnyRoleAsync(userId.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator))
            return true;

        // Kiểm tra xem người dùng được share quyền nào không
        var hasShareAccess = await _context.QuestionSetUsers
            .AnyAsync(qsu => qsu.QuestionSetId == questionSet.Id && qsu.UserId == userId.Value);

        if (hasShareAccess)
            return true;

        // Check theo class mode
        if (questionSet.VisibilityMode == QuestionSetVisibilityMode.OnlyClass)
        {
            var classIds = _context.ClassQuestionSets.Where(x => x.QuestionSetId == questionSet.Id).Select(cqs => cqs.ClassId).ToList();

            return await _context.ClassUsers
                .AnyAsync(cu => classIds.Contains(cu.ClassId) && cu.UserId == userId);
        }
        return false;
    }

    //Owner có thể delete (Check trong bảng QuestionSetUserShareMode)
    public async Task<bool> CanUserDeleteQuestionSet(Guid userId, Guid questionSetId)
    {
        if (await _identityService.IsInAnyRoleAsync(userId, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator))
            return true;

        return await _context.QuestionSetUsers
            .AnyAsync(qsu => qsu.QuestionSetId == questionSetId
                             && qsu.UserId == userId
                             && qsu.ShareMode == QuestionSetUserShareMode.Owner);
    }


    public async Task<QuestionSetPermissionsDto> GetPermissions(Guid userId, Guid questionSetId)
    {
        var canEdit = false;
        bool canDelete = await _identityService.IsInAnyRoleAsync(userId, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        // Nếu là admin hoặc moderator thì có quyền delete

        // Nếu không phải admin thì kiểm tra theo bảng QuestionSetUsers
        var qsu = await _context.QuestionSetUsers
            .Where(q => q.QuestionSetId == questionSetId && q.UserId == userId)
            .Select(q => q.ShareMode)
            .FirstOrDefaultAsync();
        if (canDelete == false)
        {
            canDelete = qsu == QuestionSetUserShareMode.Owner;
        }
        if (canEdit == false)
        {
            canEdit = qsu == QuestionSetUserShareMode.Owner || qsu == QuestionSetUserShareMode.Editable;
        }
        return new QuestionSetPermissionsDto
        {
            CanEdit = canEdit,
            CanDelete = canDelete
        };
    }

    public async Task<QuestionSet?> GetActiveQuestionSet(Guid questionSetId, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x =>
                x.Id == questionSetId
                && x.IsDeleted == false
                && x.CreatedByUser != null
                && x.CreatedByUser.IsDeleted == false
                && x.CreatedByUser.IsBanned == false, cancellationToken: cancellationToken);
        return questionSet;
    }

    public async Task<PaginatedList<QuestionSetForListResponseDto>> SearchPublicQuestionSetsAsync(
        string? name,
        List<Guid>? tagIds,
        string? sortBy,
        int confidenceFactor,
        int pageNumber,
        int pageSize, CancellationToken cancellationToken)
    {
        const int m = 5;
        double globalAverage = 0;

        var rated = _context.QuestionSets.Where(q => q.RatingCount > 0);
        if (await rated.AnyAsync(cancellationToken))
        {
            globalAverage = await rated.AverageAsync(q => q.RatingAverage, cancellationToken);
        }

        var query = _context.QuestionSets
            .Include(x => x.CreatedByUser)
            .Include(x => x.QuestionSetTags)
            .ThenInclude(t => t.Tag)
            .Where(x => x.VisibilityMode == QuestionSetVisibilityMode.Public);

        if (tagIds is { Count: > 0 })
        {
            query = query.Where(x => x.QuestionSetTags.Any(y => tagIds.Contains(y.TagId)));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{name}%"));
        }

        query = sortBy?.ToLower() == "newest"
            ? query.OrderByDescending(q => q.Created)
            : query.OrderByDescending(q =>
                (q.RatingCount + m) == 0
                    ? 0
                    : (q.RatingCount / (double)(q.RatingCount + m)) * q.RatingAverage +
                      (m / (double)(q.RatingCount + m)) * globalAverage);

        return await PaginatedList<QuestionSetForListResponseDto>.CreateAsync(
            query.Select(qs => new QuestionSetForListResponseDto
            {
                Id = qs.Id,
                Name = qs.Name,
                Description = qs.Description,
                TotalQuestionCount = qs.QuestionCount,
                CreateBy = qs.CreatedByUser != null
                    ? qs.CreatedByUser.FullName
                    : string.Empty,
                RatingCount = qs.RatingCount,
                RatingAverage = qs.RatingAverage,
                Tags = qs.QuestionSetTags
                    .Where(x => x.Tag != null)
                    .Select(x => new TagForListReponseDto
                    {
                        Id = x.TagId,
                        Name = x.Tag!.Name.Substring(0, 1)
                                   .ToUpper() +
                               x.Tag.Name.Substring(1),
                        QuestionSetCount = x.Tag.QuestionSetCount
                    })
                    .ToList(),
                VisibilityMode = qs.VisibilityMode.ToString(),
                CompletedQuestionCount = qs.QuestionCount
            }),
            pageNumber,
            pageSize
        );
    }


    public async Task<PaginatedList<QuestionSetForListResponseDto>> SearchOwnAndSharedQuestionSetsAsync(
        Guid userId,
        string? name,
        int pageNumber,
        int pageSize,
        string? filterBy,
        string? sortBy,
        CancellationToken cancellationToken)
    {
        var isFilterCreatedByMe = filterBy == "CreatedByMe";
        var isFilterSharedWithMe = filterBy == "ShareWithMe";

        var query = _context.QuestionSets
            .Include(q => q.CreatedByUser)
            .Include(q => q.QuestionSetUsers)
            .Include(q => q.AccessHistories)
            .Where(q => q.QuestionSetUsers.Any(qsu => qsu.UserId == userId));

        if (isFilterCreatedByMe)
        {
            query = query.Where(q =>
                q.QuestionSetUsers.Any(qsu => qsu.UserId == userId && qsu.ShareMode == QuestionSetUserShareMode.Owner));
        }
        else if (isFilterSharedWithMe)
        {
            query = query.Where(q =>
                q.QuestionSetUsers.Any(qsu => qsu.UserId == userId && qsu.ShareMode != QuestionSetUserShareMode.Owner));
        }

        if (!string.IsNullOrEmpty(name))
        {
            var keyword = $"%{name}%";
            query = query.Where(x => EF.Functions.ILike(x.Name, keyword));
        }

        query = query.Include(q => q.QuestionSetTags).ThenInclude(qst => qst.Tag);

        var projectedQuery = query
            .Select(q => new
            {
                QuestionSet = q,
                LastAccessedAt = q.AccessHistories
                    .Where(ah => ah.UserId == userId)
                    .Select(ah => (DateTimeOffset?)ah.LastAccess)
                    .FirstOrDefault(),
                IsOwner = q.QuestionSetUsers
                    .Any(qsu => qsu.UserId == userId && qsu.ShareMode == QuestionSetUserShareMode.Owner)
            });

        if (string.IsNullOrEmpty(filterBy))
        {
            projectedQuery = projectedQuery.Where(x =>
                x.IsOwner || (!x.IsOwner && x.LastAccessedAt != null));
        }
        else if (isFilterSharedWithMe)
        {
            projectedQuery = projectedQuery.Where(x => x.LastAccessedAt != null);
        }

        if (sortBy == "Newest")
        {
            projectedQuery = projectedQuery.OrderByDescending(x => x.QuestionSet.Created);
        }
        else
        {
            projectedQuery = projectedQuery
                .OrderBy(x => x.LastAccessedAt.HasValue ? 0 : 1)
                .ThenByDescending(x => x.LastAccessedAt);
        }

        return await PaginatedList<QuestionSetForListResponseDto>.CreateAsync(
            projectedQuery.Select(x => new QuestionSetForListResponseDto
            {
                Id = x.QuestionSet.Id,
                Name = x.QuestionSet.Name,
                Description = x.QuestionSet.Description,
                RatingAverage = x.QuestionSet.RatingAverage,
                RatingCount = x.QuestionSet.RatingCount,
                CreateBy = x.QuestionSet.CreatedByUser!.FullName,
                CreatedById = x.QuestionSet.CreatedByUser!.Id,
                CreatedAt = x.QuestionSet.Created,
                TotalQuestionCount = x.QuestionSet.QuestionCount,
                Tags = x.QuestionSet.QuestionSetTags
                    .Where(x => x.Tag != null)
                    .Select(x => new TagForListReponseDto
                    {
                        Id = x.TagId,
                        Name = x.Tag!.Name.Substring(0, 1)
                                   .ToUpper() +
                               x.Tag.Name.Substring(1),
                        QuestionSetCount = x.Tag.QuestionSetCount
                    })
                    .ToList(),
                LastAccessByMe = x.LastAccessedAt,
                VisibilityMode = x.QuestionSet.VisibilityMode.ToString(),
                CompletedQuestionCount = 0
            }),
            pageNumber,
            pageSize);
    }


}
