using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

[Authorize]
public class GetLinkDownloadFileImportQuery : IRequest<string> { }

public class GetLinkDownloadFileImportQueryHandler : IRequestHandler<GetLinkDownloadFileImportQuery, string>
{
    public Task<string> Handle(GetLinkDownloadFileImportQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult("https://drive.google.com/uc?export=download&id=1N9q14ENsOJYmpCpQbxMhipd6aEYRJRVE");
    }
}

