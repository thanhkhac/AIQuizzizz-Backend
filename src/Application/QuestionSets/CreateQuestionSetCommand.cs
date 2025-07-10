using System.Runtime.Serialization;
using System.Text.Json;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Questions.Utils;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets;

public class CreateQuestionSetCommand : IRequest<Guid>
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<CreateQuestionDto> Questions { get; set; } = new ();
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
            .NotEmpty().WithMessage("Bộ câu hỏi phải chứa ít nhất một câu hỏi")
            .Must(questions => questions.All(IsValidQuestionType))
            .WithMessage("Một hoặc nhiều câu hỏi có loại hoặc dữ liệu không hợp lệ");
            
        RuleFor(x => x.Questions)
            .Must(q => q != null && q.Count <= 500)
            .WithMessage("Bộ câu hỏi không được vượt quá 500 câu");

        RuleForEach(x => x.Questions)
            .SetValidator((command, question) => new QuestionCreateDtoValidator());
    }

    private bool IsValidQuestionType(CreateQuestionDto createQuestion)
    {
        bool isValid = createQuestion.Type switch
        {
            nameof(QuestionType.MultipleChoice) => createQuestion.MultipleChoices != null && createQuestion.MultipleChoices.Any(),
            nameof(QuestionType.Matching) => createQuestion.MatchingPairs != null && createQuestion.MatchingPairs.Any(),
            nameof(QuestionType.Ordering) => createQuestion.OrderingItems != null && createQuestion.OrderingItems.Any(),
            nameof(QuestionType.ShortText) => !string.IsNullOrWhiteSpace(createQuestion.ShortAnswer),
            _ => false
        };

        return isValid;
    }
}

public class QuestionCreateDtoValidator : AbstractValidator<CreateQuestionDto>
{
    public QuestionCreateDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage($"Loại câu hỏi không được để trống")
            .Must(type => new[] { "MultipleChoice", "Matching", "Ordering", "ShortText" }.Contains(type))
            .WithMessage($"Loại câu hỏi phải là 'MultipleChoice', 'Matching', 'Ordering','ShortText'");

        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage($"Nội dung câu hỏi không được để trống")
            .MaximumLength(1000).WithMessage($"Nội dung câu hỏi không được vượt quá 500 ký tự"); //Tăng số ký tự cho phép cho câu hỏi

        RuleFor(x => x.ExplainText)
            .MaximumLength(1000).WithMessage($" Giải thích không được vượt quá 1000 ký tự");

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage($"Điểm phải lớn hơn hoặc bằng 0")
            .LessThanOrEqualTo(100).WithMessage($"Điểm không được vượt quá 100");
        
        RuleFor(x => x.QuestionId)
            .Must(id => !id.HasValue || (id.Value != Guid.Empty && id.Value != default(Guid)))
            .WithMessage("QuestionId phải là một Guid hợp lệ nếu được cung cấp");

        // Validate cho MultipleChoice
        When(x => x.Type == "MultipleChoice", () =>
        {
            RuleFor(x => x.MultipleChoices)
                .NotEmpty().WithMessage("Phải có ít nhất 2 lựa chọn cho câu hỏi trắc nghiệm")
                .Must(options => options is { Count: >= 2 }).WithMessage("Phải có ít nhất 2 lựa chọn")
                .Must(options => options != null && options.Any(o => o.IsAnswer)).WithMessage($"Phải có ít nhất một lựa chọn đúng");

            RuleForEach(x => x.MultipleChoices)
                .ChildRules((options) =>
                {
                    options.RuleFor(o => o.Text)
                        .NotEmpty().WithMessage($"Nội dung không được để trống")
                        .MaximumLength(200).WithMessage($"Nội dung không được vượt quá 200 ký tự");
                });
        });

        // Validate cho Matching
        When(x => x.Type == "Matching", () =>
        {
            RuleFor(x => x.MatchingPairs)
                .NotEmpty().WithMessage("Phải có ít nhất 2 cặp ghép đôi")
                .Must(items => items != null && items.Count >= 2).WithMessage("Phải có ít nhất 2 cặp ghép đôi")
                ;

            RuleForEach(x => x.MatchingPairs)
                .ChildRules((items) =>
                {
                    items.RuleFor(i => i.LeftItem)
                        .NotEmpty().WithMessage("Mục bên trái không được để trống")
                        .MaximumLength(1000).WithMessage("Mục bên trái không được vượt quá 1000 ký tự");
                    items.RuleFor(i => i.RightItem)
                        .NotEmpty().WithMessage("Mục bên phải không được để trống")
                        .MaximumLength(1000).WithMessage($"Mục bên phải không được vượt quá 1000 ký tự");
                });
        });

        // Validate cho Ordering
        When(x => x.Type == "Ordering", () =>
        {
            RuleFor(x => x.OrderingItems)
                .NotEmpty().WithMessage("Câu hỏi Phải có ít nhất 2 mục để sắp xếp")
                .Must(items => items != null && items.Count >= 2).WithMessage("Phải có ít nhất 2 mục để sắp xếp")
                .Must(items => items != null && items.Select(i => i.CorrectOrder).Distinct().Count() == items.Count)
                .WithMessage("Các thứ tự đúng phải là duy nhất")
                .Must(items => items != null && items.All(i => i.CorrectOrder >= 0 && i.CorrectOrder < items.Count))
                .WithMessage("Thứ tự đúng phải nằm trong khoảng từ 0 đến n -1 (Tức là phần tử )");

            RuleForEach(x => x.OrderingItems)
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.Text)
                        .NotEmpty().WithMessage($"Nội dung không được để trống")
                        .MaximumLength(200).WithMessage($"Nội dung không được vượt quá 200 ký tự");
                });
        });

        When(x => x.Type == "ShortText", () =>
        {
            RuleFor(x => x.ShortAnswer)
                .NotEmpty().WithMessage($"Đáp án không được để trống")
                .MaximumLength(500).WithMessage($"Đáp án không được vượt quá 500 ký tự");
        });
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
                TextFormat = TextFormat.PlainText, // Có thể thay đổi theo yêu cầu
                Score = questionDto.Score
            };

            // Chuyển các nội dung câu hỏi về JSON
            question.DataJson = questionDto.Type switch
            {
                nameof(QuestionType.MultipleChoice) when questionDto.MultipleChoices != null => QuestionTypeSerializer.SerializeMultipleChoice(questionDto.MultipleChoices),
                nameof(QuestionType.Matching) when questionDto.MatchingPairs != null => QuestionTypeSerializer.SerializeMatchingPairs(questionDto.MatchingPairs),
                nameof(QuestionType.Ordering) when questionDto.OrderingItems != null => QuestionTypeSerializer.SerializeOrderingItems(questionDto.OrderingItems),
                nameof(QuestionType.ShortText) when !string.IsNullOrWhiteSpace(questionDto.ShortAnswer) => JsonSerializer.Serialize(
                    new QTypeShortAnswer { Answer = questionDto.ShortAnswer }),
                _ => throw new InvalidDataException($"Dữ liệu câu hỏi không hợp lệ cho loại câu hỏi: {questionDto.Type}")
            };

            questionSet.Questions.Add(question);
        }

        _dbContext.QuestionSets.Add(questionSet);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return questionSet.Id;
    }
}
