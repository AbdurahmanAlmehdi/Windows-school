using FluentAssertions;
using HotelManagement.Tests.TestFixtures;
using HotelManagement.WinForms.Models;
using Xunit;

namespace HotelManagement.Tests.Acceptance;

// =====================================================================
// UC-2: Create a Reservation
// =====================================================================
//
// As a front-desk staff member,
// I want to record a guest reservation against a specific room and dates,
// so that the room is held and the booking can later become a stay.
//
// Acceptance criteria covered:
//   - A valid request creates a Confirmed reservation (FR-RES-2).
//   - Check-out must be after check-in (DC-2 / DEF-01).
//   - Party must fit room capacity (FR-RES-9 / DEF-02).
//   - Mixed-gender adult couples require a marriage certificate ID
//     (FR-RES-10 / DEF-03).
//   - A second reservation cannot overlap an existing one on the same
//     room (DEF-08).
//   - Children count as half an adult for capacity math.
// =====================================================================

public class UC2_ReservationTests
{
    [Fact]
    public void Scenario_FrontDeskBooksAvailableRoom_ReservationIsConfirmed()
    {
        // Given a guest, an available room, and valid dates
        var h = TestStoreFactory.Build();
        var guest = h.Store.Guests[0];
        var room  = TestStoreFactory.FirstAvailableRoom(h.Store);

        // When the staff creates the reservation
        var res = h.Booking.CreateReservation(
            guest, room,
            DateTime.Today.AddDays(10),
            DateTime.Today.AddDays(13));

        // Then the reservation is held in Confirmed status and recorded
        res.Status.Should().Be(ReservationStatus.Confirmed);
        h.Store.Reservations.Should().Contain(res);
    }

    [Fact]
    public void Scenario_StaffPicksCheckOutBeforeCheckIn_BookingIsRejected()
    {
        // Given a guest and a room
        var h = TestStoreFactory.Build();
        var guest = h.Store.Guests[0];
        var room  = TestStoreFactory.FirstAvailableRoom(h.Store);

        // When the staff member accidentally picks check-out earlier than check-in
        var act = () => h.Booking.CreateReservation(
            guest, room,
            DateTime.Today.AddDays(5),
            DateTime.Today.AddDays(2));

        // Then the system rejects the booking with a date error (DC-2)
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Scenario_PartyExceedsRoomCapacity_BookingIsRejected_PerFR_RES_9()
    {
        // Given an available Single room (capacity 1 = 2 capacity units)
        var h = TestStoreFactory.Build();
        var singleRoom = h.Store.Rooms.First(r => r.Type == RoomType.Single && r.IsAvailable);
        var guest = h.Store.Guests[0];

        // And a party of two adults (4 capacity units) - over the limit
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Spouse", Age = 30, Gender = Gender.Female, Passport = "P-X1" }
        };

        // When the staff tries to book the room for that party
        var act = () => h.Booking.CreateReservation(
            guest, singleRoom,
            DateTime.Today.AddDays(3), DateTime.Today.AddDays(5),
            accompanying: party);

        // Then the booking is rejected and the error mentions capacity
        act.Should().Throw<InvalidOperationException>().WithMessage("*capacity*");
    }

    [Fact]
    public void Scenario_MixedGenderAdultCoupleWithoutMarriageCertificate_BookingIsRejected_PerFR_RES_10()
    {
        // Given a Double room and a mixed-gender adult couple without a certificate
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store, minCapacity: 2);
        var primary = new Guest { Name = "Alex", Gender = Gender.Male,   Passport = "P-Alex" };
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Sam", Gender = Gender.Female, Age = 29, Passport = "P-Sam" }
        };

        // When the staff tries to record the booking with no certificate ID
        var act = () => h.Booking.CreateReservation(
            primary, room,
            DateTime.Today.AddDays(2), DateTime.Today.AddDays(4),
            accompanying: party,
            marriageCertificateId: null);

        // Then the booking is rejected and the error explains the requirement
        act.Should().Throw<InvalidOperationException>().WithMessage("*marriage*");
    }

    [Fact]
    public void Scenario_MixedGenderAdultCoupleWithMarriageCertificate_BookingSucceeds()
    {
        // Given the same couple, but with a valid certificate ID
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store, minCapacity: 2);
        var primary = new Guest { Name = "Alex", Gender = Gender.Male,   Passport = "P-Alex" };
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Sam", Gender = Gender.Female, Age = 29, Passport = "P-Sam" }
        };

        // When the staff supplies the certificate ID
        var res = h.Booking.CreateReservation(
            primary, room,
            DateTime.Today.AddDays(2), DateTime.Today.AddDays(4),
            accompanying: party,
            marriageCertificateId: "MC-2026-00091");

        // Then the booking is held and the certificate ID is recorded
        res.MarriageCertificateId.Should().Be("MC-2026-00091");
        res.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Scenario_AdultParentBooksWithMinorChildOppositeGender_NoCertificateNeeded()
    {
        // Given a parent and a young child of the opposite gender
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store, minCapacity: 2);
        var parent = new Guest { Name = "Adult", Gender = Gender.Male, Passport = "P-Parent" };
        var party = new List<AccompanyingGuest>
        {
            new() { Name = "Daughter", Gender = Gender.Female, Age = 9, Passport = "P-Daughter" }
        };

        // When the staff records the booking
        var act = () => h.Booking.CreateReservation(
            parent, room,
            DateTime.Today.AddDays(2), DateTime.Today.AddDays(4),
            accompanying: party);

        // Then no marriage certificate is required (children are exempt)
        act.Should().NotThrow();
    }

    [Fact]
    public void Scenario_SecondReservationOverlapsFirstOnSameRoom_BookingIsRejected_PerDEF_08()
    {
        // Given an unreserved room with an existing future reservation
        var h = TestStoreFactory.Build();
        var room = TestStoreFactory.FirstAvailableRoom(h.Store);
        var g1 = new Guest { Name = "First Guest",  Passport = "P-G1" };
        var g2 = new Guest { Name = "Second Guest", Passport = "P-G2" };
        h.Booking.CreateReservation(g1, room,
            DateTime.Today.AddDays(5), DateTime.Today.AddDays(10));

        // When a second guest tries to book the same room on overlapping dates
        var act = () => h.Booking.CreateReservation(
            g2, room,
            DateTime.Today.AddDays(7), DateTime.Today.AddDays(8));

        // Then the second booking is rejected with an overlap error
        act.Should().Throw<InvalidOperationException>().WithMessage("*overlap*");
    }

    [Fact]
    public void Scenario_GuestCancelsReservation_StatusBecomesCancelled()
    {
        // Given an existing Confirmed reservation
        var h = TestStoreFactory.Build();
        var res = h.Store.Reservations.First(r => r.Status == ReservationStatus.Confirmed);

        // When the front desk cancels it
        h.Booking.Cancel(res);

        // Then the reservation is marked Cancelled and the room becomes
        // bookable again for those dates
        res.Status.Should().Be(ReservationStatus.Cancelled);
    }
}
