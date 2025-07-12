using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

namespace CleanArchitectureBase.Application.Tests;

public class ImportedQuestionDto
{
    public List<CreateQuestionDto>? validQuestions { get; set; }
    public List<CreateQuestionDto>? invalidQuestions { get; set; }
}

[Authorize]
public class ImportFileTestTemplateCommand : IRequest<ImportedQuestionDto>
{
    public required IFormFile FileData { get; set; }
}

public class ImportFileTestTemplateCommandValidator : AbstractValidator<ImportFileTestTemplateCommand>
{
    public ImportFileTestTemplateCommandValidator()
    {
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");
    }
}

public class ImportFileTestTemplateCommandHandler : IRequestHandler<ImportFileTestTemplateCommand, ImportedQuestionDto>
{
    public async Task<ImportedQuestionDto> Handle(ImportFileTestTemplateCommand rq, CancellationToken cancellationToken)
    {
        var file = rq.FileData;
        
        if (file == null || file.Length == 0)
        {
            throw new ErrorCodeException(ErrorCodes.FILE_NOT_FOUND, "Không tìm thấy file hoăc rỗng");
        }
        
        var fileFormat = Path.GetExtension(file.FileName);

        if (fileFormat != ".xlsx" && fileFormat != ".xls")
        {
            throw new ErrorCodeException(ErrorCodes.ERROR_FORMAT_FILE, "File không đúng định dạng");
        }
        
        var validQuestions = new List<CreateQuestionDto>();
        var invalidQuestions = new List<CreateQuestionDto>();
        var validator = new CreateQuestionDto.QuestionCreateDtoValidator();
        
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            
            stream.Position = 0;
            
            // Đăng ký encoding provider để hỗ trợ 1252
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                reader.Read();
                
                var headers = new List<string>();
                
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    var headerValue = reader.GetValue(col)?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        headers.Add(headerValue);
                    }
                }
                
                if (!headers.Contains("Question Type") || !headers.Contains("Question Text") || !headers.Contains("Score"))
                {
                    throw new ErrorCodeException(ErrorCodes.INVALID_FILE_FORMAT, "File Excel không đúng định dạng: Thiếu cột bắt buộc");
                }

                while (reader.Read())
                {
                    var question = new CreateQuestionDto
                    {
                        MultipleChoices = new List<CreateMultipleChoiceDto>(),
                        MatchingPairs = new List<CreateMatchingPairDto>(),
                        OrderingItems = new List<CreateOrderingItemDto>(),
                        Score = 0
                    };
                    
                    question.Type = NormalizeQuestionType(reader.GetValue(headers.IndexOf("Question Type"))?.ToString());
                    question.QuestionText = reader.GetValue(headers.IndexOf("Question Text"))?.ToString();
                    
                    try
                    {
                        question.Score = Convert.ToSingle(reader.GetValue(headers.IndexOf("Score")) ?? 0);
                    }
                    catch (FormatException)
                    {
                        question.Score = -1; 
                    }
                    
                    var options = new List<string>();
                    for (int i = 1; i <= 10; i++)
                    {
                        var optionIndex = headers.IndexOf($"Option {i}");

                        if (optionIndex >= 0)
                        {
                            var optValue = reader.GetValue(optionIndex)?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(optValue))
                            {
                                options.Add(optValue);
                            }
                            else
                            {
                                options.Add("");
                            }
                        }
                    }

                    switch (question.Type)
                        {
                            case nameof(QuestionType.MultipleChoice):
                                foreach (var opt in options)
                                {
                                    bool isAnswer = opt.StartsWith("*");
                                    string text = isAnswer ? opt.Substring(1).Trim() : opt.Trim();
                                    if (text.Length > 0)
                                    {
                                        question.MultipleChoices.Add(new CreateMultipleChoiceDto { Text = text, IsAnswer = isAnswer });
                                    }
                                }
                                break;
                            
                            case nameof(QuestionType.Matching):
                                for (int i = 0; i < options.Count; i += 2)
                                {
                                    if (i + 1 < options.Count && options[i + 1].Length > 0 && options[i].Length > 0)
                                    {
                                        question.MatchingPairs.Add(new CreateMatchingPairDto
                                        {
                                            LeftItem = options[i],
                                            RightItem = options[i + 1]
                                        });
                                    }
                                }
                                break;
                            
                            case nameof(QuestionType.Ordering):
                                int order = 0;
                                
                                for (int i = 0; i < options.Count; i++)
                                {
                                    if (options[i].Length > 0)
                                    {
                                        question.OrderingItems.Add(new CreateOrderingItemDto { Text = options[i], CorrectOrder = order });

                                        order++;
                                    }
                                }
                                break;
                            
                            default:
                                invalidQuestions.Add(question);
                                continue;
                        }
                    
                    var validationResult = validator.Validate(question);
                    
                    if (validationResult.IsValid)
                    {
                        validQuestions.Add(question);
                    }
                    else
                    {
                        invalidQuestions.Add(question);
                    }
                    }
                }
            }
        
        return new ImportedQuestionDto
        {
            validQuestions = validQuestions,
            invalidQuestions = invalidQuestions
        };
    }
    
    private string? NormalizeQuestionType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return null;

        type = type.Trim();
        if (!Enum.TryParse<QuestionType>(type, ignoreCase: true, out _))
        {
            return null;
        }

        return type;
    }
}
