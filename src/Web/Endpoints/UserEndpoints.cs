using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Application.Users.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapGet(GetAllAccount, "");
        group.MapPatch("/Info", UpdateUserInfo);
        group.MapPatch("{UserId}/Role", ChangeRole);
        group.MapPatch("/{UserId}/Ban", BanUser);
        group.MapGet("/ForSelection", SearchUserForSelection);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<PaginatedList<UserForListDto>>>> GetAllAccount(
        ISender sender,
        [AsParameters] SearchAllAccountQuery query)
    {
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> BanUser([FromRoute] Guid userId,
        [FromBody] BanAccountCommand command
        , ISender sender)
    {
        var rq = new BanAccountCommand()
        {
            UserId = userId,
            IsBanned = command.IsBanned
        };

        var result = await sender.Send(rq);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<Guid>>> ChangeRole(
        [FromRoute] Guid userId,
        [FromQuery] string role,
        ISender sender)
    {
        var rq = new ChangeAccountRoleCommand()
        {
            UserId = userId,
            Role = role,
        };

        var result = await sender.Send(rq);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<List<UserForSelectionDto>>>> SearchUserForSelection(
        [FromQuery] string email,
        ISender sender)
    {
        var rq = new SearchUserForSelectionQuery()
        {
            Email = email
        };

        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> UpdateUserInfo (
        [FromBody] UpdateUserInfoCommand rq,
        ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }

}
