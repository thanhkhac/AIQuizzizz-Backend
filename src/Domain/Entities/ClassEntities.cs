namespace CleanArchitectureBase.Domain.Entities;

public class Class : BaseAuditableEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsDeleted { get; set; }
    public List<ClassQuestionSet> ClassQuestionSets { get; set; } = new();
}

public enum ClassShareMode
{
    Student,
    Teacher,
    Owner
}

public class ClassUser : BaseAuditableEntity
{
    public required Guid ClassId { get; set; }
    public required Guid UserId { get; set; }
    public required ClassShareMode ShareMode { get; set; }

    public Class Class { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class ClassQuestionSet : BaseAuditableEntity
{
    public required Guid ClassId { get; set; }
    public required Guid QuestionSetId { get; set; }

    public Class Class { get; set; } = null!;
    public QuestionSet QuestionSet { get; set; } = null!;
}

public class ClassInvitation : BaseAuditableEntity
{
    public required Guid Id { get; set; }
    public required Guid ClassId { get; set; }
    public required string Code { get; set; }
    public required DateTime TimeStart { get; set; }
    public required DateTime TimeEnd { get; set; }
    public required bool IsDeleted { get; set; }

    public Class Class { get; set; } = null!;
}

public class ClassInvitationUser : BaseEntity
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid ClassInvitationId { get; set; }
    public required DateTime TimeJoin { get; set; }

    public User User { get; set; } = null!;
    public ClassInvitation ClassInvitation { get; set; } = null!;
}
