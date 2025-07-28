namespace CleanArchitectureBase.Application.Plans;

public class CreatePlanCommand : IRequest<Guid>
{
    public string? Name { get; set; }
    public float? Price { get; set; }
    public int DayDuration { get; set; }
    public bool CanLearn { get; set; }
    public bool CanOpenTest { get; set; }
    public bool CanCopyOrImportQuestionSet { get; set; }
}
