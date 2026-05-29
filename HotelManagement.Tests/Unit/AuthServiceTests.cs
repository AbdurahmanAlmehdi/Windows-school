using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class AuthServiceTests
{
    [Fact]
    public void Login_AcceptsValidSuperAdminCredentials()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        var result = h.Auth.Login("superadmin", "superadmin123");

        result.Should().BeTrue();
        h.Auth.CurrentUser.Should().NotBeNull();
        h.Auth.CurrentUser!.Username.Should().Be("superadmin");
    }

    [Fact]
    public void Login_AcceptsValidStaffCredentials()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        h.Auth.Login("staff", "staff123").Should().BeTrue();
        h.Auth.CurrentUser!.Role.Name.Should().Be("Staff");
    }

    [Fact]
    public void Login_RejectsWrongPassword()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        h.Auth.Login("superadmin", "WRONG").Should().BeFalse();
        h.Auth.CurrentUser.Should().BeNull();
    }

    [Fact]
    public void Login_RejectsUnknownUsername()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        h.Auth.Login("ghost", "anything").Should().BeFalse();
    }

    [Fact]
    public void Login_IsUsernameCaseInsensitive_PerFR_AUTH_1()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        h.Auth.Login("SuperAdmin", "superadmin123").Should().BeTrue();
        h.Auth.Login("SUPERADMIN", "superadmin123").Should().BeTrue();
    }

    [Fact]
    public void Login_IsPasswordCaseSensitive_PerFR_AUTH_1()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        h.Auth.Login("superadmin", "SUPERADMIN123").Should().BeFalse();
    }

    [Fact]
    public void Login_ReturnsFalse_WhenUsernameIsNull()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        var act = () => h.Auth.Login(null!, "any");

        act.Should().NotThrow();
        h.Auth.CurrentUser.Should().BeNull();
    }

    [Fact]
    public void Login_ReturnsFalse_WhenPasswordIsNull()
    {
        var h = TestStoreFactory.Build(loginAs: null);

        var act = () => h.Auth.Login("superadmin", null!);

        act.Should().NotThrow();
        h.Auth.CurrentUser.Should().BeNull();
    }

    [Fact]
    public void Logout_ClearsCurrentUser()
    {
        var h = TestStoreFactory.Build();
        h.Auth.CurrentUser.Should().NotBeNull();

        h.Auth.Logout();

        h.Auth.CurrentUser.Should().BeNull();
    }

    [Fact]
    public void Can_ReturnsTrueForGrantedPermission()
    {
        var h = TestStoreFactory.Build();

        h.Auth.Can(PermissionResource.Users, PermissionAction.Delete).Should().BeTrue();
    }

    [Fact]
    public void Can_ReturnsFalseForRevokedPermission_OnStaff()
    {
        var h = TestStoreFactory.Build(loginAs: "staff", password: "staff123");

        h.Auth.Can(PermissionResource.Users, PermissionAction.Read).Should().BeFalse();
        h.Auth.Can(PermissionResource.Rooms, PermissionAction.Create).Should().BeFalse();
    }

    [Fact]
    public void Require_ThrowsUnauthorized_WhenPermissionMissing()
    {
        var h = TestStoreFactory.Build(loginAs: "staff", password: "staff123");

        var act = () => h.Auth.Require(PermissionResource.Users, PermissionAction.Create);

        act.Should().Throw<UnauthorizedAccessException>();
    }
}
