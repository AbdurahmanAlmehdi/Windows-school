using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class UserServiceTests
{
    [Fact]
    public void AddUser_AddsToStore_PerFR_USR_4()
    {
        var h = TestStoreFactory.Build();
        var staffRole = h.Store.Roles.First(r => r.Name == "Staff");

        var u = h.Users.AddUser("kim", "pw123", staffRole);

        h.Store.Users.Should().Contain(u);
        u.Username.Should().Be("kim");
    }

    [Fact]
    public void AddUser_RejectsDuplicateUsername()
    {
        var h = TestStoreFactory.Build();
        var role = h.Store.Roles.First(r => r.Name == "Staff");

        var act = () => h.Users.AddUser("staff", "x", role);

        act.Should().Throw<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public void AddUser_RejectsBlankUsername()
    {
        var h = TestStoreFactory.Build();
        var role = h.Store.Roles.First(r => r.Name == "Staff");

        var act = () => h.Users.AddUser("   ", "x", role);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddUser_RejectsBlankPassword()
    {
        var h = TestStoreFactory.Build();
        var role = h.Store.Roles.First(r => r.Name == "Staff");

        var act = () => h.Users.AddUser("alice", "", role);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddUser_RequiresUsersCreatePermission_PerNFR_SEC_3()
    {
        var h = TestStoreFactory.Build(loginAs: "staff", password: "staff123");
        var role = h.Store.Roles.First(r => r.Name == "Staff");

        var act = () => h.Users.AddUser("intruder", "pw", role);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void RemoveUser_RejectsSelfRemoval_PerFR_USR_7()
    {
        var h = TestStoreFactory.Build();
        var self = h.Auth.CurrentUser!;

        var act = () => h.Users.RemoveUser(self);

        act.Should().Throw<InvalidOperationException>().WithMessage("*signed-in*");
    }

    [Fact]
    public void RemoveUser_RejectsLastSystemAdmin_PerFR_USR_7()
    {
        var h = TestStoreFactory.Build();
        var superAdmin = h.Store.Users.Single(u => u.Username == "superadmin");
        h.Auth.Logout();
        h.Auth.Login("staff", "staff123").Should().BeTrue();

        var act = () => new HotelManagement.WinForms.Services.UserService(h.Store,
                            new HotelManagement.WinForms.Services.AuthService(h.Store))
                        .RemoveUser(superAdmin);

        act.Should().Throw<InvalidOperationException>().WithMessage("*system administrator*");
    }

    [Fact]
    public void RemoveUser_RemovesNonSystemUser()
    {
        var h = TestStoreFactory.Build();
        var staffRole = h.Store.Roles.First(r => r.Name == "Staff");
        var added = h.Users.AddUser("delete-me", "pw", staffRole);

        h.Users.RemoveUser(added);

        h.Store.Users.Should().NotContain(added);
    }

    [Fact]
    public void AddRole_AddsToStore_PerFR_USR_6()
    {
        var h = TestStoreFactory.Build();
        var perms = new[] { new Permission(PermissionResource.Rooms, PermissionAction.Read) };

        var role = h.Users.AddRole("Auditor", perms);

        h.Store.Roles.Should().Contain(role);
        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void AddRole_RejectsDuplicateName()
    {
        var h = TestStoreFactory.Build();

        var act = () => h.Users.AddRole("Staff", Array.Empty<Permission>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateRole_RejectsSystemRole_PerFR_USR_5()
    {
        var h = TestStoreFactory.Build();
        var sa = h.Store.Roles.First(r => r.IsSystem);

        var act = () => h.Users.UpdateRole(sa, "RenamedSuperAdmin", Array.Empty<Permission>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveRole_RejectsAssignedRole_PerFR_USR_8()
    {
        var h = TestStoreFactory.Build();
        var staffRole = h.Store.Roles.First(r => r.Name == "Staff");

        var act = () => h.Users.RemoveRole(staffRole);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Reassign*");
    }

    [Fact]
    public void RemoveRole_RejectsSystemRole_PerFR_USR_5()
    {
        var h = TestStoreFactory.Build();
        var sa = h.Store.Roles.First(r => r.IsSystem);

        var act = () => h.Users.RemoveRole(sa);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveRole_RemovesUnassignedNonSystemRole()
    {
        var h = TestStoreFactory.Build();
        var perms = new[] { new Permission(PermissionResource.Rooms, PermissionAction.Read) };
        var role = h.Users.AddRole("TempRole", perms);

        h.Users.RemoveRole(role);

        h.Store.Roles.Should().NotContain(role);
    }
}
