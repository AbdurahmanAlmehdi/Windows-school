# Entity-Relationship Diagram

## Hotel Management System

**Document Version:** 1.0
**Date:** 2026-05-29
**Author:** Abdurahman Ibrahem
**Target DBMS:** Microsoft SQL Server 2022
**Companion document:** `Documentation/Normalization.md`
**Schema file:** `db/schema_sqlserver.sql`

---

## 1. Notation

This document uses Chen-style cardinality on a crow's-foot diagram
rendered with Mermaid `erDiagram`. Read each relationship from left to
right:

| Reading | Meaning |
|---|---|
| `A ||--o{ B` | Each `A` has **zero or more** `B`; each `B` has **exactly one** `A` |
| `A ||--|| B` | Exactly-one to exactly-one |
| `A ||--o| B` | Each `A` has **at most one** `B`; each `B` has exactly one `A` |
| `A }o--o{ B` | Many-to-many (resolved by a junction table) |

`PK` marks the primary key; `FK` a foreign key; `UK` a unique alternate
key.

---

## 2. ER Diagram

```mermaid
erDiagram
    ROOM_TYPES ||--o{ ROOMS                       : "categorises"
    ROOMS      ||--o{ RESERVATIONS                : "booked-as"
    ROOMS      ||--o{ STAYS                       : "occupied-by"
    ROOMS      ||--o{ INVOICES                    : "billed-on"
    GUESTS     ||--o{ RESERVATIONS                : "makes"
    GUESTS     ||--o{ STAYS                       : "checks-in-as"
    GUESTS     ||--o{ INVOICES                    : "billed-to"
    RESERVATIONS ||--o{ RESERVATION_ACCOMPANYING  : "lists"
    RESERVATIONS ||--o| STAYS                     : "becomes"
    STAYS      ||--o{ RESTAURANT_ORDERS           : "incurs"
    STAYS      ||--o| INVOICES                    : "produces"
    RESTAURANT_ORDERS ||--o{ ORDER_LINES          : "contains"
    MENU_ITEMS ||--o{ ORDER_LINES                 : "appears-on"
    INVOICES   ||--o{ INVOICE_LINES               : "contains"
    ROLES      ||--o{ ROLE_PERMISSIONS            : "grants"
    ROLES      ||--o{ USERS                       : "assigned-to"

    ROOM_TYPES {
        varchar  type_code      PK
        nvarchar display_name
        int      capacity
    }
    ROOMS {
        uuid     room_id        PK
        int      number         UK
        int      floor
        varchar  type           FK
        decimal  rate
        bit      is_occupied
        varchar  condition
        nvarchar maintenance_log
    }
    GUESTS {
        uuid     guest_id       PK
        nvarchar name
        nvarchar contact        "phone (nullable)"
        varchar  passport       UK
        varchar  gender
        bit      is_vip
        int      stay_count
    }
    ROLES {
        uuid     role_id        PK
        nvarchar name           UK
        bit      is_system
    }
    ROLE_PERMISSIONS {
        uuid     role_id        PK_FK
        varchar  resource       PK
        varchar  action         PK
    }
    USERS {
        uuid     user_id        PK
        nvarchar username       UK
        nvarchar password_hash
        uuid     role_id        FK
    }
    MENU_ITEMS {
        uuid     menu_item_id   PK
        nvarchar name
        decimal  price
        nvarchar category
        bit      is_available
        nvarchar description
        nvarchar image_path
    }
    RESERVATIONS {
        uuid     reservation_id PK
        uuid     guest_id       FK
        uuid     room_id        FK
        date     check_in_date
        date     check_out_date
        varchar  status
        varchar  marriage_certificate_id "ID, not image"
    }
    RESERVATION_ACCOMPANYING {
        uuid     accompanying_id PK
        uuid     reservation_id  FK
        nvarchar name
        varchar  gender
        int      age
        varchar  passport
    }
    STAYS {
        uuid     stay_id            PK
        uuid     guest_id           FK
        uuid     room_id            FK
        uuid     reservation_id     FK "nullable - walk-in stays"
        datetime check_in_date
        datetime expected_check_out
        datetime actual_check_out   "nullable"
        decimal  room_charges
        decimal  restaurant_charges
        varchar  status
    }
    RESTAURANT_ORDERS {
        uuid     order_id      PK
        uuid     stay_id       FK
        varchar  status
        datetime created_at
    }
    ORDER_LINES {
        uuid     line_id       PK
        uuid     order_id      FK
        uuid     menu_item_id  FK
        int      quantity
        nvarchar notes
    }
    INVOICES {
        uuid     invoice_id     PK
        varchar  invoice_number UK
        uuid     stay_id        FK
        uuid     guest_id       FK
        uuid     room_id        FK
        datetime invoice_date
        varchar  payment_status
        varchar  payment_method "nullable"
        datetime payment_date   "nullable"
    }
    INVOICE_LINES {
        uuid     line_id      PK
        uuid     invoice_id   FK
        nvarchar description
        int      quantity
        decimal  unit_price
        varchar  category
    }
```

---

## 3. Entity catalogue

The full attribute-level breakdown. Domains use SQL Server types.

### 3.1 `room_types`  *(lookup)*

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `type_code` | `VARCHAR(20)` | NO | PK | e.g. `Single`, `Suite` |
| `display_name` | `NVARCHAR(64)` | NO | | Human-readable label |
| `capacity` | `INT` | NO | | `> 0` |

### 3.2 `rooms`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `room_id` | `UNIQUEIDENTIFIER` | NO | PK | `DEFAULT NEWID()` |
| `number` | `INT` | NO | UK | Natural key |
| `floor` | `INT` | NO | | `>= 0` |
| `type` | `VARCHAR(20)` | NO | FK → `room_types` | |
| `rate` | `DECIMAL(10,2)` | NO | | `>= 0` |
| `is_occupied` | `BIT` | NO | | Default `0` |
| `condition` | `VARCHAR(20)` | NO | | `IN ('Clean','NeedsCleaning','OutOfService')` |
| `maintenance_log` | `NVARCHAR(MAX)` | YES | | |

### 3.3 `guests`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `guest_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `name` | `NVARCHAR(120)` | NO | | |
| `contact` | `NVARCHAR(64)` | YES | | Phone — optional after mid-term feedback |
| `passport` | `VARCHAR(64)` | NO | UK | **Natural identifier** |
| `gender` | `VARCHAR(10)` | NO | | `IN ('Male','Female')` |
| `is_vip` | `BIT` | NO | | Default `0` |
| `stay_count` | `INT` | NO | | `>= 0` |

### 3.4 `roles`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `role_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `name` | `NVARCHAR(64)` | NO | UK | |
| `is_system` | `BIT` | NO | | SuperAdmin = `1` |

### 3.5 `role_permissions`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `role_id` | `UNIQUEIDENTIFIER` | NO | PK + FK → `roles` | `ON DELETE CASCADE` |
| `resource` | `VARCHAR(20)` | NO | PK | `IN ('Rooms','Reservations','MenuItems','Orders','Invoices','Users')` |
| `action` | `VARCHAR(10)` | NO | PK | `IN ('Create','Read','Update','Delete')` |

### 3.6 `users`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `user_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `username` | `NVARCHAR(64)` | NO | UK | Case-insensitive lookup (collation default) |
| `password_hash` | `NVARCHAR(255)` | NO | | BCrypt / Argon2id per NFR-SEC-1 |
| `role_id` | `UNIQUEIDENTIFIER` | NO | FK → `roles` | |

### 3.7 `menu_items`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `menu_item_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `name` | `NVARCHAR(120)` | NO | | |
| `price` | `DECIMAL(10,2)` | NO | | `>= 0` |
| `category` | `NVARCHAR(64)` | NO | | |
| `is_available` | `BIT` | NO | | Default `1` |
| `description` | `NVARCHAR(MAX)` | YES | | |
| `image_path` | `NVARCHAR(500)` | YES | | |

### 3.8 `reservations`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `reservation_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `guest_id` | `UNIQUEIDENTIFIER` | NO | FK → `guests` | |
| `room_id` | `UNIQUEIDENTIFIER` | NO | FK → `rooms` | |
| `check_in_date` | `DATE` | NO | | |
| `check_out_date` | `DATE` | NO | | `> check_in_date` (CHECK) |
| `status` | `VARCHAR(20)` | NO | | `IN ('Pending','Confirmed','CheckedIn','Cancelled','Completed')` |
| `marriage_certificate_id` | `VARCHAR(64)` | YES | | **ID, not image** (mid-term feedback) |

Plus a trigger `trg_reservations_no_overlap` enforcing DEF-08 (no
overlapping active reservations on the same room).

### 3.9 `reservation_accompanying`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `accompanying_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `reservation_id` | `UNIQUEIDENTIFIER` | NO | FK → `reservations` | `ON DELETE CASCADE` |
| `name` | `NVARCHAR(120)` | NO | | |
| `gender` | `VARCHAR(10)` | NO | | `IN ('Male','Female')` |
| `age` | `INT` | NO | | `> 0 AND <= 120` |
| `passport` | `VARCHAR(64)` | NO | | May be `''` if minor without passport |

### 3.10 `stays`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `stay_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `guest_id` | `UNIQUEIDENTIFIER` | NO | FK → `guests` | |
| `room_id` | `UNIQUEIDENTIFIER` | NO | FK → `rooms` | |
| `reservation_id` | `UNIQUEIDENTIFIER` | YES | FK → `reservations` | DC-7: at most one source |
| `check_in_date` | `DATETIME2(0)` | NO | | |
| `expected_check_out` | `DATETIME2(0)` | NO | | |
| `actual_check_out` | `DATETIME2(0)` | YES | | `>= check_in_date` when set |
| `room_charges` | `DECIMAL(10,2)` | NO | | `>= 0`, default `0` |
| `restaurant_charges` | `DECIMAL(10,2)` | NO | | `>= 0`, default `0` |
| `status` | `VARCHAR(20)` | NO | | `IN ('Active','CheckedOut')` |

### 3.11 `restaurant_orders`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `order_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `stay_id` | `UNIQUEIDENTIFIER` | NO | FK → `stays` | `ON DELETE CASCADE` |
| `status` | `VARCHAR(20)` | NO | | `IN ('Placed','Preparing','Ready','Served','Cancelled')` |
| `created_at` | `DATETIME2(0)` | NO | | Default `SYSUTCDATETIME()` |

### 3.12 `order_lines`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `line_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `order_id` | `UNIQUEIDENTIFIER` | NO | FK → `restaurant_orders` | `ON DELETE CASCADE` |
| `menu_item_id` | `UNIQUEIDENTIFIER` | NO | FK → `menu_items` | |
| `quantity` | `INT` | NO | | `>= 1` (DC-4) |
| `notes` | `NVARCHAR(255)` | YES | | |

### 3.13 `invoices`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `invoice_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `invoice_number` | `VARCHAR(20)` | NO | UK | e.g. `INV-1001` |
| `stay_id` | `UNIQUEIDENTIFIER` | NO | FK → `stays` | |
| `guest_id` | `UNIQUEIDENTIFIER` | NO | FK → `guests` | |
| `room_id` | `UNIQUEIDENTIFIER` | NO | FK → `rooms` | |
| `invoice_date` | `DATETIME2(0)` | NO | | |
| `payment_status` | `VARCHAR(20)` | NO | | `IN ('Pending','Paid','Refunded')` |
| `payment_method` | `VARCHAR(20)` | YES | | `IN ('Cash','CreditCard','DebitCard','BankTransfer')` |
| `payment_date` | `DATETIME2(0)` | YES | | |

Plus a composite CHECK enforcing DC-6: `payment_method` is set iff
`payment_status IN ('Paid','Refunded')`.

`invoice_number` is generated from `dbo.seq_invoice_number` (SQL Server
sequence) — this replaces the static `Invoice._nextNumber` field
flagged in `StaticTestReport.md` SC-04.

### 3.14 `invoice_lines`

| Attribute | Type | Null? | Key | Notes |
|---|---|---|---|---|
| `line_id` | `UNIQUEIDENTIFIER` | NO | PK | |
| `invoice_id` | `UNIQUEIDENTIFIER` | NO | FK → `invoices` | `ON DELETE CASCADE` |
| `description` | `NVARCHAR(255)` | NO | | |
| `quantity` | `INT` | NO | | `>= 1` |
| `unit_price` | `DECIMAL(10,2)` | NO | | `>= 0` (billed-price snapshot) |
| `category` | `VARCHAR(20)` | NO | | `IN ('RoomCharge','RestaurantCharge')` |

---

## 4. Relationship catalogue

| # | Parent → Child | Cardinality | Trigger / On Delete | Source |
|---|---|---|---|---|
| R1 | `room_types` → `rooms` | 1 : N | RESTRICT | 3NF extraction |
| R2 | `guests` → `reservations` | 1 : N | RESTRICT | FR-RES-1 |
| R3 | `rooms` → `reservations` | 1 : N | RESTRICT | FR-RES-1 |
| R4 | `reservations` → `reservation_accompanying` | 1 : N | **CASCADE** | FR-RES-9 |
| R5 | `reservations` → `stays` | 1 : 0..1 | RESTRICT | FR-RES-4 / DC-7 |
| R6 | `guests` → `stays` | 1 : N | RESTRICT | FR-RES-4 |
| R7 | `rooms` → `stays` | 1 : N | RESTRICT | FR-RES-4 |
| R8 | `stays` → `restaurant_orders` | 1 : N | **CASCADE** | FR-RST-4 |
| R9 | `restaurant_orders` → `order_lines` | 1 : N | **CASCADE** | FR-RST-8 |
| R10 | `menu_items` → `order_lines` | 1 : N | RESTRICT | FR-RST-2 |
| R11 | `stays` → `invoices` | 1 : 0..1 | RESTRICT | FR-INV-1 |
| R12 | `guests` → `invoices` | 1 : N | RESTRICT | FR-INV-1 |
| R13 | `rooms` → `invoices` | 1 : N | RESTRICT | FR-INV-1 |
| R14 | `invoices` → `invoice_lines` | 1 : N | **CASCADE** | FR-INV-3 |
| R15 | `roles` → `role_permissions` | 1 : N | **CASCADE** | FR-USR-5 |
| R16 | `roles` → `users` | 1 : N | RESTRICT | FR-USR-3 |

`CASCADE` is used only for **owned-line** relationships
(reservation → accompanying, order → lines, invoice → lines,
role → permissions). All other FKs are `RESTRICT` so deletions of
referenced rows fail loudly instead of corrupting history.

---

## 5. Indexes

| Index | Table | Columns | Rationale |
|---|---|---|---|
| `pk_*` | every table | PK | Clustered by default in SQL Server |
| `uk_rooms_number` | `rooms` | `number` | Natural-key lookup by front-desk |
| `uk_guests_passport` | `guests` | `passport` | Mid-term feedback — primary lookup |
| `uk_users_username` | `users` | `username` | Login (FR-AUTH-1) |
| `uk_roles_name` | `roles` | `name` | Role lookup |
| `uk_inv_number` | `invoices` | `invoice_number` | External invoice reference |
| `ix_res_room` | `reservations` | `room_id` | Overlap check (DEF-08 trigger) |
| `ix_res_status` | `reservations` | `status` | "Active reservations" reports |
| `ix_stay_status` | `stays` | `status` | Occupancy reports (FR-RPT-1) |
| `ix_ord_status` | `restaurant_orders` | `status` | Kitchen queue / cancelled-filter (DEF-06) |
| `ix_inv_date` | `invoices` | `invoice_date` | "Today's revenue" (FR-RPT-8) |
| `ix_inv_status` | `invoices` | `payment_status` | Outstanding-invoices report |

---

## 6. Constraints summary

Beyond PKs and FKs the schema enforces the following business rules
declaratively (no application code required):

| ID | Constraint | Enforces |
|---|---|---|
| `chk_rooms_rate` | `rate >= 0` | DC-5 (DEF-09) |
| `chk_rooms_floor` | `floor >= 0` | FR-ROOM-9 (DEF-10) |
| `chk_res_dates` | `check_out_date > check_in_date` | DC-2 (DEF-01) |
| `chk_acc_age` | `age > 0 AND age <= 120` | data sanity |
| `chk_stay_actual` | `actual_check_out >= check_in_date` | temporal sanity |
| `chk_stay_charges` | `room_charges >= 0 AND restaurant_charges >= 0` | accounting |
| `chk_oline_qty` | `quantity >= 1` | DC-4 (DEF-14) |
| `chk_iline_qty` | `quantity >= 1` | accounting |
| `chk_iline_price` | `unit_price >= 0` | accounting |
| `chk_inv_consistency` | payment method set ⇔ status in (Paid, Refunded) | DC-6 |
| `trg_reservations_no_overlap` | no two active reservations overlap on one room | DEF-08 |

Together with the application-layer validation introduced in Phase 2,
this gives **belt-and-braces** enforcement: a corrupt insert is
rejected by the database even if a future caller forgets the
service-layer guard.

---

*End of ERD document.*
