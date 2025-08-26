using System.Net.Http.Headers;
using System.Text.Json;
using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using UglyToad.PdfPig;

namespace CleanArchitectureBase.Infrastructure.File;

public class PdfService : IPdfService
{
    private readonly HttpClient _httpClient;
    private readonly string url;

    public PdfService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        url = "";
        if (OperatingSystem.IsLinux())
        {
            url = "http://pdf-outline-app:1234";
        }
        else if (OperatingSystem.IsWindows())
        {
            url = "http://localhost:1234";
        }
    }


    public async Task<DocumentStructureDto> ExtractStructure(Stream pdfStream)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(pdfStream);

        form.Add(fileContent, "file", "file.pdf"); // "file" phải trùng với request.files['file']

        var response = await _httpClient.PostAsync(url + "/outline", form);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new DocumentStructureDto();
        }

        return JsonSerializer.Deserialize<DocumentStructureDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new DocumentStructureDto();
    }

    public void TrValidatePdf(Stream pdfStream, int maxPageCount)
    {
        if (pdfStream == null || pdfStream.Length == 0)
            throw new ErrorCodeException(ErrorCodes.INVALID_FILE_TYPE);

        if (!IsPdf(pdfStream))
            throw new ErrorCodeException(ErrorCodes.INVALID_FILE_TYPE, "Không phải PDF");

        try
        {
            pdfStream.Position = 0;
            using (var pdf = PdfDocument.Open(pdfStream, new ParsingOptions()
                   {
                       SkipMissingFonts = true
                   }))
            {
                if (pdf.NumberOfPages > maxPageCount)
                    throw new ErrorCodeException(ErrorCodes.PDF_PAGE_LIMIT_EXCEEDED);
            }

        }
        catch (Exception)
        {
            throw new ErrorCodeException(ErrorCodes.INVALID_FILE_FORMAT, "Sai định dạng file");
        }finally
        {
            pdfStream.Position = 0;
        }

    }

    bool IsPdf(Stream stream)
    {
        stream.Position = 0;
        byte[] buffer = new byte[4];
        stream.ReadExactly(buffer, 0, buffer.Length);
        stream.Position = 0;

        // 0x: tiền tố của số heej thâập lục phân
        // Tra bảng: https://www.asciitable.com/
        // 2550 4446 2d31 2e34 0a25 c3ad c3ac c2a6
        return buffer[0] == 0x25 && //%
               buffer[1] == 0x50 && //P
               buffer[2] == 0x44 && //D
               buffer[3] == 0x46; //F
    }
}
