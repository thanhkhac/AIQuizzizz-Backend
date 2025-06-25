// File: Data/Configurations/PlanConfiguration.cs

using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureBase.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        //Cấu hình thuộc tính
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Price)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.DayDuration)
            .IsRequired();

        builder.Property(p => p.LearnMode)
            .IsRequired();

        builder.Property(p => p.OpenTest)
            .IsRequired();

        builder.Property(p => p.CopyQuestionSet)
            .IsRequired();

        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
    }
}

public class TokenPackageConfiguration : IEntityTypeConfiguration<TokenPackage>
{
    public void Configure(EntityTypeBuilder<TokenPackage> builder)
    {
        //Cấu hình thuộc tính
        builder.Property(tp => tp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.Price)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(tp => tp.TokenCount)
            .IsRequired();

        builder.Property(tp => tp.IsDeleted)
            .HasDefaultValue(false);
    }
}

//Cấu hình bảng trung gian giữa User và Token Package
public class UserTokenPurchaseConfiguration : IEntityTypeConfiguration<UserTokenPurchase>
{
    public void Configure(EntityTypeBuilder<UserTokenPurchase> builder)
    {
        builder.HasOne(utp => utp.TokenPackage)
            .WithMany()
            .HasForeignKey(utp => utp.TokenPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(utp => utp.User)
            .WithMany()
            .HasForeignKey(utp => utp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

//Cấu hình bảng trung gian giữa người dùng và Plan
public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.Property(us => us.DateStart)
            .IsRequired();

        builder.Property(us => us.DateFinish)
            .IsRequired();

        builder.Property(us => us.IsActive)
            .HasDefaultValue(false);

        builder.HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.Plan)
            .WithMany()
            .HasForeignKey(us => us.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.Property(q => q.Type)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (QuestionType)Enum.Parse(typeof(QuestionType), v));

        builder.Property(q => q.QuestionText)
            .HasMaxLength(1000);

        builder.Property(q => q.TextFormat)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (TextFormat)Enum.Parse(typeof(TextFormat), v));

        builder.Property(q => q.Score)
            .HasColumnType("numeric(5,2)");

        builder.Property(q => q.DataJson)
            .IsRequired()
            .HasColumnType("json");

        builder.Ignore(q => q.Data);

        //Một question thuộc về một QuestionSet
        builder.HasOne(q => q.QuestionSet)
            .WithMany(qs => qs.Questions)
            .HasForeignKey(q => q.QuestionSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionSetConfiguration : IEntityTypeConfiguration<QuestionSet>
{
    public void Configure(EntityTypeBuilder<QuestionSet> builder)
    {
        builder.Property(qs => qs.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(qs => qs.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(qs => qs.VisibilityMode)
            .HasConversion(
                v => v.ToString(),
                v => (QuestionSetVisibilityMode)Enum.Parse(typeof(QuestionSetVisibilityMode), v));

        builder.Property(qs => qs.QuestionCount)
            .HasDefaultValue(0);

        //Một QuestionSet có nhiều question
        builder.HasMany(qs => qs.Questions)
            .WithOne(q => q.QuestionSet)
            .HasForeignKey(q => q.QuestionSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionSetTagConfiguration : IEntityTypeConfiguration<QuestionSetTag>
{
    public void Configure(EntityTypeBuilder<QuestionSetTag> builder)
    {
        //composite key
        builder.HasKey(qst => new { qst.TagId, qst.QuestionSetId });

        builder.HasOne(qst => qst.Tag)
            .WithMany()
            .HasForeignKey(qst => qst.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(qst => qst.QuestionSet)
            .WithMany()
            .HasForeignKey(qst => qst.QuestionSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        //Độ dài phải ngang với Description của QuestionSet
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(500);
    }
}

//Bảng trung gian giữa người dùng và bộ câu hỏi
public class QuestionSetUserConfiguration : IEntityTypeConfiguration<QuestionSetUser>
{

    public void Configure(EntityTypeBuilder<QuestionSetUser> builder)
    {
        builder.HasKey(qsu => new { qsu.UserId, qsu.QuestionSetId });

        builder.Property(qsu => qsu.ShareMode)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (QuestionSetUserShareMode)Enum.Parse(typeof(QuestionSetUserShareMode), v));

        builder.HasOne(qsu => qsu.User)
            .WithMany()
            .HasForeignKey(qsu => qsu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(qsu => qsu.QuestionSet)
            .WithMany()
            .HasForeignKey(qsu => qsu.QuestionSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Question)
            .WithMany()
            .HasForeignKey(c => c.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.ChildComments)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserQuestionSetHistoryConfiguration : IEntityTypeConfiguration<UserQuestionSetHistory>
{
    public void Configure(EntityTypeBuilder<UserQuestionSetHistory> builder)
    {
        builder.Property(uqsh => uqsh.IsCorrect)
            .IsRequired();

        builder.HasOne(uqsh => uqsh.User)
            .WithMany()
            .HasForeignKey(uqsh => uqsh.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(uqsh => uqsh.Question)
            .WithMany()
            .HasForeignKey(uqsh => uqsh.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TestTemplateConfiguration : IEntityTypeConfiguration<TestTemplate>
{
    public void Configure(EntityTypeBuilder<TestTemplate> builder)
    {
        builder.Property(tt => tt.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}

public class TestTemplateQuestionConfiguration : IEntityTypeConfiguration<TestTemplateQuestion>
{
    public void Configure(EntityTypeBuilder<TestTemplateQuestion> builder)
    {
        builder.HasOne(ttq => ttq.TestTemplate)
            .WithMany()
            .HasForeignKey(ttq => ttq.TestTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ttq => ttq.Question)
            .WithMany()
            .HasForeignKey(ttq => ttq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}

public class FolderTestTemplateConfiguration : IEntityTypeConfiguration<FolderTestTemplate>
{
    public void Configure(EntityTypeBuilder<FolderTestTemplate> builder)
    {
        builder.HasKey(ftt => new { ftt.FolderId, ftt.TestTemplateId });

        builder.HasOne(ftt => ftt.Folder)
            .WithMany()
            .HasForeignKey(ftt => ftt.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ftt => ftt.TestTemplate)
            .WithMany()
            .HasForeignKey(ftt => ftt.TestTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FolderUserConfiguration : IEntityTypeConfiguration<FolderUser>
{
    public void Configure(EntityTypeBuilder<FolderUser> builder)
    {
        builder.HasKey(fu => new { fu.UserId, fu.FolderId });

        builder.Property(fu => fu.ShareMode)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (FolderShareMode)Enum.Parse(typeof(FolderShareMode), v));

        builder.HasOne(fu => fu.User)
            .WithMany()
            .HasForeignKey(fu => fu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fu => fu.Folder)
            .WithMany()
            .HasForeignKey(fu => fu.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.TimeStart)
            .IsRequired();

        builder.Property(t => t.TimeFinish)
            .IsRequired();

        builder.Property(t => t.TimeLimit)
            .IsRequired();
        builder.Property(t => t.QuestionCount)
            .IsRequired();

        builder.Property(t => t.GradeAttemptMethod)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (GradeAttemptMethod)Enum.Parse(typeof(GradeAttemptMethod), v));

        builder.Property(t => t.GradeQuestionMethod)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (GradeQuestionMethod)Enum.Parse(typeof(GradeQuestionMethod), v));

        builder.Property(t => t.ClassId)
            .IsRequired();

        builder.HasOne(t => t.Class)
            .WithMany()
            .HasForeignKey(t => t.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.IsShowCorrectAnswerInReview)
            .HasDefaultValue(false);
    }
}

public class TestVersionConfiguration : IEntityTypeConfiguration<TestVersion>
{
    public void Configure(EntityTypeBuilder<TestVersion> builder)
    {
        builder.Property(tv => tv.No)
            .IsRequired();

        builder.HasOne(tv => tv.Test)
            .WithMany()
            .HasForeignKey(tv => tv.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TestVersionQuestionConfiguration : IEntityTypeConfiguration<TestVersionQuestion>
{
    public void Configure(EntityTypeBuilder<TestVersionQuestion> builder)
    {
        builder.Property(tvq => tvq.Order)
            .IsRequired();

        builder.HasOne(tvq => tvq.TestVersion)
            .WithMany()
            .HasForeignKey(tvq => tvq.TestVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tvq => tvq.Question)
            .WithMany()
            .HasForeignKey(tvq => tvq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.Property(a => a.TimeStart)
            .IsRequired();

        builder.Property(a => a.TimeFinish)
            .IsRequired();

        builder.Property(a => a.Score)
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        builder.HasOne(a => a.Test)
            .WithMany()
            .HasForeignKey(a => a.TestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AttemptQuestionConfiguration : IEntityTypeConfiguration<AttemptQuestion>
{
    public void Configure(EntityTypeBuilder<AttemptQuestion> builder)
    {
        builder.Property(aq => aq.Order)
            .HasDefaultValue(0);

        builder.Property(aq => aq.Score)
            .HasColumnType("numeric(5,2)");

        builder.Property(aq => aq.DataJson)
            .IsRequired()
            .HasColumnType("json");

        builder.Ignore(aq => aq.Data);

        builder.HasOne(aq => aq.Attempt)
            .WithMany()
            .HasForeignKey(aq => aq.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aq => aq.Question)
            .WithMany()
            .HasForeignKey(aq => aq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(c => c.ClassQuestionSets)
            .WithOne(cqs => cqs.Class)
            .HasForeignKey(cqs => cqs.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ClassUserConfiguration : IEntityTypeConfiguration<ClassUser>
{
    public void Configure(EntityTypeBuilder<ClassUser> builder)
    {
        builder.HasKey(cu => new { cu.ClassId, cu.UserId });

        builder.Property(cu => cu.ShareMode)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ClassShareMode)Enum.Parse(typeof(ClassShareMode), v));

        builder.HasOne(cu => cu.Class)
            .WithMany()
            .HasForeignKey(cu => cu.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cu => cu.User)
            .WithMany()
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClassQuestionSetConfiguration : IEntityTypeConfiguration<ClassQuestionSet>
{
    public void Configure(EntityTypeBuilder<ClassQuestionSet> builder)
    {
        builder.HasKey(cqs => new { cqs.ClassId, cqs.QuestionSetId });

        builder.HasOne(cqs => cqs.Class)
            .WithMany(c => c.ClassQuestionSets)
            .HasForeignKey(cqs => cqs.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cqs => cqs.QuestionSet)
            .WithMany()
            .HasForeignKey(cqs => cqs.QuestionSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClassInvitationConfiguration : IEntityTypeConfiguration<ClassInvitation>
{
    public void Configure(EntityTypeBuilder<ClassInvitation> builder)
    {
        builder.Property(ci => ci.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ci => ci.TimeStart)
            .IsRequired();

        builder.Property(ci => ci.TimeEnd)
            .IsRequired();

        builder.Property(ci => ci.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(ci => ci.Class)
            .WithMany()
            .HasForeignKey(ci => ci.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ClassInvitationUserConfiguration : IEntityTypeConfiguration<ClassInvitationUser>
{
    public void Configure(EntityTypeBuilder<ClassInvitationUser> builder)
    {
        builder.Property(ciu => ciu.TimeJoin)
            .IsRequired();

        builder.HasOne(ciu => ciu.User)
            .WithMany()
            .HasForeignKey(ciu => ciu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ciu => ciu.ClassInvitation)
            .WithMany()
            .HasForeignKey(ciu => ciu.ClassInvitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TestGradeConfiguration : IEntityTypeConfiguration<TestGrade>
{

    public void Configure(EntityTypeBuilder<TestGrade> builder)
    {
        builder.HasKey(tg => tg.Id);

        builder.Property(tg => tg.TestId).IsRequired();
        builder.Property(tg => tg.UserId).IsRequired();
        builder.Property(tg => tg.Score).HasColumnType("numeric(5,2)").IsRequired();

        //Nối với bảng Test
        builder.HasOne(tg => tg.Test)
            .WithMany(t => t.TestGrades) //Navigation nối ngược từ Test về TestGrade
            .HasForeignKey(tg => tg.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        //Nối với bảng User
        builder.HasOne(tg => tg.User)
            .WithMany() // nếu User không có navigation property ngược
            .HasForeignKey(tg => tg.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
