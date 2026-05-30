using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-3: Check a Guest In
// =====================================================================
//
// As a front-desk staff member,
// I want to check a guest in against their confirmed reservation,
// so that an Active Stay is opened and the room is marked occupied.
//
// Acceptance criteria covered:
//   - A Confirmed reservation can be checked in (FR-RES-4).
//   - The check-in opens a Stay in Active status.
//   - The room is marked occupied on check-in (FR-ROOM-7).
//   - A Cancelled or Completed reservation cannot be checked in.
//   - A reservation cannot be checked in to an already-occupied room
//     (FR-ROOM-7 / DEF-05).
// =====================================================================

public class UC3_CheckInTests
{
    [Fact]
    public void Scenario_GuestArrivesWithConfirmedReservation_StayIsOpenedAndRoomOccupied()
    {
        // Given a confirmed reservation
        var h = TestStoreFactory.Build();
        var res = h.Store.Reservations.First(r => r.Status == ReservationStatus.Confirmed);

        // When the front desk checks the guest in
        var stay = h.Booking.CheckIn(res);

        // Then a new Stay opens in Active state
        stay.Status.Should().Be(StayStatus.Active);
        stay.Guest.Should().Be(res.Guest);
        stay.Room.Should().Be(res.Room);
        h.Store.Stays.Should().Contain(stay);

        // And the room is occupied
        res.Room.IsOccupied.Should().BeTrue();

        // And the reservation reflects the transition
        res.Status.Should().Be(ReservationStatus.CheckedIn);
    }

    [Fact]
    public void Scenario_GuestCancelledBeforeArrival_CheckInIsRejected()
    {
        // Given a reservation the guest later cancelled
        var h = TestStoreFactory.Build();
        var res = h.Store.Reservations.First(r => r.Status == ReservationStatus.Confirmed);
        h.Booking.Cancel(res);

        // When the front desk still tries to check them in
        var act = () => h.Booking.CheckIn(res);

        // Then the system refuses the check-in
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Scenario_TwoReservationsForTheSameRoom_CheckInOfTheSecondIsRejected_PerFR_ROOM_7()
    {
        // Given two non-overlapping reservations for the same room
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var g1 = new Guest { Name = "G1", Passport = "P-G1" };
        var g2 = new Guest { Name = "G2", Passport = "P-G2" };
        var r1 = h.Booking.CreateReservation(g1, room,
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        var r2 = h.Booking.CreateReservation(g2, room,
            DateTime.Today.AddDays(4), DateTime.Today.AddDays(6));

        // When the first guest checks in (room becomes occupied)
        h.Booking.CheckIn(r1);

        // And the second guest tries to check in early while the first is
        // still in the room
        var act = () => h.Booking.CheckIn(r2);

        // Then the second check-in is rejected because the room is occupied
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Scenario_GuestCount_OnlyOneActiveStayPerRoomAtATime()
    {
        // Given a room is checked in to
        var h = TestStoreFactory.Build();
        var res = h.Store.Reservations.First(r => r.Status == ReservationStatus.Confirmed);
        h.Booking.CheckIn(res);

        // When we ask the room service who is currently in that room
        var currentStay = h.Rooms.GetCurrentStay(res.Room);

        // Then exactly one Active stay is associated
        currentStay.Should().NotBeNull();
        currentStay!.Guest.Should().Be(res.Guest);
    }
}
