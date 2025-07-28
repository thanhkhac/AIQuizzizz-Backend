    using CleanArchitectureBase.Application.Comments.Dto;
    using CleanArchitectureBase.Application.Comments.Service;
    using CleanArchitectureBase.Application.Common.Interfaces;
    using CleanArchitectureBase.Application.Common.Models;
    using AutoMapper;
    using CleanArchitectureBase.Application.Common.Exceptions;
    using CleanArchitectureBase.Application.Common.Security;
    using CleanArchitectureBase.Domain.Constants;

    namespace CleanArchitectureBase.Application.Comments;

    [Authorize]
    public class GetCommentByQuestionQuery : IRequest<PaginatedList<CommentDto>>
    {
        public required Guid QuestionId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetCommentByQuestionQueryValidator : AbstractValidator<GetCommentByQuestionQuery>
    {
        public GetCommentByQuestionQueryValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty().WithMessage("QuestionID không được để trống");
        }
    }

    public class GetCommentByQuestionQueryHandler : IRequestHandler<GetCommentByQuestionQuery, PaginatedList<CommentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;
        
        public GetCommentByQuestionQueryHandler(IApplicationDbContext context, ICommentService commentService, IMapper mapper)
        {
            _context = context;
            _commentService = commentService;
            _mapper = mapper;
        }
        
        public async Task<PaginatedList<CommentDto>> Handle(GetCommentByQuestionQuery rq, CancellationToken cancellationToken)
        {
            var question = await _context.Questions
                .Include(x => x.QuestionSet)
                .Where(x => x.Id == rq.QuestionId && x.IsDeleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (question == null || question.QuestionSet == null)
                throw new ErrorCodeException(ErrorCodes.QUESTION_CAN_NOT_COMMENT,
                    "Question không tồn tại hoặc không thể comment");
            
            var canGetComment = await _commentService.CanComment(question.QuestionSet.Id);
            if (!canGetComment)
                throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_COMMENT, "User không được comment");

            var comments = await _context.Comments
                .Include(x => x.ChildComments)
                .Where(x => x.QuestionId == rq.QuestionId && x.IsDeleted == false)
                .ToListAsync(cancellationToken);
            
            var commentDtos = _mapper.Map<List<CommentDto>>(comments);
            
            return PaginatedList<CommentDto>.Create(
                commentDtos,
                rq.PageNumber,
                rq.PageSize
            );
        }
    }
