using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-8: Manage Staff Users and Roles
// =====================================================================
//
// As a system administrator,
// I want to add and remove staff users and custom roles,
// so that the right people have the right access.
//
// Acceptance criteria covered:
//   - A SuperAdmin can add a new user with the Staff role (FR-USR-4).
//   - Duplicate usernames are rejected (FR-USR-4).
//   - Staff users cannot add other users (FR-USR-3 / NFR-SEC-3).
//   - A user cannot remove themselves (FR-USR-7).
//   - The system protects against orphaning itself by removing the
//     last system-role holder (FR-USR-7 / DEF-17).
//   - System roles cannot be edited or removed (FR-USR-5).
//   - Custom roles can be added and removed when not in use (FR-USR-6,
//     FR-USR-8).
// =====================================================================

public class UC8_UserManagementTests
{
    [Fact]
    public void Scenario_SuperAdminAddsNewStaffUser_UserIsRegistered_PerFR_USR_4()
    {
        // Given the SuperAdmin is signed in
        var h = TestStoreFactory.Build();
        var staffRole = h.Store.Roles.First(r => r.Name == "Staff");

        // When they add a new staff user
        var newUser = h.Users.AddUser("alex", "tempPw123!", staffRole);

        // Then the user is registered and assigned the Staff role
        h.Store.Users.Should().Contain(newUser);
        newUser.Username.Should().Be("alex");
        newUser.Role.Should().Be(staffRole);
    }

    [Fact]
    public void Scenario_AdminTriesToCreateDuplicateUsername_OperationIsRejected()
    {
        // Given the SuperAdmin is signed in
        var h = TestStoreFactory.Build();
        var staffRole = h.Store.Roles.First(r => r.Name == "Staff");

        // When they try to create a user with an existing name
        var act = () => h.Users.AddUser("staff", "anyPw", staffRole);

        // Then the operation is rejected
        act.Should().Throw<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public void Scenario_StaffMemberTriesToAddAnotherUser_OperationIsForbidden_PerNFR_SEC_3()
    {
        // Given a regular Staff user is signed in
        var h = TestStoreFactory.Build(loginAs: "staff", password: "staff123");
        var staffRole = h.Store.Roles.First(r => r.Name == "Staff");

        // When they try to create another account
        var act = () => h.Users.AddUser("intruder", "pw", staffRole);

        // Then the operation is forbidden
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void Scenario_SignedInUserTriesToRemoveThemselves_OperationIsRejected_PerFR_USR_7()
    {
        // Given the SuperAdmin is signed in
        var h = TestStoreFactory.Build();
        var self = h.Auth.CurrentUser!;

        // When they try to remove themselves
        var act = () => h.Users.RemoveUser(self);

        // Then the operation is rejected with a "signed-in" message
        act.Should().Throw<InvalidOperationException>().WithMessage("*signed-in*");
    }

    [Fact]
    public void Scenario_AttemptToRemoveLastSystemAdministrator_OperationIsRejected_PerFR_USR_7()
    {
        // Given the seed has exactly one SuperAdmin, and a non-privileged
        // caller tries to remove them via a separately-constructed service
        // (bypassing the self-removal guard)
        var h = TestStoreFactory.Build();
        var superAdmin = h.Store.Users.Single(u => u.Username == "superadmin");

        var act = () => new UserService(h.Store, new AuthService(h.Store))
            .RemoveUser(superAdmin);

        // Then the system invariant kicks in regardless of authorization,
        // preventing application lock-out
        act.Should().Throw<InvalidOperationException>().WithMessage("*system administrator*");
    }

    [Fact]
    public void Scenario_AdminTriesToEditTheSuperAdminRole_OperationIsForbidden_PerFR_USR_5()
    {
        // Given the SuperAdmin is signed in
        var h = TestStoreFactory.Build();
        var systemRole = h.Store.Roles.First(r => r.IsSystem);

        // When they try to rename the system role
        var act = () => h.Users.UpdateRole(systemRole, "RenamedSuperAdmin", Array.Empty<Permission>());

        // Then the operation is forbidden
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Scenario_ManagerCreatesCustomAuditorRole_RoleIsAvailableForAssignment_PerFR_USR_6()
    {
        // Given the SuperAdmin is signed in
        var h = TestStoreFactory.Build();
        var perms = new[]
        {
            new Permission(PermissionResource.Rooms,    PermissionAction.Read),
            new Permission(PermissionResource.Invoices, PermissionAction.Read),
        };

        // When they create a custom Auditor role
        var role = h.Users.AddRole("Auditor", perms);

        // Then the role is registered with the requested permissions
        h.Store.Roles.Should().Contain(role);
        role.Permissions.Should().HaveCount(2);
    }

    [Fact]
    public void Scenario_AdminTriesToDeleteRoleAssignedToAUser_OperationIsRejected_PerFR_USR_8()
    {
        // Given the SuperAdmin is signed in (the Staff role is assigned)
        var h = TestStoreFactory.Build();
        var staffRole = h.Store.Roles.First(r => r.Name == "Staff");

        // When they try to delete the Staff role without reassigning users
        var act = () => h.Users.RemoveRole(staffRole);

        // Then the operation is rejected with a reassignment-needed message
        act.Should().Throw<InvalidOperationException>().WithMessage("*Reassign*");
    }
}
