using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Pdf;

public class GetPdfStructureQuery : IRequest<DocumentStructureDto>
{
    public required FileStreamData FileData { get; set; }
}


public class GetPdfStructureQueryCommandValidator : AbstractValidator<GetPdfStructureQuery>
{
    private const long MaxFileSizeInBytes = 15 * 1024 * 1024; // 50MB
    
    private const int MaxPageCount = 10000;

    public GetPdfStructureQueryCommandValidator(IPdfService pdfService)
    {
        IPdfService pdfService1 = pdfService;
        
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");

        RuleFor(x => x.FileData.Data)
            .NotNull().WithMessage("Dữ liệu stream không được trống")
            // .Must(stream => stream!.Length > 0).WithMessage("Stream không được rỗng")
            // .Must(stream => stream!.Length <= MaxFileSizeInBytes)
            // .WithMessage("Dung lượng tệp không được vượt quá 15MB")
            .Custom((stream, context) =>
            {
                if (stream == null || stream.Length == 0)
                    throw new ErrorCodeException(ErrorCodes.FILE_EMPTY);

                if (stream.Length > MaxFileSizeInBytes)
                    throw new ErrorCodeException(ErrorCodes.FILE_TOO_LARGE);
                try
                {
                    pdfService1.TrValidatePdf(stream!, MaxPageCount);
                }
                catch (ArgumentException ex)
                {
                    context.AddFailure(ex.Message);
                }
            });;
    }
}

public class GetPdfStructureQueryHandler : IRequestHandler<GetPdfStructureQuery, DocumentStructureDto>
{
    private readonly IPdfService _pdfService;
    
    public GetPdfStructureQueryHandler(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    public async Task<DocumentStructureDto> Handle(GetPdfStructureQuery request, CancellationToken cancellationToken)
    {
        var result = await _pdfService.ExtractStructure(request.FileData.Data!);
        
        return result;
    }
}
