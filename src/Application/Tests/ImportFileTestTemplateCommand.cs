using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class ImportedQuestionDto
{
    public List<CreateUpdateQuestionDto>? validQuestions { get; set; }
    public List<CreateUpdateQuestionDto>? invalidQuestions { get; set; }
}

[Authorize]
public class ImportFileTestTemplateCommand : IRequest<ImportedQuestionDto>
{
    public required FileStreamData FileData { get; set; }
}

public class ImportFileTestTemplateCommandValidator : AbstractValidator<ImportFileTestTemplateCommand>
{
    public ImportFileTestTemplateCommandValidator()
    {
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");

        RuleFor(x => x.FileData.Data)
            .NotNull().WithMessage("Dữ liệu stream không được trống")
            .Must(stream => stream!.Length > 0).WithMessage("Stream không được rỗng");
    }
}

public class ImportFileTestTemplateCommandHandler : IRequestHandler<ImportFileTestTemplateCommand, ImportedQuestionDto>
{
    private readonly IFileService _file;

    public ImportFileTestTemplateCommandHandler(IFileService file)
    {
        _file = file;
    }
    
    public Task<ImportedQuestionDto> Handle(ImportFileTestTemplateCommand rq, CancellationToken cancellationToken)
    {
        var file = rq.FileData;
        
        if (file == null || file.Data!.Length == 0)
        {
            throw new ErrorCodeException(ErrorCodes.FILE_NOT_FOUND, "Không tìm thấy file hoăc rỗng");
        }
        
        var fileFormat = Path.GetExtension(file.FileName);

        if (fileFormat != ".xlsx" && fileFormat != ".xls")
        {
            throw new ErrorCodeException(ErrorCodes.ERROR_FORMAT_FILE, "File không đúng định dạng");
        }

        var importQuestions = _file.GetQuestionFromFile(file);
        
        return importQuestions;
    }
}
