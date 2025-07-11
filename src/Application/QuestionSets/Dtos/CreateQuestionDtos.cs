using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Dtos;

public class CreateMultipleChoiceDto
{
    public string? Text { get; set; }
    public bool IsAnswer { get; set; }
}

public class CreateMatchingPairDto
{
    public string? LeftItem { get; set; }
    public string? RightItem { get; set; }
}

public class CreateOrderingItemDto
{
    public string? Text { get; set; }
    public int CorrectOrder { get; set; }
}

public class CreateQuestionDto
{
    public Guid? QuestionId { get; set; }
    public string? Type { get; set; }
    public string? QuestionText { get; set; }
    public string? ExplainText { get; set; } //TODO: Thêm trường explain cho entity
    public required float Score { get; set; }
    public List<CreateMultipleChoiceDto>? MultipleChoices { get; set; }
    public List<CreateMatchingPairDto>? MatchingPairs { get; set; }
    public List<CreateOrderingItemDto>? OrderingItems { get; set; }
    public string? ShortAnswer { get; set; }

    public class QuestionCreateDtoValidator : AbstractValidator<CreateQuestionDto>
    {
        public QuestionCreateDtoValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage($"Loại câu hỏi không được để trống")
                .Must(type => new[]
                {
                    "MultipleChoice", "Matching", "Ordering", "ShortText"
                }.Contains(type))
                .WithMessage($"Loại câu hỏi phải là 'MultipleChoice', 'Matching', 'Ordering','ShortText'");
                
            RuleFor(x => x)
                .Must(question => question.Type switch
                {
                    nameof(QuestionType.MultipleChoice) => question.MultipleChoices != null && question.MultipleChoices.Any(),
                    nameof(QuestionType.Matching) => question.MatchingPairs != null && question.MatchingPairs.Any(),
                    nameof(QuestionType.Ordering) => question.OrderingItems != null && question.OrderingItems.Any(),
                    nameof(QuestionType.ShortText) => !string.IsNullOrWhiteSpace(question.ShortAnswer),
                    _ => false
                })
                .WithMessage("Một hoặc nhiều câu hỏi có loại hoặc dữ liệu không hợp lệ");


            RuleFor(x => x.QuestionText)
                .NotEmpty().WithMessage($"Nội dung câu hỏi không được để trống")
                .MaximumLength(5000).WithMessage($"Nội dung câu hỏi không được vượt quá 500 ký tự"); //Tăng số ký tự cho phép cho câu hỏi

            RuleFor(x => x.ExplainText)
                .MaximumLength(5000).WithMessage($" Giải thích không được vượt quá 1000 ký tự");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage($"Điểm phải lớn hơn hoặc bằng 0")
                .LessThanOrEqualTo(1000).WithMessage($"Điểm không được vượt quá 100");

            RuleFor(x => x.QuestionId)
                .Must(id => !id.HasValue || (id.Value != Guid.Empty && id.Value != default(Guid)))
                .WithMessage("QuestionId phải là một Guid hợp lệ");

            // Validate cho MultipleChoice
            When(x => x.Type == "MultipleChoice", () =>
            {
                RuleFor(x => x.MultipleChoices)
                    .NotEmpty().WithMessage("Phải có ít nhất 2 lựa chọn cho câu hỏi trắc nghiệm")
                    .Must(options => options is { Count: >= 2 }).WithMessage("Phải có ít nhất 2 lựa chọn")
                    .Must(options => options is { Count: <= 10 }).WithMessage("Không được quá 10 lựa chọn")
                    .Must(options => options != null && options.Any(o => o.IsAnswer)).WithMessage($"Phải có ít nhất một lựa chọn đúng");

                RuleForEach(x => x.MultipleChoices)
                    .ChildRules((options) =>
                    {
                        options.RuleFor(o => o.Text)
                            .NotEmpty().WithMessage($"Nội dung không được để trống");
                            // .MaximumLength(500).WithMessage($"Nội dung không được vượt quá 200 ký tự");
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
}
