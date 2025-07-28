using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest;

[Authorize]
public class AddTestTemplateToFolderCommand : IRequest<Guid>
{
    public Guid FolderId { get; set; }
    public Guid TestTemplateId { get; set; }
}

public class AddTestTemplateToFolderCommandValidator : AbstractValidator<AddTestTemplateToFolderCommand>
{
    public AddTestTemplateToFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được trống");
        
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("TestTemplateId không được trống");
    }
}

public class AddTestTemplateToFolderCommandHandler : IRequestHandler<AddTestTemplateToFolderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;

    public AddTestTemplateToFolderCommandHandler(
        IApplicationDbContext context,
        IFolderTestService folderTestService)
    {
        _context = context;
        _folderTestService = folderTestService;
    }

    public async Task<Guid> Handle(AddTestTemplateToFolderCommand rq, CancellationToken cancellationToken)
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
        if(templateFolder != null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_ALREADY_EXISTS_IN_FOLDER, "Test template đã tồn tại trong folder");
        
        await _folderTestService.TryCanUseTesTemplate(testTemplate.Id);

        var folderTestTemplate = new FolderTestTemplate { TestTemplateId = rq.TestTemplateId, FolderId = rq.FolderId, };

        _context.FolderTestTemplates.Add(folderTestTemplate);

        await _context.SaveChangesAsync(cancellationToken);
        
        return testTemplate.Id;
    }
}
