using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<User> DomainUsers { get; }
    public DbSet<Plan> Plans { get; }
    public DbSet<TokenPackage> TokenPackages { get; }
    public DbSet<UserTokenPurchase> UserTokenPurchases { get; }
    public DbSet<UserSubscription> UserSubscriptions { get; }
    public DbSet<Question> Questions { get; }
    public DbSet<QuestionSet> QuestionSets { get; }
    public DbSet<Comment> Comments { get; }
    public DbSet<UserQuestionSetHistory> UserQuestionSetHistories { get; }
    public DbSet<TestTemplate> TestTemplates { get; }
    public DbSet<TestTemplateQuestion> TestTemplateQuestions { get; }
    public DbSet<Folder> Folders { get; }
    public DbSet<FolderTestTemplate> FolderTestTemplates { get; }
    public DbSet<FolderUser> FolderUsers { get; }
    public DbSet<Test> Tests { get; }
    public DbSet<TestVersion> TestVersions { get; }
    public DbSet<TestVersionQuestion> TestVersionQuestions { get; }
    public DbSet<Attempt> Attempts { get; }
    public DbSet<AttemptQuestion> AttemptQuestions { get; }
    public DbSet<Class> Classes { get; }
    public DbSet<ClassUser> ClassUsers { get; }
    public DbSet<ClassQuestionSet> ClassQuestionSets { get; }
    public DbSet<ClassInvitation> ClassInvitations { get; }
    public DbSet<ClassInvitationUser> ClassInvitationUsers { get; }
    public DbSet<TestGrade> TestGrades { get; }
    public DbSet<QuestionSetUser> QuestionSetUsers { get; }
    public DbSet<Tag> Tags { get; }
    public DbSet<QuestionSetTag> QuestionSetTags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
