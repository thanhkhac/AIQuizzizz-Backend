using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Tests.Service;
using Hangfire;

namespace CleanArchitectureBase.Infrastructure.Hangfire;

public class HangFireService : IHangFireService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ITestService _testService;
    
    public HangFireService(IBackgroundJobClient backgroundJobClient, ITestService testService)
    {
        _backgroundJobClient = backgroundJobClient;
        _testService = testService;
    }
    
    public Task AutoSubmitTest(Guid attemptId, int timeLimit)
    {
        _backgroundJobClient.Schedule(
            () => _testService.AutoSubmit(attemptId),
            DateTime.Now.AddMinutes(timeLimit).AddSeconds(15)
        );
        
        return Task.CompletedTask;
    }

    public Task DeleteJobByArgument(string content)
    {
        var connection = JobStorage.Current.GetConnection();
        var monitoringApi = JobStorage.Current.GetMonitoringApi();

        var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue);
        
        foreach (var kv in scheduledJobs)
        {
            var jobId = kv.Key;
            var jobData = kv.Value.Job;

            if (jobData?.Args != null && jobData.Args.Any(a => a != null && a.ToString()!.Contains(content)))
            {
                BackgroundJob.Delete(jobId);
            }
        }
        
        return Task.CompletedTask;
    }
}
