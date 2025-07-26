using CleanArchitectureBase.Application.Comments.Dto;
using CleanArchitectureBase.Application.Comments.Service;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using AutoMapper;

namespace CleanArchitectureBase.Application.Comments;

public class GetCommentByQuestionQuery : IRequest<PaginatedList<CommentDto>>
{
    public required Guid QuestionId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
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
        var question = await _commentService.CanComment(rq.QuestionId, cancellationToken);

        var comments = await _context.Comments
            .Where(x => x.QuestionId == rq.QuestionId && x.IsDeleted == false)
            .ToListAsync(cancellationToken);
        
        var commentDtos = _mapper.Map<List<CommentDto>>(comments);

        return await PaginatedList<CommentDto>.CreateAsync(
            commentDtos.AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
