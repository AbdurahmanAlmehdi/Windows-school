# Database Normalization

## Hotel Management System — UNF → 3NF Derivation

**Document Version:** 1.0
**Date:** 2026-05-29
**Author:** Abdurahman Ibrahem
**Target DBMS:** Microsoft SQL Server 2022
**Reference SRS:** v1.2 (2026-05-11) §3.5 *Logical Data Model*

---

## 1. Purpose

This document derives the relational schema of the Hotel Management
System from a denormalized real-world artifact (a paper *guest folio
receipt*) through the three normal forms. The target is **Third Normal
Form (3NF)**.

Boyce–Codd Normal Form (BCNF) and higher forms were considered but are
not required for this domain: no relation has overlapping non-trivial
candidate keys, and the additional rigidity would impede the
natural-key lookups (room number, invoice number, passport) that
front-of-house staff rely on.

## 2. Notation

| Symbol | Meaning |
|---|---|
| `pk` | Primary key (UUID surrogate) |
| `ak` | Alternate / candidate key (`UNIQUE` constraint) |
| `fk → T` | Foreign key referencing table `T` |
| `→` | Functional dependency (LHS determines RHS) |
| `{A, B}` | Composite of attributes A and B |
| ⟂ | Multi-valued / repeating group |

---

## 3. Starting artifact — Unnormalized Form (UNF)

Consider the paper *guest folio receipt* the front desk produces when
a guest checks out. One physical sheet captures **everything** about
one stay in a single wide row:

| Field | Sample value |
|---|---|
| GuestName | John Smith |
| GuestPhone | 555-0101 |
| GuestPassport | P10000001 |
| GuestGender | Male |
| GuestIsVip | TRUE |
| RoomNumber | 201 |
| RoomFloor | 2 |
| RoomType | Double |
| RoomRate | 149.99 |
| RoomCapacity | 2 |
| CheckInDate | 2026-05-19 |
| CheckOutDate | 2026-05-22 |
| MarriageCertId | MC-2024-0091 |
| **AccompanyingGuests** ⟂ | `"Mary Smith / F / 35 / P20000091"` |
| **OrderItems** ⟂ | `"Grilled Salmon × 1; Espresso × 2"` |
| OrderStatus | Served |
| InvoiceNumber | INV-1001 |
| InvoiceDate | 2026-05-22 |
| **InvoiceLines** ⟂ | `"Room 201 × 3 nights; Salmon × 1; Espresso × 2"` |
| PaymentStatus | Paid |
| PaymentMethod | CreditCard |
| PaymentDate | 2026-05-22 |
| Subtotal | 332.96 |
| Tax | 33.30 |
| Total | 366.26 |

### 3.1 Problems with UNF

1. **Repeating groups** — `AccompanyingGuests`, `OrderItems`, and
   `InvoiceLines` each contain a variable number of values that cannot
   sit atomically in a single column.
2. **Update anomaly** — changing the rate of Room 201 would require
   editing every past folio that references it.
3. **Insertion anomaly** — a brand-new room with no folios yet cannot
   be recorded at all under this design.
4. **Deletion anomaly** — deleting a guest's only folio loses the
   guest record entirely.

These four anomalies are the textbook motivation for normalizing.

---

## 4. First Normal Form (1NF)

> **Rule.** Every attribute is atomic, no repeating groups, every row
> uniquely identifiable.

Every multi-valued attribute is extracted to its own relation. Each
line of the original group becomes a row keyed back to the parent
record.

### 4.1 Resulting relations (1NF)

**`folios`** — one row per folio (still one wide row, but no lists inside):
- `folio_id` `pk`
- `guest_name`, `guest_phone`, `guest_passport`, `guest_gender`, `guest_is_vip`
- `room_number`, `room_floor`, `room_type`, `room_rate`, `room_capacity`
- `check_in_date`, `check_out_date`, `marriage_certificate_id`
- `invoice_number`, `invoice_date`, `payment_status`, `payment_method`, `payment_date`
- `subtotal`, `tax`, `total`

**`folio_accompanying`** — one row per accompanying person:
- `folio_id` `fk → folios`, `position`, `name`, `gender`, `age`, `passport`
- PK: `{folio_id, position}`

**`folio_order_items`** — one row per item ordered:
- `folio_id` `fk → folios`, `position`, `item_name`, `item_price`, `quantity`, `order_status`
- PK: `{folio_id, position}`

**`folio_invoice_lines`** — one row per invoice line:
- `folio_id` `fk → folios`, `position`, `description`, `quantity`, `unit_price`, `category`
- PK: `{folio_id, position}`

### 4.2 What 1NF still allows (the symptom 2NF will fix)

The repeating groups are gone, but the design is still pathological:

- `folios.room_rate` repeats for every folio that mentions Room 201.
  Changing the rate requires a multi-row update.
- `folios.guest_passport` repeats for every visit John Smith makes.
- `folio_order_items.item_price` repeats every time the dish is ordered.

These are **redundancies** caused by *partial dependencies* — the next
form eliminates them.

---

## 5. Second Normal Form (2NF)

> **Rule.** Already 1NF, **and** every non-key attribute is **fully**
> functionally dependent on the whole primary key (no partial
> dependency on a subset of a composite key).

The composite-key relations from 1NF are the obvious suspects.

### 5.1 `folio_order_items` — partial dependencies discovered

The PK is `{folio_id, position}`. Observed FDs:

- `{folio_id, position}` → `quantity, order_status`  ✓ (depend on the whole key)
- `item_name` → `item_price`  ✗ (price depends only on **what** was
  ordered, not **which folio** it appeared on — **partial dependency**)

**Fix.** Extract `menu_items`, keyed by dish, and replace
`item_name`/`item_price` in `folio_order_items` with `menu_item_id`
(FK).

### 5.2 `folio_invoice_lines` — frozen price, *not* a 2NF violation

`unit_price` on an invoice line is determined by the underlying item,
but the line records the *price at the moment of billing*, not the
live price. The dependency `line → unit_price` is on the line itself
(a billing event), not on a key subset of `{folio_id, position}`. We
therefore **keep** `unit_price` on the line. This is the standard
"billed-price snapshot" pattern and is intentionally not normalized
away.

### 5.3 The `folios` header itself — multiple aggregates conflated

`folios` was *not* keyed by a composite, but it conflates four
independent aggregates: **Guest**, **Room**, **Stay**, **Invoice** —
plus the **Reservation** that precedes the stay. Each is independently
identifiable and each carries its own non-key attributes. To make every
attribute depend on *one* full key, we split the folio into:

| New relation | Identifying attribute | Non-key attributes |
|---|---|---|
| `guests` | `guest_id` `pk` | `name`, `phone`, `passport` `ak`, `gender`, `is_vip`, `stay_count` |
| `rooms` | `room_id` `pk` | `number` `ak`, `floor`, `type`, `rate`, `capacity`, `is_occupied`, `condition` |
| `reservations` | `reservation_id` `pk` | `guest_id` `fk`, `room_id` `fk`, `check_in_date`, `check_out_date`, `status`, `marriage_certificate_id` |
| `reservation_accompanying` | `accompanying_id` `pk` | `reservation_id` `fk`, `name`, `gender`, `age`, `passport` |
| `stays` | `stay_id` `pk` | `guest_id`, `room_id`, `reservation_id`, `check_in_date`, `expected_check_out`, `actual_check_out`, `room_charges`, `restaurant_charges`, `status` |
| `restaurant_orders` | `order_id` `pk` | `stay_id` `fk`, `status`, `created_at` |
| `order_lines` | `line_id` `pk` | `order_id` `fk`, `menu_item_id` `fk`, `quantity`, `notes` |
| `menu_items` | `menu_item_id` `pk` | `name` `ak`, `price`, `category`, `is_available`, `description`, `image_path` |
| `invoices` | `invoice_id` `pk` | `invoice_number` `ak`, `stay_id`, `guest_id`, `room_id`, `invoice_date`, `payment_status`, `payment_method`, `payment_date` |
| `invoice_lines` | `line_id` `pk` | `invoice_id` `fk`, `description`, `quantity`, `unit_price`, `category` |

### 5.4 What 2NF still allows (the symptom 3NF will fix)

Every non-key attribute now depends on the *full* PK of its relation.
But two **transitive** dependencies remain:

1. In `rooms`: `room_id → type → capacity` — capacity is determined by
   the *type*, not by the specific room.
2. In users (to be modelled): `user_id → role → permission_set` —
   permissions belong to the role, not to the individual user.

These are 3NF violations.

---

## 6. Third Normal Form (3NF)

> **Rule.** Already 2NF, **and** no non-key attribute transitively
> depends on the primary key through another non-key attribute.
> Equivalently: every non-key attribute depends on **the key, the
> whole key, and nothing but the key**.

### 6.1 `rooms.capacity` — extracted to `room_types`

Today, `Room.Capacity` is computed in C# from `Room.Type` via a
`switch`:

```csharp
RoomType.Single    => 1
RoomType.Double    => 2
RoomType.Suite     => 4
RoomType.Deluxe    => 4
RoomType.Penthouse => 6
```

That is a textbook transitive dependency: `room_id → type → capacity`.

**Fix.**

**`room_types`** — `type_code` `pk`, `display_name`, `capacity`

`rooms.type` becomes `fk → room_types.type_code`. Capacity is now
stored **exactly once**, on the type, never duplicated across rooms.

### 6.2 Role-based permissions — extracted to `roles` and `role_permissions`

A naive `users.role` enum carrying a permission set would suffer the
transitive dependency `user_id → role → permissions`. The fix is the
classic RBAC extraction:

| Relation | Key | Non-key |
|---|---|---|
| `roles` | `role_id` `pk` | `name` `ak`, `is_system` |
| `role_permissions` | `{role_id, resource, action}` `pk` | (existence is the fact) |
| `users` | `user_id` `pk` | `username` `ak`, `password_hash`, `role_id` `fk` |

Permissions are now stored **once per `(role, resource, action)`** —
never per user.

### 6.3 Status enums — kept inline (a justified design choice)

The remaining enumerations
(`ReservationStatus`, `StayStatus`, `OrderStatus`, `PaymentStatus`,
`PaymentMethod`, `Gender`, `InvoiceLineCategory`, `RoomCondition`) are
**pure labels**: they carry no additional attributes (no display name,
no ordering, no metadata). Extracting them to lookup tables would not
improve 3NF compliance — the label *is* the value, with no transitive
attribute hanging off it.

We therefore keep them inline as
`CHECK (col IN ('a','b',…))` constraints on the parent columns. If a
future requirement attaches metadata to one of them (e.g. a
`display_name` or an `is_terminal` flag), the lookup-table refactor
becomes warranted; today it would be ceremony without normalization
benefit.

This is the same posture the SRS implicitly takes by using C# `enum`s
for these and a dedicated `Role` class only for the one case that
*does* carry attached attributes (permissions).

### 6.4 Final 3NF schema — table catalogue

| # | Relation | PK | Notable FKs |
|---|---|---|---|
| 1 | `room_types` | `type_code` | — |
| 2 | `rooms` | `room_id` (UUID) | `type → room_types` |
| 3 | `guests` | `guest_id` (UUID) | — |
| 4 | `roles` | `role_id` (UUID) | — |
| 5 | `role_permissions` | `{role_id, resource, action}` | `role_id → roles` |
| 6 | `users` | `user_id` (UUID) | `role_id → roles` |
| 7 | `menu_items` | `menu_item_id` (UUID) | — |
| 8 | `reservations` | `reservation_id` (UUID) | `guest_id`, `room_id` |
| 9 | `reservation_accompanying` | `accompanying_id` (UUID) | `reservation_id` |
| 10 | `stays` | `stay_id` (UUID) | `guest_id`, `room_id`, `reservation_id` |
| 11 | `restaurant_orders` | `order_id` (UUID) | `stay_id` |
| 12 | `order_lines` | `line_id` (UUID) | `order_id`, `menu_item_id` |
| 13 | `invoices` | `invoice_id` (UUID) | `stay_id`, `guest_id`, `room_id` |
| 14 | `invoice_lines` | `line_id` (UUID) | `invoice_id` |

---

## 7. Functional dependency catalogue (final 3NF)

The complete set of FDs the schema enforces, expressed concisely:

```
room_types:               type_code → display_name, capacity

rooms:                    room_id   → number, floor, type, rate,
                                      is_occupied, condition, maintenance_log
                          number    → room_id            (alternate key)

guests:                   guest_id  → name, phone, passport, gender,
                                      is_vip, stay_count
                          passport  → guest_id           (natural identifier)

roles:                    role_id   → name, is_system
                          name      → role_id

role_permissions:         {role_id, resource, action} → ∅
                              (the row's existence is itself the fact)

users:                    user_id   → username, password_hash, role_id
                          username  → user_id

menu_items:               menu_item_id → name, price, category,
                                         is_available, description, image_path

reservations:             reservation_id → guest_id, room_id,
                                           check_in_date, check_out_date,
                                           status, marriage_certificate_id

reservation_accompanying: accompanying_id → reservation_id, name,
                                            gender, age, passport

stays:                    stay_id   → guest_id, room_id, reservation_id,
                                      check_in_date, expected_check_out,
                                      actual_check_out, room_charges,
                                      restaurant_charges, status

restaurant_orders:        order_id  → stay_id, status, created_at

order_lines:              line_id   → order_id, menu_item_id, quantity, notes

invoices:                 invoice_id     → invoice_number, stay_id,
                                           guest_id, room_id, invoice_date,
                                           payment_status, payment_method,
                                           payment_date
                          invoice_number → invoice_id

invoice_lines:            line_id   → invoice_id, description, quantity,
                                      unit_price, category
```

Every FD's RHS sits in the same relation as its LHS, every LHS is a
key (PK or AK), and no non-key attribute determines another non-key
attribute. **The schema is in 3NF.**

---

## 8. Surrogate vs natural keys — why UUIDs

Every relation uses a **UUID surrogate** PK
(`UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()`), even when a natural key
exists. Natural keys (room number, invoice number, passport, role
name, username, menu-item name) are preserved as `UNIQUE` alternate
keys.

| Concern | UUID surrogate | Natural key |
|---|---|---|
| Immutability | Stable for the life of the row | Subject to change (room renumbering, passport renewal, business renaming) |
| Cross-row references | Compact, indexable, opaque | Mutable values cascade through every FK |
| Distributed generation | `NEWID()` is collision-free across instances | Sequences require coordination |
| Privacy | Opaque | Embeds business data (passport numbers) in every child row's FK |
| Course requirement | Explicitly requested by the instructor | — |

For sequential-write workloads where UUID v4's randomness causes
clustered-index fragmentation, SQL Server offers `NEWSEQUENTIALID()`
as a drop-in replacement. We use `NEWID()` here for two reasons: (1)
predictable test data when paired with literal `UNIQUEIDENTIFIER`
seeds in `db/seed_sqlserver.sql`; (2) keeping the schema portable to
other DBMSs that lack the sequential variant.

---

## 9. Changes driven by mid-term UI feedback

Two changes to the **attribute set** (not the normal form) follow from
instructor feedback at the mid-term review:

1. **`guests.passport`** is promoted to the **natural identifier** of
   a guest (previously secondary to phone). It becomes `NOT NULL
   UNIQUE`, the front-desk app keys all lookups by it, and phone
   (`contact`) becomes an optional attribute.
2. **`reservations.marriage_certificate_path`** is **replaced** by
   **`reservations.marriage_certificate_id`** `VARCHAR(64) NULL`. The
   system no longer stores certificate *images*; it records the
   certificate's issuing-authority ID. This eliminates the
   `MarriageCerts/` directory and the `File.Copy` call in
   `ReservationDialog`, removing a file-system side effect from the
   data layer.

Neither change affects normal form — both are attribute-level edits
within the existing 3NF structure.

---

## 10. Summary

- Started from a single denormalized *guest folio receipt*.
- **1NF** removed the multi-valued `Accompanying`, `OrderItems`, and
  `InvoiceLines` groups.
- **2NF** split the folio into seven independent aggregates — Guest,
  Room, Reservation, Stay, Order, Menu Item, Invoice — eliminating
  partial dependencies.
- **3NF** extracted `room_types` (resolving `type → capacity`) and
  `roles + role_permissions` (resolving `role → permissions`).
  Pure-label enums were kept inline as `CHECK` constraints with a
  documented justification.
- Every relation uses a UUID surrogate PK with the natural key
  preserved as a `UNIQUE` alternate.

The result is **fourteen relations in 3NF**, suitable for direct
translation to SQL Server DDL (see `db/schema_sqlserver.sql`) and
visualised in `Documentation/ERD.md`.

*End of normalization derivation.*
