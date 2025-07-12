using System.Runtime.Serialization;
using System.Text.Json;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Utils;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets;

[Authorize]
public class CreateQuestionSetCommand : IRequest<Guid>
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<CreateUpdateQuestionDto> Questions { get; set; } = new();
}

public class CreateQuestionSetCommandValidator : AbstractValidator<CreateQuestionSetCommand>
{
    public CreateQuestionSetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên bộ câu hỏi không được để trống")
            .MaximumLength(200).WithMessage("Tên bộ câu hỏi không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả bộ câu hỏi không được để trống")
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Bộ câu hỏi phải chứa ít nhất một câu hỏi");

        RuleFor(x => x.Questions)
            .Must(q => q != null && q.Count <= 500)
            .WithMessage("Bộ câu hỏi không được vượt quá 500 câu");

        RuleForEach(x => x.Questions)
            .SetValidator((command, question) => new CreateUpdateQuestionDto.QuestionCreateDtoValidator());
    }
}

public class CreateQuestionSetCommandHandler : IRequestHandler<CreateQuestionSetCommand, Guid>
{

    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    public CreateQuestionSetCommandHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateQuestionSetCommand request, CancellationToken cancellationToken)
    {
        //Khởi tạo questionSet
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = request.Name!,
            Description = request.Description!,
            VisibilityMode = QuestionSetVisibilityMode.Private, //Để mặc định là private
            QuestionCount = request.Questions.Count,
            Questions = new List<Question>()
        };

        //Xử lý thêm các câu hỏi để đưa vào questionset
        foreach (var questionDto in request.Questions)
        {
            //Khởi tạo question
            var question = new Question
            {
                Id = Guid.NewGuid(),
                QuestionSetId = questionSet.Id,
                Type = Enum.Parse<QuestionType>(questionDto.Type!),
                QuestionText = questionDto.QuestionText,
                TextFormat = TextFormat.Html, 
                Score = questionDto.Score
            };

            // Chuyển các nội dung câu hỏi về JSON
            question.DataJson = CreateUpdateQuestionDto.Serializer.Serialize(questionDto);

            questionSet.Questions.Add(question);
        }

        _dbContext.QuestionSets.Add(questionSet);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return questionSet.Id;
    }
}
