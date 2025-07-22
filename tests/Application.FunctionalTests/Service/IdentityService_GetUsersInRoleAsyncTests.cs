namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_GetUsersInRoleAsyncTests : IdentityServiceTestBase
{
    // [Test]
    // public async Task GetUsersInRoleAsync_ReturnsUserIds()
    // {
    //     // Precondition: Có user trong role Admin
    //     var user = new UserAccount { Id = Guid.NewGuid() };
    //     _userManagerMock.Setup(x => x.GetUsersInRoleAsync("Administrator")).ReturnsAsync(new List<UserAccount> { user });
    //     // Giả định bạn đã sửa IdentityService để gọi đúng _userManagerMock
    //     // var result = await _service.GetUsersInRoleAsync();
    //     // Assert.That(result, Is.EquivalentTo(new[] { user.Id }));
    //     Assert.Pass("Bạn cần sửa IdentityService để test thực sự với UserManager mock.");
    // }
} 
