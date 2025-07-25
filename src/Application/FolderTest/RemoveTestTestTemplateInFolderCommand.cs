using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Application.TestTemplates.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.FolderTest;

[Authorize]
public class RemoveTestTestTemplateInFolderCommand : IRequest<Guid>
{
    public Guid FolderId { get; set; }
    public Guid TestTemplateId { get; set; } 
}

public class RemoveTestTestTemplateInFolderCommandValidator : AbstractValidator<AddTestTemplateToFolderCommand>
{
    public RemoveTestTestTemplateInFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được trống");
        
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("TestTemplateId không được trống");
    }
}

public class RemoveTestTestTemplateInFolderCommandHandler : IRequestHandler<RemoveTestTestTemplateInFolderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;
    private readonly ITestTemplateService _testTemplateService;

    public RemoveTestTestTemplateInFolderCommandHandler(
        IApplicationDbContext context,
        IFolderTestService folderTestService,
        ITestTemplateService testTemplateService)
    {
        _context = context;
        _folderTestService = folderTestService;
        _testTemplateService = testTemplateService;
    }

    public async Task<Guid> Handle(RemoveTestTestTemplateInFolderCommand rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Where(x => x.Id == rq.FolderId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (folder == null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_NOT_FOUND, "Không tìm thấy folder");
        
        var testTemplate = await _context.TestTemplates
            .Where(x => x.Id == rq.TestTemplateId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "Test template không tồn tại");
        
        if (!await _folderTestService.IsOwnerOrEditor(rq.FolderId, cancellationToken))
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER,
                "User không có quyền trong folder");
        
        var templateFolder = await _context.FolderTestTemplates
            .Where(x => x.FolderId == rq.FolderId && x.TestTemplateId == rq.TestTemplateId)
            .FirstOrDefaultAsync(cancellationToken);
        if(templateFolder == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND_IN_FOLDER, "Test template không tồn tại trong folder");
        
        _context.FolderTestTemplates.Remove(templateFolder);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return testTemplate.Id;
    }
}
