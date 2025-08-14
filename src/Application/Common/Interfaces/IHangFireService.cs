namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IHangFireService
{
    Task AutoSubmitTest(Guid attemptId, int timeLimit);
    Task DeleteJobByArgument(string content);
}
