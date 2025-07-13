using System.Text.Json;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Utils;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets;

[Authorize]
public class UpdateQuestionSetCommand : IRequest<Guid>
{
    public Guid QuestionSetId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<CreateUpdateQuestionDto> CreateUpdateQuestions { get; set; } = new();
    public List<Guid> DeleteQuestionIds { get; set; } = new();
}

public class UpdateQuestionSetCommandValidator : AbstractValidator<UpdateQuestionSetCommand>
{
    public UpdateQuestionSetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên bộ câu hỏi không được để trống")
            .MaximumLength(200).WithMessage("Tên bộ câu hỏi không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả bộ câu hỏi không được để trống")
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.CreateUpdateQuestions)
            .Must(q => q != null && q.Count <= 500)
            .WithMessage("Bộ câu hỏi không được vượt quá 500 câu");

        RuleFor(x => x.DeleteQuestionIds)
            .Must(q => q.Count <= 500)
            .WithMessage("Không thể xóa toàn bộ câu hỏi");

        RuleForEach(x => x.CreateUpdateQuestions)
            .SetValidator((command, question) => new CreateUpdateQuestionDto.QuestionCreateDtoValidator());
    }
}

public class UpdateQuestionSetCommandHandler : IRequestHandler<UpdateQuestionSetCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _currentUser;

    public UpdateQuestionSetCommandHandler(
        IApplicationDbContext dbContext,
        IQuestionSetService questionSetService,
        IUser currentUser)
    {
        _dbContext = dbContext;
        _questionSetService = questionSetService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(UpdateQuestionSetCommand request, CancellationToken cancellationToken)
    {
        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);

        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);

        if (!await _questionSetService.CanUserEditQuestionSet(_currentUser.UserId!.Value, questionSet.Id))
            throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN);

        // Cập nhật thông tin
        questionSet.Name = request.Name!;
        questionSet.Description = request.Description!;

        var updateIds = request.CreateUpdateQuestions
            .Where(q => q.QuestionId.HasValue)
            .Select(q => q.QuestionId!.Value)
            .ToList();

        var targetIds = request.DeleteQuestionIds
            .Union(updateIds)
            .ToList();

        var existingQuestions = await _dbContext.Questions
            .Where(q =>
                q.QuestionSetId == request.QuestionSetId
                && targetIds.Contains(q.Id)
                && q.IsDeleted == false
            )
            .ToListAsync(cancellationToken);

        // Xóa các câu hỏi
        var questionsToDelete = existingQuestions
            .Where(q => request.DeleteQuestionIds.Contains(q.Id))
            .ToList();

        foreach (var question in questionsToDelete)
            question.IsDeleted = true;


        var questionCountAdd = 0 - questionsToDelete.Count;
        // Thêm hoặc cập nhật câu hỏi
        foreach (var dto in request.CreateUpdateQuestions)
        {
            var type = Enum.Parse<QuestionType>(dto.Type!);
            var dataJson = CreateUpdateQuestionDto.Serializer.Serialize(dto);

            if (dto.QuestionId.HasValue)
            {
                var existing = existingQuestions.FirstOrDefault(q => q.Id == dto.QuestionId.Value);
                if (existing != null)
                {
                    existing.Type = type;
                    existing.QuestionText = dto.QuestionText;
                    existing.Score = dto.Score;
                    existing.TextFormat = TextFormat.Html;
                    existing.DataJson = dataJson;
                    existing.ExplainText = dto.ExplainText;
                }
            }
            else
            {
                questionCountAdd++;
                questionSet.Questions.Add(new Question
                {
                    Id = Guid.NewGuid(),
                    QuestionSetId = questionSet.Id,
                    Type = type,
                    QuestionText = dto.QuestionText,
                    TextFormat = TextFormat.PlainText,
                    ExplainText = dto.ExplainText,
                    Score = dto.Score,
                    DataJson = dataJson
                });
            }
        }

        questionSet.QuestionCount = questionSet.QuestionCount + questionCountAdd;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return request.QuestionSetId;
    }
}
