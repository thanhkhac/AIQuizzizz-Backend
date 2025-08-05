using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Commands;

public class UpdateQuestionSetSharingCommand : IRequest
{
    [JsonIgnore]
    public Guid QuestionSetId { get; set; }
    public string? VisibilityMode { get; set; }
    public List<UpsertSharingModelDto> SharingModels { get; set; } = new();
    public List<Guid> DeleteUserIds { get; set; } = new();
}

public class UpdateQuestionSetSharingCommandValidator : AbstractValidator<UpdateQuestionSetSharingCommand>
{
    private static readonly string[] AllowedVisibilityModes =
    {
        "Public", "Private", "OnlyClass"
    };

    private static readonly string[] AllowedShareModes =
    {
        "Editable", "ViewOnly"
    };

    public UpdateQuestionSetSharingCommandValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty();

        RuleFor(x => x.VisibilityMode)
            .Must(x => string.IsNullOrEmpty(x) || AllowedVisibilityModes.Contains(x))
            .WithMessage($"VisibilityMode must be one of: {string.Join(", ", AllowedVisibilityModes)}");

        RuleFor(x => x.DeleteUserIds)
            .Must(x => x == null || x.Distinct().Count() == x.Count)
            .WithMessage("DeleteUserIds must not contain duplicates.");

        RuleForEach(x => x.SharingModels)
            .ChildRules(model =>
            {
                model.RuleFor(x => x.SharingUserId)
                    .NotNull()
                    .WithMessage("SharingUserId is required.");

                model.RuleFor(x => x.ShareMode)
                    .NotEmpty()
                    .WithMessage("ShareMode is required.")
                    .Must(mode => AllowedShareModes.Contains(mode!))
                    .WithMessage($"ShareMode must be one of: {string.Join(", ", AllowedShareModes)}");
            });
    }
}

public class UpdateQuestionSetSharingCommandHandler : IRequestHandler<UpdateQuestionSetSharingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _user;
    public UpdateQuestionSetSharingCommandHandler(IApplicationDbContext context, IQuestionSetService questionSetService, IUser user)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
    }

    public async Task Handle(UpdateQuestionSetSharingCommand request, CancellationToken cancellationToken)
    {
        var questionSetId = request.QuestionSetId;
        var userId = _user.UserId!.Value;
        var questionSet = await _questionSetService.GetActiveQuestionSet(questionSetId, cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        var canEdit = questionSet.CreatedBy == userId;
        if (canEdit == false)
            throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN);
        var ownerId = questionSet.CreatedBy!.Value;

        var newVisibility = Enum.Parse<QuestionSetVisibilityMode>(request.VisibilityMode!);

        if (questionSet.VisibilityMode == QuestionSetVisibilityMode.Public && newVisibility == QuestionSetVisibilityMode.OnlyClass)
        {
            var unauthorizedClassLinks = await (
                from cqs in _context.ClassQuestionSets
                join cls in _context.Classes on cqs.ClassId equals cls.Id
                
                join cu in _context.ClassUsers on new
                {
                    cqs.ClassId,
                    UserId = ownerId
                } equals new
                {
                    cu.ClassId,
                    cu.UserId
                } into cuJoin
                from cu in cuJoin.DefaultIfEmpty()
                where cqs.QuestionSetId == questionSetId
                      && (cu == null || cu.ShareMode != ClassShareMode.Owner && cu.ShareMode != ClassShareMode.Teacher)
                select cqs
            ).ToListAsync(cancellationToken);
            
            _context.ClassQuestionSets.RemoveRange(unauthorizedClassLinks);
        }

        //Bất cứ mode nào chuyển về private thì đều xóa hết class link
        if (questionSet.VisibilityMode != QuestionSetVisibilityMode.Private && newVisibility == QuestionSetVisibilityMode.Private)
        {
            var allClassLinks = await _context.ClassQuestionSets
                .Include(cqs => cqs.Class)
                .Where(cqs => cqs.QuestionSetId == questionSetId)
                .ToListAsync(cancellationToken);
            _context.ClassQuestionSets.RemoveRange(allClassLinks);
        }


        questionSet.VisibilityMode = newVisibility;


        var sharingModels = request.SharingModels;
        sharingModels.RemoveAll(x => x.SharingUserId == ownerId);
        var sharingModelIds = sharingModels.Select(x => x.SharingUserId).ToList();

        var existingUserIds = await _context.DomainUsers
            .Where(u => sharingModelIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var invalidUserIds = sharingModelIds.Except(existingUserIds).ToList();

        if (invalidUserIds.Any())
            throw new ErrorCodeException(ErrorCodes.USER_NOTFOUND, $"UserId không tồn tại: {string.Join(", ", invalidUserIds)}");

        var existedQuestionSetUser =
            await _context.QuestionSetUsers
                .Where(x => x.QuestionSetId == questionSetId && sharingModelIds.Contains(x.UserId))
                .ToListAsync(cancellationToken);

        foreach (var model in sharingModels)
        {
            var entity = existedQuestionSetUser.Find(x => x.UserId == model.SharingUserId);
            var newShareMode = Enum.Parse<QuestionSetUserShareMode>(model.ShareMode!);
            //Đã tồn tại
            if (entity != null)
            {
                if (entity.ShareMode == QuestionSetUserShareMode.Owner)
                    continue;
                entity.ShareMode = newShareMode;
            }
            else //chưa tồn tại
            {
                var newQuestionSetUser = new QuestionSetUser
                {
                    QuestionSetId = questionSetId,
                    UserId = model.SharingUserId,
                    ShareMode = newShareMode
                };
                _context.QuestionSetUsers.Add(newQuestionSetUser);
            }
        }

        var deleteIds = request.DeleteUserIds;
        deleteIds.Remove(ownerId);

        var deleteEntities = _context.QuestionSetUsers
            .Where(x => deleteIds.Contains(x.UserId)
                        && x.QuestionSetId == questionSetId);

        _context.QuestionSetUsers.RemoveRange(deleteEntities);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
