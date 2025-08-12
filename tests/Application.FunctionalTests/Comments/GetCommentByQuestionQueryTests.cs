// using CleanArchitectureBase.Application.Common.Exceptions;
// using CleanArchitectureBase.Application.Comments.Dto;
// using CleanArchitectureBase.Application.Comments.Queries;
// using CleanArchitectureBase.Application.Comments.Services;
// using CleanArchitectureBase.Domain.Constants;
// using CleanArchitectureBase.Domain.Entities;
// using Microsoft.EntityFrameworkCore;
// using AutoMapper;
//
// namespace CleanArchitectureBase.Application.Command.UnitTests.Comments;
//
// using static Testing;
//
// public class GetCommentByQuestionQueryTests : BaseTestFixture
// {
//     private Mock<ICommentService> _mockCommentService = null!;
//     private IMapper _mapper;
//
//     public GetCommentByQuestionQueryTests()
//     {
//         var configurationProvider = new MapperConfiguration(cfg =>
//         {
//             cfg.CreateMap<Comment, CommentDto>();
//             cfg.CreateMap<List<Comment>, List<CommentDto>>();
//         });
//         _mapper = configurationProvider.CreateMapper();
//     }
//
//     [SetUp]
//     public override async Task TestSetUp()
//     {
//         await base.TestSetUp();
//         _mockCommentService = new Mock<ICommentService>();
//
//                 _factory.Services.AddTransient(provider => _mockCommentService.Object);
//         _factory.Services.AddTransient(provider => _mapper);     }
//
//     //normal
//     [Test]
//     public async Task ShouldReturnComments_WhenUserHasPermission()
//     {
//                 var userId = await RunAsDefaultUserAsync();
//         var questionSet = new QuestionSet { Id = Guid.NewGuid(), Name = "Test Question Set", CreatedBy = userId };
//         await AddAsync(questionSet);
//         var question = new Question { Id = Guid.NewGuid(), QuestionSetId = questionSet.Id, QuestionText = "Sample Question" };
//         await AddAsync(question);
//         await AddAsync(new QuestionSetUser { UserId = userId, QuestionSetId = questionSet.Id, ShareMode = QuestionSetUserShareMode.Owner });
//
//         var comment1 = new Comment { Id = Guid.NewGuid(), QuestionId = question.Id, Content = "Comment 1", IsDeleted = false };
//         var comment2 = new Comment { Id = Guid.NewGuid(), QuestionId = question.Id, Content = "Comment 2", IsDeleted = false, ParentId = comment1.Id };
//         await AddAsync(comment1);
//         await AddAsync(comment2);
//
//         _mockCommentService.Setup(s => s.CanComment(questionSet.Id)).ReturnsAsync(true);
//
//         var query = new GetCommentByQuestionQuery { QuestionId = question.Id };
//
//                 var result = await SendAsync(query);
//
//                 result.Should().NotBeNull();
//         result.Items.Should().HaveCount(2);
//         result.Items.Should().Contain(c => c.Id == comment1.Id && c.Content == "Comment 1");
//         result.Items.Should().Contain(c => c.Id == comment2.Id && c.Content == "Comment 2");
//     }
//
//     //abnormal
//     [Test]
//     public async Task ShouldThrowError_WhenQuestionIdIsEmpty()
//     {
//                 await RunAsDefaultUserAsync();
//         var query = new GetCommentByQuestionQuery { QuestionId = Guid.Empty };
//
//                 var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
//         ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
//     }
//
//     //abnormal
//     [Test]
//     public async Task ShouldThrowError_WhenQuestionNotFound()
//     {
//                 await RunAsDefaultUserAsync();
//         var query = new GetCommentByQuestionQuery { QuestionId = Guid.NewGuid() }; 
//                 var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
//         ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_CAN_NOT_COMMENT);
//     }
//
//     //abnormal
//     [Test]
//     public async Task ShouldThrowError_WhenUserCannotComment()
//     {
//                 var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
//         var questionSet = new QuestionSet { Id = Guid.NewGuid(), Name = "Test Question Set", CreatedBy = ownerId, VisibilityMode = QuestionSetVisibilityMode.Private };
//         await AddAsync(questionSet);
//         var question = new Question { Id = Guid.NewGuid(), QuestionSetId = questionSet.Id, QuestionText = "Sample Question" };
//         await AddAsync(question);
//
//         await RunAsDefaultUserAsync(); 
//         var query = new GetCommentByQuestionQuery { QuestionId = question.Id };
//
//                 var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
//         ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_COMMENT);
//     }
//
//     //abnormal
//     [Test]
//     public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
//     {
//                 var questionSet = new QuestionSet { Id = Guid.NewGuid(), Name = "Test Question Set", CreatedBy = Guid.NewGuid() };
//         await AddAsync(questionSet);
//         var question = new Question { Id = Guid.NewGuid(), QuestionSetId = questionSet.Id, QuestionText = "Sample Question" };
//         await AddAsync(question);
//
//         
//         var query = new GetCommentByQuestionQuery { QuestionId = question.Id };
//
//                 var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
//         ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
//     }
// }


