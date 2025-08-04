namespace CleanArchitectureBase.Application.Plans.Dto;

public class PlanDetailDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int Price { get; set; }
    public int Duration { get; set; }
    public string? Unit { get; set; }
    public bool CanLearn { get; set; }
    public bool CanOpenTest { get; set; }
    public bool CanCopyOrImportQuestionSet { get; set; }
    public bool IsActive { get; set; }
}
