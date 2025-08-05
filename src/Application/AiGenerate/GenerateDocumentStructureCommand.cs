using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.AiGenerate;

public class GenerateDocumentStructureCommand : IRequest<DocumentStructureDto>
{
    public required FileStreamData FileData { get; set; }
}

public class GenerateDocumentStructureCommandValidator : AbstractValidator<GenerateDocumentStructureCommand>
{
    private const long MaxFileSizeInBytes = 50 * 1024 * 1024; // 50MB

    public GenerateDocumentStructureCommandValidator()
    {
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");

        RuleFor(x => x.FileData.Data)
            .NotNull().WithMessage("Dữ liệu stream không được trống")
            .Must(stream => stream!.Length > 0).WithMessage("Stream không được rỗng")
            .Must(stream => stream!.Length <= MaxFileSizeInBytes)
            .WithMessage("Dung lượng tệp không được vượt quá 50MB");
    }
}

public class GenerateDocumentStructureCommandHandler : IRequestHandler<GenerateDocumentStructureCommand, DocumentStructureDto>
{
    private readonly IAiGenerateService _aiGenerateService;
    public GenerateDocumentStructureCommandHandler(IAiGenerateService aiGenerateService)
    {
        _aiGenerateService = aiGenerateService;
    }

    public async Task<DocumentStructureDto> Handle(GenerateDocumentStructureCommand request, CancellationToken cancellationToken)
    {
        var prompt = @"
            Bối cảnh:
            - Bạn là một chuyên gia phân tích tài liệu
            - Tôi sẽ cung cấp nội dung văn bản trích từ một tài liệu PDF. 
            - Tài liệu có thể có hoặc không có từ “Chương”, “Phần”, v.v.

            Yêu cầu:
            - Chỉ trả về kết quả JSON, không thêm giải thích, chú thích hoặc văn bản khác.
            - Hãy phân tích và trích xuất các tiêu đề chính của tài liệu, tương đương các mục lớn trong cấu trúc học thuật
            - Trích xuất theo dạng cây phân cấp tối đa 3 cấp, nhưng không bắt buộc phải đủ 3 cấp.
            - Bao gồm các tiêu đề thường xuất hiện đầu dòng như:
              - “I.”, “II.”, “III.” (La Mã)
              - “1.”, “2.”, “3.” (số thường)
              - Dòng viết hoa toàn bộ, đứng riêng
            - Bỏ qua các tiêu đề nhỏ như “a)”, “Ví dụ”, chú thích, trích dẫn

            Yêu cầu:
            - Không tạo thêm cấp nếu không có thông tin rõ ràng để phân cấp.
            - Không lấy tên tài liệu

            Trả về kết quả dưới dạng JSON dạng cây như sau:
            {
              ""children"": [
                {
                  ""title"": """",
                  ""children"": [
                    {
                      ""title"": """",
                      ""children"": [
                        { ""title"": """" ,
                            children:
                        }
                      ]
                    }
                  ]
                }
              ]
            }

            Nếu không phát hiện được bất kỳ tiêu đề hợp lệ nào, hoặc tài liệu vô nghĩa, hãy trả về JSON sau
            { ""errorCode"": ""NO_STRUCTURE_FOUND"" }
        ";

        var result = await _aiGenerateService.SendPromptWithFileAsync(
            fileData: request.FileData,
            prompt: prompt,
            cancellationToken: cancellationToken);

        //Xử lý chuỗi
        int startIndex = result.IndexOf('{');
        int endIndex = result.LastIndexOf('}');

        string normalizedResult = (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
            ? result.Substring(startIndex, endIndex - startIndex + 1)
            : string.Empty;


        try
        {
            var jsonDoc = JsonDocument.Parse(normalizedResult);
            if (jsonDoc.RootElement.TryGetProperty("errorCode", out var errorCode))
            {
                if (errorCode.GetString() == "NO_STRUCTURE_FOUND")
                {
                    throw new ErrorCodeException(ErrorCodes.NO_STRUCTURE_FOUND);
                }
            }
        }
        catch (JsonException) { }


        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var document = JsonSerializer.Deserialize<DocumentStructureDto>(normalizedResult, options);
            if (document == null)
            {
                throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
            }
            Console.WriteLine(JsonSerializer.Serialize(document, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

            return document;
        }
        catch (Exception)
        {
            throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
        }
    }
}
