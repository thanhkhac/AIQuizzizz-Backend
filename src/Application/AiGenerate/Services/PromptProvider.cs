using CleanArchitectureBase.Application.AiGenerate.Dtos;

namespace CleanArchitectureBase.Application.AiGenerate.Services;

public static class PromptProvider
{
    public static string GetGenerateDocumentStructureSystemInstructionPrompt()
    {
        var systemInstruction = @"
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

        return systemInstruction;
    }

    public static string GetGenerateQuestionInstructionSystemPrompt(bool isGenerateExplain, string language)
    {
        var explainField = isGenerateExplain ? @"""""explainText"""": """"Giải thích tại sao đáp án đúng""""," : "";
        var explainFieldRule = isGenerateExplain
            ? @"explainText là giải thích và trích dẫn nguồn tài liệu và phần nào trong tài liệu đó như là 
                ""<Trích dẫn nếu có> 
                -<Mục:...Tài liệu: <Tên tài liệu>>-"""
            : "";

        var systemInstruction = $@"
        Bạn là một hệ thống sinh câu hỏi tự động từ tài liệu học thuật.
        Chỉ trả về mảng JSON hợp lệ, không được thêm bất kỳ văn bản, mô tả, tiêu đề, hoặc định dạng markdown nào.
        Ngôn ngữ bắt buộc của tất cả câu hỏi và đáp án: {language}.

        Hãy tạo câu hỏi dưới dạng JSON, mỗi câu hỏi tuân theo các quy tắc sau đây:

        [
          {{
            ""type"": ""MultipleChoice"",
            ""questionText"": ""Câu hỏi trắc nghiệm là gì?"",
            {explainField}
            ""multipleChoices"": [
              {{ ""text"": ""Đáp án đúng"", ""isAnswer"": true }}, 
              {{ ""text"": ""Đáp án sai"" }}
            ]
          }},
          {{
            ""type"": ""Matching"",
            ""questionText"": ""Nối cặp đúng"",
           {explainField}
            ""matchingPairs"": [
              {{ ""leftItem"": ""Hà Nội"", ""rightItem"": ""Việt Nam"" }},
              {{ ""leftItem"": ""Paris"", ""rightItem"": ""Pháp"" }}
            ]
          }},
          {{
            ""type"": ""Ordering"",
            ""questionText"": ""Sắp xếp các bước theo đúng thứ tự"",
             {explainField}
            ""orderingItems"": [
              {{ ""text"": ""Bước 1"", ""correctOrder"": 0 }},
              {{ ""text"": ""Bước 2"", ""correctOrder"": 1 }}
            ]
          }},
          {{
            ""type"": ""ShortText"",
            ""questionText"": ""Trả lời ngắn gọn"",
             {explainField}
            ""shortAnswer"": ""Đáp án đúng""
          }}
        ]
        
        Đặc tả nội dung:
        questionText là nội dung câu hỏi
        {explainFieldRule}
        
        Các ràng buộc cho JSON:
        Type bắt buộc phải là ""MultipleChoice"" | ""Matching"" | ""Ordering"" | ""ShortText""
        questionText không vượt quá 5000 ký tự
        explainText không vượt quá 5000 ký tự
        multipleChoices[].text không vượt quá 1000
        multipleChoices array không được vượt quá 4 phần tử, phải có ít nhất một lựa chọn đúng, có thể có nhiều lựa chọn đúng
        matchingPairs: leftItem và rightItem không vượt quá 1000 ký tự, array không vượt quá 5 cặp
        orderingItems: text không được vượt quá 1000 ký tự, array không được vượt quá 10 phần tử, correctOrder phải bắt đầu từ 0
        shortAnswer: không vượt quá 1000 ký tự

        ";
        return systemInstruction.Trim();
    }

    public static string GetGenerateQuestionPrompt(
        DocumentStructureDto? documentStructure,
        DocumentStructureDto? selectedParts,
        List<string> selectedQuesiontype,
        int questionCount,
        bool isGenerateExplain)
    {
        string BuildStructureJson(DocumentStructureDto dto)
        {
            return System.Text.Json.JsonSerializer.Serialize(dto,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }

        var fullStructureJson = documentStructure != null ? BuildStructureJson(documentStructure) : "";
        var selectedStructureJson = selectedParts != null ? BuildStructureJson(selectedParts) : "";
        var questionTypes = string.Join(", ", selectedQuesiontype);
        var documentName = isGenerateExplain ? documentStructure!.Title : "";  
        var prompt = $@"
            {documentName}
            
            Đây là outline của tài liệu dưới dạng JSON:
            {fullStructureJson}

            Danh sách các mục mà người dùng đã chọn để tạo câu hỏi:
            {selectedStructureJson}

            Số lượng câu hỏi cần tạo: {questionCount}
            Loại câu hỏi cần tạo: {questionTypes}

            Hãy tạo câu hỏi dưới dạng JSON, mỗi câu hỏi tuân theo các quy tắc đã được mô tả trong instruction system prompt.
            ";
        return prompt.Trim();
    }

}
