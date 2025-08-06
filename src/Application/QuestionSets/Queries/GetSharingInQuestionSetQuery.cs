using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

[Authorize]
public class GetSharingInQuestionSetQuery : IRequest<ResourceShareDto>
{
    public Guid QuestionSetId { get; set; }
}

public class GetSharingInQuestionSetQueryValidator : AbstractValidator<GetSharingInQuestionSetQuery>
{
    public GetSharingInQuestionSetQueryValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được trống");
    }
}

public class GetSharingInQuestionSetQueryHandler : IRequestHandler<GetSharingInQuestionSetQuery, ResourceShareDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQuestionSetService _questionSetService;
    
    public GetSharingInQuestionSetQueryHandler(
        IApplicationDbContext context,
        IUser user,
        IQuestionSetService questionSetService)
    {
        _context = context;
        _user = user;
        _questionSetService = questionSetService;
    }
    
    public async Task<ResourceShareDto> Handle(GetSharingInQuestionSetQuery rq, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Include(q => q.Questions)
            .Where(qs => qs.Id.Equals(rq.QuestionSetId))
            .FirstOrDefaultAsync(cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        var canView = await _questionSetService.CanUserEditQuestionSet(_user.UserId!.Value, questionSet.Id);
        if (!canView)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET);
        
        var questionSetUsers = await _context.QuestionSetUsers
            .Include(x => x.User)
            .Include(x => x.QuestionSet)
            .Where(x => x.QuestionSetId.Equals(rq.QuestionSetId))
            .ToListAsync(cancellationToken);
        
        var sharedModes = new List<SharingModelDto>();

        questionSetUsers.ForEach(x => 
        sharedModes.Add(new SharingModelDto
            {
                ShareMode = x.ShareMode.ToString(),
                UserId = x.User!.Id,
                FullName = x.User.FullName
            })
        );
        
        var orderPriority = new List<string> { "Owner", "Editable", "ViewOnly" };

        return new ResourceShareDto
        {
            Id = rq.QuestionSetId,
            VisibilityMode = questionSet.VisibilityMode.ToString(),
            SharingModel = sharedModes
                .OrderBy(x => orderPriority.IndexOf(x.ShareMode!))
                .ToList()
        };
    }
}
