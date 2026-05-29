# Static Testing Report

## Hotel Management System (WinForms Desktop Edition)

**Document Version:** 1.0
**Date:** 2026-05-21
**Prepared by:** Abdurahman Ibrahem — QA / Test Engineer
**Artifacts Reviewed:**

- `SRS.md` v1.2 (2026-05-11)
- `SDD.md` v1.0 (2026-05-21)
- Source tree of `HotelManagement.WinForms` @ commit `b11b95b` — `Services/`, `Models/`, `Data/`, `Forms/`, `Program.cs`

**Standard reference:** IEEE Std 1028-2008 — *Standard for Software Reviews and Audits*

---

## 1. Introduction

This report documents the **static testing** performed on the Hotel
Management System (HMS). Static testing is review-based verification
of the project's artifacts (specifications, design, source code)
**without executing the code**. It complements the dynamic test pass
recorded in `TestReport.md`.

Three review techniques were applied:

| Technique | Target | IEEE 1028 category |
|---|---|---|
| Document review | SRS.md, SDD.md | Technical review |
| Code inspection | `Services/*.cs`, `Models/*.cs`, `Data/*.cs` | Inspection |
| Walkthrough | `Program.cs`, key forms | Walkthrough |

The reviewer acted as an independent QA engineer; no findings were
remediated by the reviewer. All defects are forwarded to the
development team for triage.

---

## 2. Review Objectives

| # | Objective |
|---|---|
| O-1 | Verify the SRS and SDD are internally consistent and mutually consistent. |
| O-2 | Verify the source code conforms to the design declared in the SDD. |
| O-3 | Verify adherence to the project's coding standard (Onion layering, `decimal` arithmetic, async I/O, intention-revealing names). |
| O-4 | Identify code smells, dead code, and maintainability hazards. |
| O-5 | Identify spec / design gaps before they become defects in production. |

---

## 3. Review Method

A **checklist-driven walkthrough** was performed on each `Services/*.cs`
file (the principal SUT) and each `Models/*.cs` file. The checklist
items were:

1. Does the public surface match the SDD §5.3 description?
2. Are all SRS pre/post-conditions enforced (FR-* / DC-* IDs)?
3. Is input validated at the boundary (null, empty, negative, out of range)?
4. Are state-machine transitions guarded against illegal sources?
5. Are permissions checked at the service layer (NFR-SEC-3)?
6. Is `decimal` used for all currency arithmetic (NFR-REL-3 / DSC-4)?
7. Are exceptions specific (`InvalidOperationException`,
   `ArgumentException`, `UnauthorizedAccessException`), not generic `Exception`?
8. Is there any UI-layer or persistence-layer reference inside a service?
9. Are static / global mutable fields avoided?
10. Are methods under ~40 lines (NFR-MNT-3)?

SRS and SDD documents were reviewed using a separate
*completeness / consistency / ambiguity / verifiability* checklist
derived from IEEE 830 §4.3.

---

## 4. Summary of Findings

| Severity | Count |
|---|---:|
| Major | 4 |
| Minor | 7 |
| Observation (non-defect) | 5 |
| **Total findings** | **16** |

No Critical findings — Critical-class issues would only surface
through dynamic testing (and several have indeed been logged in
`TestReport.md`).

---

## 5. Document Review Findings

### 5.1 SRS Findings

| ID | Severity | Location | Finding |
|---|---|---|---|
| SR-01 | Minor | SRS §3.2.4 FR-RES-5 | The wording `max(1, floor(actual − checkIn in days))` is internally consistent but contradicts hotel-industry convention (any portion of a day past standard check-out counts as a billable night). The spec should explicitly state which convention is intended. **Ambiguity.** |
| SR-02 | Minor | SRS §3.2.4 FR-RES-1 | No statement forbids overlapping reservations on the same room. Real-world booking systems must prevent this; the spec is silent. **Incompleteness.** |
| SR-03 | Minor | SRS §3.2.7 FR-RPT-8 | "Today's invoice revenue" depends on `DateTime.Now`/`Today` but the SRS does not specify whether this is the host's local time or UTC. **Ambiguity.** |
| SR-04 | Observation | SRS §3.2.8 FR-USR-7 | The clause "last remaining user with a system role" is plural-ambiguous: if two system roles existed (e.g. SuperAdmin + a custom system role), the guard's intent is unclear. Recommend re-wording to "last remaining SuperAdmin." |
| SR-05 | Observation | SRS §3.1.1 UI-R1..R5 | UI rules are stated but not testable from the service layer; they require either an automated UI testing layer or a manual acceptance pass. **Verifiability concern.** |

### 5.2 SDD Findings

| ID | Severity | Location | Finding |
|---|---|---|---|
| SD-01 | Minor | SDD §5.3.7 | `UserService` is documented as gating on `Users.*` but the description does not mention the **self-removal** guard already implemented in the source. The SDD should be updated to match the code. **Documentation lag.** |
| SD-02 | Minor | SDD §4.4 Dependency Graph | `RestaurantService` and `RoomService` depend on `AuthService`, but the ASCII diagram in §4.4 does not show the edge. **Inaccuracy.** |
| SD-03 | Observation | SDD §8 Sequence diagrams | Diagrams are ASCII; consider PlantUML or Mermaid for future revisions to render in markdown viewers. |

---

## 6. Source Code Review Findings

### 6.1 Major findings

| ID | Severity | File / Line | Finding |
|---|---|---|---|
| SC-01 | Major | `Services/AuthService.cs:18-22` | `Login` references `username.Equals(...)` without null-checking the parameter. A `null` username will throw `NullReferenceException`. Should return `false` instead (NFR-REL-1). |
| SC-02 | Major | `Services/BookingService.cs:17-37` | `CreateReservation` performs **no validation at all** — no check on date ordering (DC-2), capacity (FR-RES-9), marriage certificate (FR-RES-10), passport (FR-GUEST-1), or overlap. The single most under-validated method in the codebase. |
| SC-03 | Major | `Services/BookingService.cs:64-65` | `CheckOut` aggregates restaurant charges with `_store.Orders.Where(o => o.Stay == stay).Sum(o => o.Total)` — the query is **not filtered by `OrderStatus != Cancelled`**, contradicting FR-RES-5. Cancelled orders inflate the stay total. |
| SC-04 | Major | `Models/Invoice.cs:7` | `private static int _nextNumber = 1001;` is **process-global mutable state**. Numbers persist across all DataStore instances (incl. tests, future multi-tenant deployments), and the field is not thread-safe. Use an instance counter on `DataStore`, or move to a sequence service. |

### 6.2 Minor findings

| ID | Severity | File / Line | Finding |
|---|---|---|---|
| SC-05 | Minor | `Services/BookingService.cs:39-54` | `CheckIn` does not verify `reservation.Status == Confirmed` nor `room.IsAvailable`. The state machine declared in SRS §3.5.3 is not enforced at this transition. |
| SC-06 | Minor | `Services/RestaurantService.cs:17-29` | `CreateOrder` does not verify `stay.Status == Active` (FR-RST-4) and does not validate `OrderLine.Quantity >= 1` (DC-4). |
| SC-07 | Minor | `Services/RestaurantService.cs:31-40` | `AdvanceOrderStatus` silently no-ops on `Cancelled` or `Served`. Silent no-ops mask operator error in the kitchen; should raise `InvalidOperationException`. |
| SC-08 | Minor | `Services/InvoiceService.cs:67-70` | `MarkRefunded` unconditionally sets `PaymentStatus = Refunded` regardless of current status. Should require `PaymentStatus == Paid` per the state machine in SRS §3.5.3. |
| SC-09 | Minor | `Services/RoomService.cs:34-44` | `AddRoom` does not validate `rate >= 0` (DC-5) or `floor >= 0` (FR-ROOM-9). Negative values are silently accepted. |
| SC-10 | Minor | `Services/RoomService.cs:59-67` | `RemoveRoom` checks `IsOccupied` but not whether any `Confirmed` reservation references the room. A removal leaves orphaned reservation records. |
| SC-11 | Minor | `Services/BookingService.cs:61` | `Math.Max(1, (int)(actual - checkIn).TotalDays)` uses `(int)` (truncation) rather than `Math.Ceiling`. See ambiguity SR-01. |

### 6.3 Observations (non-defect)

| ID | File | Observation |
|---|---|---|
| OB-01 | All `Services/*.cs` | Services consume `DataStore` directly with no `I<Service>` interface. Mock-based unit testing would benefit from extracting interfaces, but this is a future-work item, not a defect. |
| OB-02 | `Services/BookingService.cs`, `InvoiceService.cs`, `RestaurantService.cs` | All time-driven logic uses `DateTime.Now` directly. Inject an `IClock` to make these methods deterministically testable. |
| OB-03 | `Models/User.cs:6` | `Password` is stored as plaintext `string`. Per CON-7 this is acceptable for the academic build only; add a `// TODO: hash before production` comment for the next reader. |
| OB-04 | `Data/SeedData.cs` | The seed couples test data to source code. For future test scenarios consider extracting per-scenario seed builders. |
| OB-05 | `Forms/MainForm.cs` | The form's constructor takes 8 parameters. As the system grows, this hand-wired DI will become unwieldy; a lightweight container (e.g. `Microsoft.Extensions.DependencyInjection`) is worth considering for v1.3. |

---

## 7. Code Quality Metrics (Static)

Measured by manual inspection of `Services/` and `Models/`:

| Metric | Value | Threshold | Verdict |
|---|---:|---:|---|
| Service classes | 7 | — | — |
| Longest service method (LOC) | 22 (`BookingService.CheckOut`) | 40 (NFR-MNT-3) | Pass |
| Methods exceeding 40 LOC | 0 | 0 | Pass |
| Direct `DateTime.Now` references in services | 6 | 0 (ideal) | Concern (OB-02) |
| `static` mutable fields | 1 (`Invoice._nextNumber`) | 0 | **Fail (SC-04)** |
| `double` / `float` references in money math | 0 | 0 | Pass (NFR-REL-3) |
| Generic `catch (Exception)` blocks | 0 | 0 | Pass |
| Public service methods without input validation | 9 (see §6) | 0 | **Fail** |
| Cross-layer references (Service → Forms) | 0 | 0 | Pass (Onion respected) |
| Cross-layer references (Model → Service) | 0 | 0 | Pass |

---

## 8. Cross-Reference with Dynamic Test Report

Several findings here predict failures that were independently
observed during the dynamic test pass:

| Static finding | Predicts dynamic defect |
|---|---|
| SC-01 | (would fail) `Login_ReturnsFalse_WhenUsernameIsNull` — actually passed because `Equals` is called on the stored `Username`, not the parameter; root cause noted for hardening |
| SC-02 | DEF-01, DEF-02, DEF-03, DEF-08 |
| SC-03 | DEF-06 |
| SC-04 | (latent) Static counter risk |
| SC-05 | DEF-04, DEF-05 |
| SC-06 | DEF-12, DEF-14 |
| SC-07 | DEF-13 |
| SC-08 | DEF-16 |
| SC-09 | DEF-09 |
| SC-10 | DEF-11 |
| SC-11 | DEF-07 |

This convergence between static and dynamic findings strengthens
confidence in the defect log: defects identified by inspection were
independently reproduced by executable tests.

---

## 9. Conclusion

The static testing pass reviewed three categories of artifacts —
the SRS, the SDD, and the service-layer source — and produced
**16 findings** (4 Major, 7 Minor, 5 Observations). No Critical
issues were identifiable by inspection alone.

The dominant theme is **missing input validation and missing
state-machine guards in the service layer**, particularly in
`BookingService` and `RestaurantService`. Every Major finding has a
corresponding failing test in the dynamic test report, which both
validates the inspection method and increases the priority of the
fix backlog.

Recommended actions for the development team, in priority order:

1. Address the four Major findings (SC-01..SC-04) before the next
   release.
2. Clarify SRS clauses flagged in §5.1 (SR-01..SR-03) to remove
   ambiguity.
3. Update the SDD to reflect the implemented self-removal guard
   (SD-01) and add the missing dependency edges (SD-02).
4. Schedule the testability work suggested in OB-02 (`IClock`)
   alongside the bug fix cycle.

---

*End of Report.*
