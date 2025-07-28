using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Application.Tags.Dto;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

public class GetQuestionSetDetailQuery : IRequest<QuestionSetDetailDto>
{
    public Guid QuestionSetId { get; set; }
}

public class GetQuestionSetDetailQueryHandler : IRequestHandler<GetQuestionSetDetailQuery, QuestionSetDetailDto>
{
    private IQuestionSetService _questionSetService;
    private IUser _user;
    private IApplicationDbContext _context;

    public GetQuestionSetDetailQueryHandler(IQuestionSetService questionSetService, IUser user, IApplicationDbContext context)
    {
        _questionSetService = questionSetService;
        _user = user;
        _context = context;
    }

    public async Task<QuestionSetDetailDto> Handle(GetQuestionSetDetailQuery request, CancellationToken cancellationToken)
    {
        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null) throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);

        var canView = await _questionSetService.CanUserViewQuestionSet(_user.UserId, questionSet);
        if (canView == false) throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You are not allowed to view this question set");

        if (_user.UserId != null)
        {
            var existingHistory = await _context.UserQuestionSetAccessHistories
                .FirstOrDefaultAsync(x => x.UserId == _user.UserId && x.QuestionSetId == questionSet.Id, cancellationToken);

            if (existingHistory != null)
            {
                existingHistory.LastAccess = DateTime.UtcNow;
            }
            else
            {
                _context.UserQuestionSetAccessHistories.Add(new Domain.Entities.UserQuestionSetAccessHistory
                {
                    UserId = _user.UserId.Value,
                    QuestionSetId = questionSet.Id,
                    LastAccess = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        var tags = await _context.QuestionSetTags.Include(x => x.Tag)
            .Where(x => x.QuestionSetId == request.QuestionSetId)
            .Where(x => x.Tag != null)
            .Select(x => new TagForListReponseDto
            {
                Id = x.TagId,
                Name = x.Tag!.Name,
                QuestionSetCount = x.Tag.QuestionSetCount
            })
            .ToListAsync(cancellationToken);

        foreach (var tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag.Name))
            {
                tag.Name = char.ToUpper(tag.Name[0]) + tag.Name.Substring(1);
            }
        }

        QuestionSetDetailDto result = new QuestionSetDetailDto
        {
            Id = questionSet.Id,
            Name = questionSet.Name,
            Description = questionSet.Description,
            VisibilityMode = questionSet.VisibilityMode.ToString(),
            QuestionCount = questionSet.QuestionCount,
            IsDeleted = questionSet.IsDeleted,
            CreatedAt = questionSet.Created,
            CreatedBy = new CreatedByDto
            {
                Id = questionSet.CreatedByUser!.Id,
                FullName = questionSet.CreatedByUser!.FullName,
                Email = questionSet.CreatedByUser!.Email,
            },
            Tags = tags
        };

        return result;
    }
}
