using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitectureBase.Domain.Constants;
public static class PermissionConstants
{
    public record ClaimDefinition(string Code, string Name);

    public record PermissionGroup(string Name, IReadOnlyList<ClaimDefinition> Claims);

    public static class Groups
    {
        public static readonly PermissionGroup Department = new(
            Name: "Chức năng phòng ban",
            Claims: new List<ClaimDefinition>
            {
                    new ClaimDefinition("CreateDepartment", "Tạo phòng ban"),
                    new ClaimDefinition("DeleteDepartment", "Xóa phòng ban"),
                    new ClaimDefinition("UpdateDepartment", "Cập nhật phòng ban"),
                    new ClaimDefinition("ViewDepartment", "Xem phòng ban")
            }.AsReadOnly()
        );

        public static readonly PermissionGroup Room = new(
            Name: "Chức năng quản lý phòng trọ",
            Claims: new List<ClaimDefinition>
            {
                    new ClaimDefinition("CreateRoom", "Tạo phòng trọ"),
                    new ClaimDefinition("DeleteRoom", "Xóa phòng trọ"),
                    new ClaimDefinition("UpdateRoom", "Cập nhật phòng trọ"),
                    new ClaimDefinition("ViewRoom", "Xem phòng trọ")
            }.AsReadOnly()
        );

        public static readonly IReadOnlyList<PermissionGroup> All = new List<PermissionGroup>
            {
                Department,
                Room
            }.AsReadOnly();
    }

    public static IEnumerable<ClaimDefinition> GetAllClaims()
    {
        return Groups.All.SelectMany(g => g.Claims);
    }

    public static IEnumerable<string> GetAllClaimCodes()
    {
        return GetAllClaims().Select(c => c.Code);
    }
}
