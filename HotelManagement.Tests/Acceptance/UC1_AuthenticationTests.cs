using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-1: Sign in to the Hotel Management System
// =====================================================================
//
// As a front-desk staff member, manager, or administrator,
// I want to authenticate with my username and password,
// so that I can access exactly the features my role is permitted to use.
//
// Acceptance criteria covered:
//   - Valid credentials grant a session.
//   - The session reflects the role's CRUD permission map (NFR-SEC-3).
//   - Invalid credentials never grant a session.
//   - Username matching is case-insensitive; passwords are case-sensitive.
//   - Null inputs return false instead of crashing (NFR-REL-1).
// =====================================================================

public class UC1_AuthenticationTests
{
    [Fact]
    public void Scenario_FrontDeskStaffSignsInWithValidCredentials_ReceivesStaffPermissions()
    {
        // Given a fresh hotel management session at the login screen
        var h = TestStoreFactory.Build(loginAs: null);

        // When the front-desk staff member signs in with the correct credentials
        var signedIn = h.Auth.Login("staff", "staff123");

        // Then they reach the application as a Staff user
        signedIn.Should().BeTrue();
        h.Auth.CurrentUser.Should().NotBeNull();
        h.Auth.CurrentUser!.Username.Should().Be("staff");
        h.Auth.CurrentUser.Role.Name.Should().Be("Staff");

        // And the permission map they see reflects the Staff role
        h.Auth.Can(PermissionResource.Reservations, PermissionAction.Create).Should().BeTrue();
        h.Auth.Can(PermissionResource.Orders,       PermissionAction.Create).Should().BeTrue();
        h.Auth.Can(PermissionResource.Users,        PermissionAction.Delete).Should().BeFalse();
        h.Auth.Can(PermissionResource.Rooms,        PermissionAction.Create).Should().BeFalse();
    }

    [Fact]
    public void Scenario_AdministratorSignsIn_HasFullPermissionMap()
    {
        // Given a fresh session
        var h = TestStoreFactory.Build(loginAs: null);

        // When the SuperAdmin signs in
        var signedIn = h.Auth.Login("superadmin", "superadmin123");

        // Then the SuperAdmin has every CRUD on every resource
        signedIn.Should().BeTrue();
        foreach (var resource in Enum.GetValues<PermissionResource>())
        foreach (var action in Enum.GetValues<PermissionAction>())
            h.Auth.Can(resource, action).Should().BeTrue(
                $"SuperAdmin should be allowed to {action} on {resource}");
    }

    [Fact]
    public void Scenario_UserTriesToSignInWithWrongPassword_AccessIsDenied()
    {
        // Given the login screen
        var h = TestStoreFactory.Build(loginAs: null);

        // When the user submits a known username with the wrong password
        var signedIn = h.Auth.Login("staff", "WRONG-PASSWORD");

        // Then access is denied and no session is created
        signedIn.Should().BeFalse();
        h.Auth.CurrentUser.Should().BeNull();
    }

    [Fact]
    public void Scenario_UsernameMatchingIsCaseInsensitive_PerFR_AUTH_1()
    {
        // Given the login screen
        var h = TestStoreFactory.Build(loginAs: null);

        // When the user types their username in a different case
        // Then they are still recognised
        h.Auth.Login("SuperAdmin", "superadmin123").Should().BeTrue();
    }

    [Fact]
    public void Scenario_PasswordIsCaseSensitive_PerFR_AUTH_1()
    {
        // Given the login screen
        var h = TestStoreFactory.Build(loginAs: null);

        // When the user types the password with wrong case
        // Then they are denied
        h.Auth.Login("staff", "STAFF123").Should().BeFalse();
    }

    [Fact]
    public void Scenario_LoginFormSubmitsBlankFields_DoesNotCrash()
    {
        // Given the login screen
        var h = TestStoreFactory.Build(loginAs: null);

        // When the user submits the form with null fields (defensive input from
        // a malformed UI binding)
        var act = () => h.Auth.Login(null!, null!);

        // Then no exception is thrown and no session is created
        act.Should().NotThrow();
        h.Auth.CurrentUser.Should().BeNull();
    }

    [Fact]
    public void Scenario_UserLogsOut_SessionIsCleared()
    {
        // Given a signed-in superadmin
        var h = TestStoreFactory.Build();
        h.Auth.CurrentUser.Should().NotBeNull();

        // When they log out
        h.Auth.Logout();

        // Then the session is cleared and permission checks fail
        h.Auth.CurrentUser.Should().BeNull();
        h.Auth.Can(PermissionResource.Rooms, PermissionAction.Read).Should().BeFalse();
    }
}
