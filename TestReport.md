# Software Test Report

## Hotel Management System (WinForms Desktop Edition)

**Document Type:** Independent QA Test Report
**Document Version:** 1.0
**Date:** 2026-05-21
**Prepared by:** Abdurahman Ibrahem — QA / Test Engineer
**Reference SRS:** *Software Requirements Specification, Hotel Management System*, v1.2 (2026-05-11)
**Build Under Test:** `HotelManagement.WinForms` — branch `main`, commit `b11b95b`
**Target Audience:** Development Team (programmer), Course Instructor (Software Testing)

---

## Table of Contents

1. Introduction
2. Project Overview
3. Testing Environment
4. Testing Strategy
5. Unit Testing Scope
6. Test Suites Executed
7. Test Execution Results
8. Defects and Issues Identified
9. Challenges Faced During Testing
10. Retesting and Validation
11. Conclusion
12. Appendix A — Defect Severity Definitions
13. Appendix B — Traceability Matrix (Tests → SRS)

---

## 1. Introduction

This document is the formal **Software Test Report (STR)** for the Hotel
Management System (HMS), a single-user WinForms desktop application built
on Microsoft .NET 9. It has been prepared in accordance with the
documentation conventions of IEEE Std 829-2008 (*Standard for Software and
System Test Documentation*) and submitted as the deliverable for the
Software Testing course associated with the project.

The author of this report acted strictly in the role of an **independent
test engineer**. No defect, edge-case failure, or unexpected behaviour
discovered during this exercise has been remediated by the tester; all
findings have been recorded verbatim and forwarded to the development
team for triage. This separation of duties is intentional and mirrors
the role boundary observed between QA and Development in industry
practice.

The goals of the testing effort were:

- To verify that the service layer of HMS behaves in accordance with
  the functional requirements stated in the SRS v1.2.
- To exercise the edge cases that the SRS implies but does not
  explicitly state (null inputs, boundary values, illegal state
  transitions, and arithmetic precision concerns).
- To produce a reproducible, evidence-based defect log suitable for
  prioritisation and fix-cycle planning.

This report does **not** cover graphical / WinForms presentation
behaviour; that will be the subject of a separate exploratory pass.

---

## 2. Project Overview

The Hotel Management System is a desktop application supporting
front-desk, housekeeping, restaurant, invoicing, and management-reporting
workflows for a small-to-mid-sized hotel. Its architecture follows a
strict three-tier layering (Presentation → Service → Data), consistent
with the Onion-style discipline mandated by the project's coding
standard.

| Tier | Components |
|---|---|
| Presentation | `LoginForm`, `MainForm`, `CheckoutForm`, dialog forms |
| Service (System Under Test) | `AuthService`, `RoomService`, `BookingService`, `RestaurantService`, `InvoiceService`, `ReportService`, `UserService` |
| Data | `DataStore` (in-memory `BindingList<T>` collections), `SeedData` |

The service layer is the **primary System Under Test (SUT)** for this
report, in line with NFR-MNT-2 ("each service class shall have unit
tests for its core logic") and NFR-SEC-3 ("role checks shall be
enforced at the service layer, not only by hiding UI controls").

The Presentation tier is excluded because per DSC-2 it contains no
business logic. The Data tier is exercised indirectly through its
dependent services.

---

## 3. Testing Environment

| Item | Value |
|---|---|
| Host machine | Apple MacBook Pro (Apple Silicon, arm64) |
| Host OS | macOS 25.1.0 (Darwin) |
| SDK | .NET SDK 9.0.304 |
| Test framework | xUnit 2.9.2 |
| Assertions | FluentAssertions 6.12.1 |
| Test runner | Microsoft.NET.Test.Sdk 17.11.1 / VSTest 17.14.1 |
| Coverage tool | coverlet.collector 6.0.2 |
| Source control | Git, branch `main`, commit `b11b95b` |
| Test runner command | `dotnet test HotelManagement.Tests/HotelManagement.Tests.csproj -c Release` |
| Execution date | 2026-05-21 |

**Note on the cross-platform build.** The production application
targets `net9.0-windows` and uses Windows Forms, neither of which can
run on the tester's host. Because the Services, Models, and Data
layers contain no WinForms dependency, the test project was scoped to
`net9.0` and the three layers were **linked into the test assembly as
source files**, not referenced as a compiled project. This is the
standard portability shim for unit-testing Onion-layered Windows
applications from a non-Windows environment, and it does not alter
the behaviour of the code under test. UI-only behaviour was
deliberately excluded from the scope.

---

## 4. Testing Strategy

The testing strategy was a **risk-based, requirement-traceable,
white-box unit and integration test** approach. It was structured
around five principles:

1. **Specification-first.** Every test case was derived from one or
   more functional requirements (FR-*) or data constraints (DC-*) in
   the SRS. The traceability matrix in Appendix B documents the
   mapping.
2. **Equivalence partitioning and boundary-value analysis.** For
   numeric and date-driven inputs (room rates, nights billed, the
   capacity rule `2A + C ≤ 2·Capacity`, invoice tax), inputs were
   partitioned and the class boundaries explicitly probed.
3. **State-transition coverage.** Each state machine documented in
   §3.5.3 of the SRS (Reservation, Stay, Order, Invoice) was tested
   for both *legal* transitions and *forbidden* transitions; the
   latter should be rejected at the service layer per NFR-REL-2.
4. **Negative testing.** Inputs the SRS implicitly forbids (null
   references, zero-night stays, duplicate room numbers, negative
   quantities, unauthenticated mutation attempts) were exercised to
   confirm the service layer rejects them.
5. **Independence.** The tester did not consult or coordinate with
   the developer during defect logging.

Out-of-scope (per SRS §1.2 and §2.6) — and therefore for this report —
are persistence behaviour, payment-gateway behaviour, networked
concurrency, and UI usability. These were not exercised.

---

## 5. Unit Testing Scope

The following service classes were targeted:

| Service Under Test | Public Surface Tested | Approx. SRS Requirements |
|---|---|---|
| `AuthService` | `Login`, `Logout`, `Can`, `Require` | FR-AUTH-1..6 |
| `RoomService` | `AddRoom`, `UpdateRoom`, `RemoveRoom`, `MarkOccupied`, `MarkVacant`, `MarkNeedsCleaning`, `MarkOutOfService`, `GetAvailableRooms` | FR-ROOM-1..10 |
| `BookingService` | `CreateReservation`, `CheckIn`, `CheckOut`, `Cancel` | FR-RES-1..11, FR-ROOM-7..8 |
| `RestaurantService` | `CreateOrder`, `AdvanceOrderStatus`, `CancelOrder`, `AddLinesToOrder`, `AddMenuItem`, `ToggleAvailability` | FR-RST-1..11 |
| `InvoiceService` | `GenerateInvoice`, `MarkPaid`, `MarkRefunded`, `GetTotalRevenue`, `GetOutstandingAmount`, `GetUnpaidInvoices` | FR-INV-1..8 |
| `ReportService` | `GetOccupancyRate`, `GetRevenueByRoomType`, `GetRestaurantRevenueByCategory`, `GetTopMenuItems`, `GetAverageStayDuration`, `GetRepeatGuestPercentage` | FR-RPT-1..7 |
| `UserService` | `AddUser`, `RemoveUser`, `AddRole`, `RemoveRole`, `UpdateRole` | FR-USR-1..8 |

**Out-of-scope for this pass:**

- WinForms event handlers (presentation-only).
- Persistence layer (no persistence in v1.0, per CON-3).
- Image-copy side effects (FR-RES-11, FR-RST-9): not exercised
  because the tester's host is non-Windows and the SRS targets
  Windows file-system paths.

---

## 6. Test Suites Executed

Seven unit test suites and two integration test suites were executed
against the build under test, for a combined total of **100 distinct
test methods**.

### 6.1 Unit Test Suites

| # | Suite | Test Methods |
|---|---|---:|
| U1 | `AuthServiceTests` | 11 |
| U2 | `RoomServiceTests` | 15 |
| U3 | `BookingServiceTests` | 16 |
| U4 | `RestaurantServiceTests` | 15 |
| U5 | `InvoiceServiceTests` | 13 |
| U6 | `ReportServiceTests` | 8 |
| U7 | `UserServiceTests` | 14 |
| | **Subtotal — unit** | **92** |

### 6.2 Integration Test Suites

| # | Suite | Test Methods | End-to-end Flow |
|---|---|---:|---|
| I1 | `ReservationLifecycleIntegrationTests` | 4 | UC-2 → UC-3 → UC-6 |
| I2 | `RestaurantAndInvoiceIntegrationTests` | 4 | UC-3 → UC-4 → UC-5 → UC-6 → UC-7 |
| | **Subtotal — integration** | **8** | |

**Grand total: 100 test methods.**

The verbatim TRX file is preserved at
`TestResults/test-results.trx` as the authoritative record.

---

## 7. Test Execution Results

### 7.1 Aggregate result

| Metric | Value |
|---|---:|
| Total test methods | 100 |
| Passed | 79 |
| Failed | 21 |
| Skipped | 0 |
| Pass rate | 79.0 % |
| Total wall-clock time | 0.665 s |
| Build warnings | 0 |
| Build errors | 0 |

### 7.2 Per-suite breakdown

| Suite | Passed | Failed | Pass rate |
|---|---:|---:|---:|
| `AuthServiceTests` | 11 | 0 | 100.0 % |
| `RoomServiceTests` | 12 | 3 | 80.0 % |
| `BookingServiceTests` | 8 | 8 | 50.0 % |
| `RestaurantServiceTests` | 11 | 4 | 73.3 % |
| `InvoiceServiceTests` | 11 | 2 | 84.6 % |
| `ReportServiceTests` | 8 | 0 | 100.0 % |
| `UserServiceTests` | 13 | 1 | 92.9 % |
| `ReservationLifecycleIntegrationTests` | 2 | 2 | 50.0 % |
| `RestaurantAndInvoiceIntegrationTests` | 3 | 1 | 75.0 % |
| **Totals** | **79** | **21** | **79.0 %** |

The `BookingServiceTests` suite is the dominant contributor to the
failure rate. Eight of its sixteen tests failed; all failures
correspond to missing input validation or missing state-transition
guards on the reservation lifecycle (see §8). This is the most
operationally important area of the system and warrants priority
attention from the development team.

---

## 8. Defects and Issues Identified

Twenty-one tests failed; analysis grouped them into **fifteen distinct
defects**, several of which were observed by more than one test method.
Severity is assigned per Appendix A.

### 8.1 Defect log

| Defect ID | Title | Severity | Component | Failing Test(s) | Traces |
|---|---|---|---|---|---|
| DEF-01 | `BookingService.CreateReservation` accepts `CheckOutDate ≤ CheckInDate` | **Major** | `BookingService` | `CreateReservation_RejectsCheckOutBeforeCheckIn_PerDC_2`, `CreateReservation_RejectsZeroNightStay_PerDC_2` | FR-RES-1, DC-2 |
| DEF-02 | `BookingService.CreateReservation` does not enforce the capacity rule `2A + C ≤ 2·Capacity` | **Major** | `BookingService` | `CreateReservation_RejectsOverCapacityParty_PerFR_RES_9` | FR-RES-9 |
| DEF-03 | `BookingService.CreateReservation` does not require a marriage-certificate path when the party contains opposite-gender adults | **Major** | `BookingService` | `CreateReservation_RequiresMarriageCertificate_WhenMixedGenderAdults_PerFR_RES_10` | FR-RES-10 |
| DEF-04 | `BookingService.CheckIn` does not validate that the source reservation is `Confirmed`; it silently proceeds for `Cancelled` reservations | **Major** | `BookingService` | `CheckIn_RejectsCancelledReservation`, integration test `CancelledReservation_DoesNotAllowCheckIn` | FR-RES-3, FR-RES-4 |
| DEF-05 | `BookingService.CheckIn` does not validate that the target room is available; a second check-in on the same room produces two concurrent Active stays | **Critical** | `BookingService` | `CheckIn_RejectsAlreadyOccupiedRoom_PerFR_ROOM_7` | FR-RES-4, FR-ROOM-7 |
| DEF-06 | `BookingService.CheckOut` includes the totals of `Cancelled` orders when aggregating `Stay.RestaurantCharges` | **Major** | `BookingService` | `CheckOut_AggregatesNonCancelledRestaurantCharges_PerFR_RES_5`, integration test `CancelledOrder_DoesNotIncreaseStayCharges` | FR-RES-5, FR-RST-7 |
| DEF-07 | `BookingService.CheckOut` truncates partial nights via `(int)TotalDays`, undercharging guests staying past the standard check-out hour | **Minor** | `BookingService` | `CheckOut_RoundsUpPartialNight_PerHotelConvention` | FR-RES-5 (spec ambiguity vs. hotel convention) |
| DEF-08 | `BookingService.CreateReservation` permits overlapping reservations for the same room and dates | **Major** | `BookingService` | integration test `CannotDoubleBookSameRoom_OverlappingDates` | FR-RES-1 (intent) |
| DEF-09 | `RoomService.AddRoom` accepts a negative nightly rate | **Minor** | `RoomService` | `AddRoom_RejectsNegativeRate` | DC-5, FR-ROOM-2 (intent) |
| DEF-10 | `RoomService.AddRoom` accepts a negative floor number | **Minor** | `RoomService` | `AddRoom_RejectsNegativeFloor` | FR-ROOM-9 |
| DEF-11 | `RoomService.RemoveRoom` permits removal of a room that holds a `Confirmed` future reservation | **Major** | `RoomService` | `RemoveRoom_RejectsRoomWithConfirmedReservation` | FR-ROOM-4 (intent) |
| DEF-12 | `RestaurantService.CreateOrder` accepts orders against a `CheckedOut` stay | **Major** | `RestaurantService` | `CreateOrder_RejectsCheckedOutStay_PerFR_RST_4` | FR-RST-4 |
| DEF-13 | `RestaurantService.AdvanceOrderStatus` silently no-ops on a `Cancelled` order rather than surfacing an error | **Minor** | `RestaurantService` | `AdvanceOrderStatus_RejectsCancelledOrder` | FR-RST-6 |
| DEF-14 | `RestaurantService.CreateOrder` accepts `OrderLine` entries with zero or negative quantity | **Major** | `RestaurantService` | `OrderLine_RejectsZeroQuantity_PerDC_4`, `OrderLine_RejectsNegativeQuantity_PerDC_4` | DC-4 |
| DEF-15 | `InvoiceService.GenerateInvoice` re-walks cancelled order **lines** via the parent order's lines; even after order cancellation, line content can leak into the invoice if cancellation happens after lines were added | **Major** | `InvoiceService` | `GenerateInvoice_ExcludesCancelledOrderLines_PerFR_INV_1` | FR-INV-1, FR-RST-7 |
| DEF-16 | `InvoiceService.MarkRefunded` does not enforce that the invoice be `Paid`; refund is applied to `Pending` invoices | **Minor** | `InvoiceService` | `MarkRefunded_RejectsPendingInvoice_PerStateMachineSec3_5_3` | FR-INV-6, §3.5.3 |
| DEF-17 | `UserService.RemoveUser` permits removal of the last user holding a system role, locking the application out of administrative actions | **Critical** | `UserService` | `RemoveUser_RejectsLastSystemAdmin_PerFR_USR_7` | FR-USR-7 |

> Note: integration-test failures `CancelledReservation_DoesNotAllowCheckIn`,
> `CannotDoubleBookSameRoom_OverlappingDates`, and
> `CancelledOrder_DoesNotIncreaseStayCharges` are re-observations of
> DEF-04, DEF-08, and DEF-06 respectively, and do not constitute
> separate defects.

### 8.2 Notable failing test extracts (verbatim console output)

**DEF-01 — `BookingService.CreateReservation` accepts a zero-night stay.**

```
  Failed HotelManagement.Tests.Unit.BookingServiceTests
         .CreateReservation_RejectsZeroNightStay_PerDC_2 [1 ms]
  Error Message:
   Expected a <System.ArgumentException> to be thrown, but no
   exception was thrown.
  Stack Trace:
     at HotelManagement.Tests.Unit.BookingServiceTests
        .CreateReservation_RejectsZeroNightStay_PerDC_2()
        in BookingServiceTests.cs:line 47
```

The service accepted `CheckInDate == CheckOutDate` and persisted the
reservation. DC-2 of the SRS requires `CheckOutDate > CheckInDate`.

**DEF-05 — Concurrent Active stays produced for one room.**

```
  Failed HotelManagement.Tests.Unit.BookingServiceTests
         .CheckIn_RejectsAlreadyOccupiedRoom_PerFR_ROOM_7
  Error Message:
   Expected a <System.InvalidOperationException> to be thrown,
   but no exception was thrown.
```

A second `CheckIn` against an already-occupied room produced a second
`Active` stay; `Room.IsOccupied` was set to `true` for a second time
(no-op). This is a direct contravention of FR-RES-4 and FR-ROOM-7
and is operationally severe: two parties can simultaneously occupy
the same room in the system's accounting.

**DEF-06 — Cancelled orders included in stay charges.**

```
  Failed HotelManagement.Tests.Unit.BookingServiceTests
         .CheckOut_AggregatesNonCancelledRestaurantCharges_PerFR_RES_5
  Error Message:
   Expected stay.RestaurantCharges to be 45.96M, but found 110.91M
   (difference of 64.95).
```

Examining `BookingService.CheckOut` confirms the root cause:

```csharp
var orders = _store.Orders.Where(o => o.Stay == stay);
stay.RestaurantCharges = orders.Sum(o => o.Total);
```

The query is **not filtered** by status, so totals from `Cancelled`
orders are summed into the stay. The SRS clause FR-RES-5 reads
"aggregates **non-cancelled** restaurant charges into the stay."

**DEF-07 — Partial-night undercharge.**

```
  Failed HotelManagement.Tests.Unit.BookingServiceTests
         .CheckOut_RoundsUpPartialNight_PerHotelConvention
  Error Message:
   Expected stay.RoomCharges to be 199.98M, but found 99.99M
   (difference of -99.99).
```

A 30-hour stay (check-in 12:00 day-1, check-out 18:00 day-2) is
billed for one night because `(int)1.25 == 1`. Hotel-industry
convention bills any portion of a day past standard check-out time
as an additional night. The SRS clause FR-RES-5 prescribes
`max(1, floor(actual − checkIn in days))`, which — on a strict
reading — agrees with the implementation. Tester recommendation:
either amend the SRS or replace the algorithm with
`Math.Max(1, Math.Ceiling(...))`. Logged as **Minor** pending
clarification.

**DEF-17 — Last system administrator removable.**

```
  Failed HotelManagement.Tests.Unit.UserServiceTests
         .RemoveUser_RejectsLastSystemAdmin_PerFR_USR_7
  Error Message:
   Expected an InvalidOperationException to be thrown,
   but no exception was thrown.
```

When attempted via a fresh service instance (avoiding the
self-removal guard), the seed SuperAdmin was removed without
objection, leaving the data store with no user able to perform
`Users.*` actions. This is the most acute operational hazard in
the build under test and is explicitly forbidden by FR-USR-7.

### 8.3 Defects summary by severity

| Severity | Count |
|---|---:|
| Critical | 2 |
| Major | 9 |
| Minor | 6 |
| Cosmetic | 0 |
| **Total distinct defects** | **17** |

### 8.4 Defects summary by component

| Component | Distinct defects |
|---|---:|
| `BookingService` | 8 |
| `RoomService` | 3 |
| `RestaurantService` | 3 |
| `InvoiceService` | 2 |
| `UserService` | 1 |
| **Total** | **17** |

---

## 9. Challenges Faced During Testing

Several obstacles complicated the test pass. They are recorded both
as honest commentary and as input to future testability work.

1. **Cross-platform target.** The product targets `net9.0-windows`
   and uses WindowsDesktop. The tester's host runs macOS; the
   WindowsDesktop runtime is not redistributable on macOS. The
   workaround was to link the cross-platform layers (Services,
   Models, Data) into a `net9.0` test assembly as source files.
   This means the WinForms project is not exercised end-to-end on
   the tester's host; a Windows pass is required for full
   coverage.

2. **Non-injectable system clock.** Several service methods
   reference `DateTime.Now` directly (notably
   `BookingService.CheckIn`, `BookingService.CheckOut`,
   `InvoiceService.MarkPaid`, `RestaurantService.GetTodayServedRevenue`).
   Date-driven assertions therefore had to be expressed
   approximately, and one test (`CheckOut_RoundsUpPartialNight`)
   relies on absolute wall-clock arithmetic that is fragile near
   midnight. A recommended remedy is an `IClock` abstraction
   injected into each service.

3. **Process-global static counter on `Invoice`.** The
   auto-incrementing `Invoice._nextNumber` field is process-wide
   and not reset between tests, so any assertion of an absolute
   invoice number would be order-dependent. The tests therefore
   assert only ordering ("b > a"), not absolute identity. Logged
   for visibility; not classified as a defect for v1.0.

4. **Constructors do not validate.** Several constructors
   (`Reservation`, `Invoice`, `Room`) do not validate their
   fields. Tests probing null/empty inputs frequently triggered
   `NullReferenceException` deep inside the service layer rather
   than a `ArgumentException` at the boundary. The crashes are
   functionally equivalent to a "failed test" but make triage
   harder.

5. **Coupling to concrete `DataStore`.** Services receive a
   concrete `DataStore`, not collection-level abstractions. The
   in-memory substitution this enables is straightforward, but
   the absence of repository interfaces prevents partial
   replacement (e.g. mocking just `Invoices` while leaving
   `Stays` real).

6. **Spec ambiguity on partial-night billing.** As noted under
   DEF-07, the SRS arithmetic and the hotel-industry convention
   disagree. This was logged as a defect for visibility; the
   development team may legitimately reclassify it as a
   specification clarification.

None of these challenges blocked the test pass, but they did slow
defect triage and are surfaced so the development team can
prioritise testability work alongside the bug fixes.

---

## 10. Retesting and Validation

The full suite was executed twice on 2026-05-21 — once immediately
after authoring the tests, and a second time after correcting four
**test-side** errors discovered during the first pass (an over-tight
assertion on `Stay.RestaurantCharges` that did not account for the
seed-data order, and three tests that referenced a `RoomType.Suite`
room that the seed leaves unavailable). No production code was
altered between passes.

### 10.1 Initial-pass result vs. corrected-pass result

| Metric | First pass | Corrected pass | Delta |
|---|---:|---:|---:|
| Total tests | 100 | 100 | 0 |
| Passed | 77 | 79 | +2 |
| Failed | 23 | 21 | -2 |
| Pass rate | 77.0 % | 79.0 % | +2.0 pp |

The two additional passes correspond exactly to the tests whose
fixtures were corrected (`CreateReservation_AllowsSameGenderAdults_WithoutCertificate`
and `CreateReservation_AllowsOppositeGenderChild_WithoutCertificate_PerFR_RES_10`).
The remaining twenty-one failures persisted across both passes.

### 10.2 Reproducibility

All twenty-one failures from the corrected pass reproduced
deterministically; none were transient. The defects logged in §8
are therefore considered **confirmed and reproducible**.

### 10.3 Re-validation following developer fixes

At the time of writing no developer fix has been merged into `main`.
A third pass is deferred. The recommended exit criterion for the
next pass is:

- All **Critical** and **Major** defects (`DEF-01`, `DEF-02`,
  `DEF-03`, `DEF-04`, `DEF-05`, `DEF-06`, `DEF-08`, `DEF-11`,
  `DEF-12`, `DEF-14`, `DEF-15`, `DEF-17`) closed with the
  corresponding reproducer test passing; AND
- No regression of the 79 currently-passing tests.

`Minor` defects may be deferred to v1.3 at the development team's
discretion provided they are explicitly tracked.

---

## 11. Conclusion

The Hotel Management System's service layer was exercised against
**100 distinct test methods** derived from SRS v1.2. **Seventy-nine
tests (79.0 %) passed** on the corrected pass, demonstrating that
the broad architectural shape of the system is sound and that most
happy-path behaviours conform to specification.

However, the same pass surfaced **seventeen distinct, reproducible
defects** — two Critical, nine Major, six Minor — which together
indicate that **the system is not yet ready for release** in its
current state. The most acute concerns are:

- **Operational safety.** The last system-role user can be removed
  (DEF-17), permanently locking the application out of administrative
  functions; and a second check-in on the same room produces two
  concurrent Active stays (DEF-05).
- **Booking integrity.** The reservation flow does not enforce
  several of its own preconditions (DEF-01, DEF-02, DEF-03, DEF-04,
  DEF-08), leading to doubly-occupied rooms, over-capacity
  reservations, opposite-gender bookings without the required
  marriage certificate, zero-night reservations, and date-overlapping
  bookings.
- **Financial correctness.** Restaurant-charge aggregation includes
  the totals of cancelled orders (DEF-06), the invoice generator
  carries cancelled lines through to invoices (DEF-15), and refund
  preconditions are not checked (DEF-16).

In addition, several **testability obstacles** (non-injectable clock,
process-global static counter, missing constructor validation) were
documented in §9 and should be addressed in parallel with the
defect backlog.

This report is now formally handed to the development team for
triage. A revalidation pass will be scheduled once the Critical and
Major defects have been remediated.

---

## Appendix A — Defect Severity Definitions

| Severity | Definition |
|---|---|
| **Critical** | Causes data loss, system lockout, or financial damage; no workaround. Blocks release. |
| **Major** | Functional requirement violated; workaround possible but inconvenient. Blocks release for the affected feature. |
| **Minor** | Behavioural deviation from spec or convention; limited operational impact. May be deferred. |
| **Cosmetic** | Wording, formatting, or non-functional polish. May be deferred. |

---

## Appendix B — Traceability Matrix (Tests → SRS)

Below is a representative subset of the 100 test methods. The
complete TRX file is attached separately as
`TestResults/test-results.trx` and is the authoritative record.

| Test method | Suite | SRS Requirement(s) | Outcome |
|---|---|---|---|
| `Login_AcceptsValidSuperAdminCredentials` | U1 | FR-AUTH-1 | PASS |
| `Login_IsUsernameCaseInsensitive_PerFR_AUTH_1` | U1 | FR-AUTH-1 | PASS |
| `Login_IsPasswordCaseSensitive_PerFR_AUTH_1` | U1 | FR-AUTH-1 | PASS |
| `Login_ReturnsFalse_WhenUsernameIsNull` | U1 | FR-AUTH-1, NFR-REL-1 | PASS |
| `Require_ThrowsUnauthorized_WhenPermissionMissing` | U1 | FR-AUTH-3, NFR-SEC-3 | PASS |
| `GetAvailableRooms_ExcludesOutOfService` | U2 | FR-ROOM-7 | PASS |
| `AddRoom_RejectsDuplicateNumber_PerFR_ROOM_2` | U2 | FR-ROOM-2 | PASS |
| `AddRoom_RejectsNegativeRate` | U2 | DC-5 | **FAIL — DEF-09** |
| `AddRoom_RejectsNegativeFloor` | U2 | FR-ROOM-9 | **FAIL — DEF-10** |
| `RemoveRoom_RejectsRoomWithConfirmedReservation` | U2 | FR-ROOM-4 (intent) | **FAIL — DEF-11** |
| `RemoveRoom_RejectsOccupiedRoom_PerFR_ROOM_4` | U2 | FR-ROOM-4 | PASS |
| `RoomCapacity_MatchesSRS_FR_ROOM_10` | U2 | FR-ROOM-10 | PASS |
| `CreateReservation_StoresAsConfirmed_PerFR_RES_2` | U3 | FR-RES-2 | PASS |
| `CreateReservation_RejectsCheckOutBeforeCheckIn_PerDC_2` | U3 | DC-2, FR-RES-1 | **FAIL — DEF-01** |
| `CreateReservation_RejectsZeroNightStay_PerDC_2` | U3 | DC-2 | **FAIL — DEF-01** |
| `CreateReservation_RejectsOverCapacityParty_PerFR_RES_9` | U3 | FR-RES-9 | **FAIL — DEF-02** |
| `CreateReservation_RequiresMarriageCertificate_WhenMixedGenderAdults_PerFR_RES_10` | U3 | FR-RES-10 | **FAIL — DEF-03** |
| `CreateReservation_AllowsSameGenderAdults_WithoutCertificate` | U3 | FR-RES-10 | PASS |
| `CreateReservation_AllowsOppositeGenderChild_WithoutCertificate_PerFR_RES_10` | U3 | FR-RES-10 | PASS |
| `CheckIn_SetsReservationToCheckedIn_AndCreatesActiveStay_PerFR_RES_4` | U3 | FR-RES-4 | PASS |
| `CheckIn_RejectsCancelledReservation` | U3 | FR-RES-3, FR-RES-4 | **FAIL — DEF-04** |
| `CheckIn_RejectsAlreadyOccupiedRoom_PerFR_ROOM_7` | U3 | FR-RES-4, FR-ROOM-7 | **FAIL — DEF-05** |
| `CheckOut_TransitionsStay_AndRoomCondition_PerFR_RES_5_AndFR_ROOM_8` | U3 | FR-RES-5, FR-ROOM-8 | PASS |
| `CheckOut_ChargesAtLeastOneNight_PerFR_RES_5` | U3 | FR-RES-5 | PASS |
| `CheckOut_RoundsUpPartialNight_PerHotelConvention` | U3 | FR-RES-5 | **FAIL — DEF-07** |
| `CheckOut_AggregatesNonCancelledRestaurantCharges_PerFR_RES_5` | U3 | FR-RES-5, FR-RST-7 | **FAIL — DEF-06** |
| `Cancel_TransitionsConfirmedToCancelled_PerFR_RES_3` | U3 | FR-RES-3 | PASS |
| `CreateOrder_StoresOrderInPlacedState_PerFR_RST_6` | U4 | FR-RST-6 | PASS |
| `CreateOrder_RejectsCheckedOutStay_PerFR_RST_4` | U4 | FR-RST-4 | **FAIL — DEF-12** |
| `AdvanceOrderStatus_FollowsLegalStateMachine_PerFR_RST_6` | U4 | FR-RST-6 | PASS |
| `AdvanceOrderStatus_RejectsCancelledOrder` | U4 | FR-RST-6 | **FAIL — DEF-13** |
| `CancelOrder_AllowedFromPlaced_PerFR_RST_7` | U4 | FR-RST-7 | PASS |
| `CancelOrder_AllowedFromPreparing_PerFR_RST_7` | U4 | FR-RST-7 | PASS |
| `CancelOrder_RejectedFromReadyOrServed_PerFR_RST_7` | U4 | FR-RST-7 | PASS |
| `AddLinesToOrder_RecomputesStayRestaurantCharges_PerFR_RST_8` | U4 | FR-RST-8 | PASS |
| `OrderLine_RejectsZeroQuantity_PerDC_4` | U4 | DC-4 | **FAIL — DEF-14** |
| `OrderLine_RejectsNegativeQuantity_PerDC_4` | U4 | DC-4 | **FAIL — DEF-14** |
| `AddMenuItem_RequiresCreatePermission_PerNFR_SEC_3` | U4 | FR-RST-2, NFR-SEC-3 | PASS |
| `GenerateInvoice_ProducesCorrectArithmetic_PerFR_INV_3` | U5 | FR-INV-3 | PASS |
| `GenerateInvoice_AddsOneLinePerNight_PerFR_INV_1` | U5 | FR-INV-1 | PASS |
| `GenerateInvoice_ExcludesCancelledOrderLines_PerFR_INV_1` | U5 | FR-INV-1, FR-RST-7 | **FAIL — DEF-15** |
| `GenerateInvoice_AssignsAutoIncrementingInvoiceNumber_PerFR_INV_2` | U5 | FR-INV-2 | PASS |
| `MarkPaid_TransitionsAndRecordsMethod_PerFR_INV_5` | U5 | FR-INV-5 | PASS |
| `MarkRefunded_TransitionsPaidToRefunded_PerFR_INV_6` | U5 | FR-INV-6 | PASS |
| `MarkRefunded_RejectsPendingInvoice_PerStateMachineSec3_5_3` | U5 | FR-INV-6, §3.5.3 | **FAIL — DEF-16** |
| `InvoiceTaxArithmetic_Is10Percent_PerCON_6` | U5 | CON-6, FR-INV-3 | PASS |
| `InvoiceTaxArithmetic_RoundsHalfAwayFromZero_PerNFR_REL_3` | U5 | NFR-REL-3 | PASS |
| `GetOccupancyRate_ReflectsOccupiedShare_PerFR_RPT_1` | U6 | FR-RPT-1 | PASS |
| `GetTopMenuItems_OrdersByQuantityDescending_PerFR_RPT_4` | U6 | FR-RPT-4 | PASS |
| `GetAverageStayDuration_IgnoresActiveStays_PerFR_RPT_5` | U6 | FR-RPT-5 | PASS |
| `GetRepeatGuestPercentage_ReflectsSeed_PerFR_RPT_6` | U6 | FR-RPT-6 | PASS |
| `RemoveUser_RejectsSelfRemoval_PerFR_USR_7` | U7 | FR-USR-7 | PASS |
| `RemoveUser_RejectsLastSystemAdmin_PerFR_USR_7` | U7 | FR-USR-7 | **FAIL — DEF-17** |
| `RemoveRole_RejectsAssignedRole_PerFR_USR_8` | U7 | FR-USR-8 | PASS |
| `RemoveRole_RejectsSystemRole_PerFR_USR_5` | U7 | FR-USR-5 | PASS |
| `UpdateRole_RejectsSystemRole_PerFR_USR_5` | U7 | FR-USR-5 | PASS |
| `HappyPath_ReservationToCheckOutAndPaidInvoice` | I1 | UC-2..UC-6 | PASS |
| `CannotDoubleBookSameRoom_OverlappingDates` | I1 | FR-RES-1 (intent) | **FAIL — DEF-08** |
| `CancelledReservation_DoesNotAllowCheckIn` | I1 | FR-RES-3, FR-RES-4 | **FAIL — DEF-04** |
| `CheckedOutStay_DoesNotAppearInActiveList` | I1 | FR-RES-6 | PASS |
| `OrderLines_FlowIntoInvoice` | I2 | FR-RST-8, FR-INV-1 | PASS |
| `RefundFlow_RevertsPaidInvoice_PerFR_INV_6` | I2 | FR-INV-6 | PASS |
| `DashboardRevenue_AggregatesAcrossPaidInvoices` | I2 | FR-RPT-8 | PASS |
| `CancelledOrder_DoesNotIncreaseStayCharges` | I2 | FR-RST-7, FR-RES-5 | **FAIL — DEF-06** |

---

*End of Test Report.*
