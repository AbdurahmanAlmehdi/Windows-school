-- =====================================================================
-- Hotel Management System - SQL Server 2022 schema (v2.0)
--
-- Source:         SRS.md  §3.5 Logical Data Model
-- Normalization:  Documentation/Normalization.md  (UNF -> 3NF)
-- ER diagram:     Documentation/ERD.md
--
-- Conventions:
--   * Surrogate UUID PKs (UNIQUEIDENTIFIER DEFAULT NEWID()).
--   * Natural keys preserved as UNIQUE alternate keys.
--   * Pure-label enums enforced via CHECK constraints.
--   * Lookup tables only where an attribute hangs off the label
--     (room_types -> capacity).
--   * decimal(10,2) for money (NFR-REL-3 / DSC-4).
--   * Owned-lines relations use ON DELETE CASCADE; all other FKs
--     RESTRICT to preserve history.
--
-- v2.0 changes vs MySQL v1.2:
--   * Ported from MySQL 8.0 to SQL Server 2022.
--   * INT AUTO_INCREMENT  ->  UNIQUEIDENTIFIER DEFAULT NEWID()
--   * ENUM(...)          ->  VARCHAR + CHECK or room_types lookup
--   * BOOLEAN             ->  BIT
--   * TINYTEXT / TEXT     ->  NVARCHAR(MAX)
--   * `condition`         ->  [condition]
--   * Replaced reservations.marriage_certificate_path
--                            -> marriage_certificate_id VARCHAR(64) NULL
--     (mid-term UI feedback: store the issuing-authority ID,
--     never an image.)
--   * Promoted guests.passport to UNIQUE NOT NULL (natural identifier).
--   * Added trg_reservations_no_overlap (closes DEF-08).
--   * Added dbo.seq_invoice_number (replaces Invoice._nextNumber
--     static field flagged in StaticTestReport.md SC-04).
-- =====================================================================

USE [master];
GO

IF DB_ID(N'HotelManagement') IS NOT NULL
BEGIN
    ALTER DATABASE [HotelManagement] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [HotelManagement];
END
GO

CREATE DATABASE [HotelManagement];
GO

USE [HotelManagement];
GO

-- ---------------------------------------------------------------------
-- room_types  (3NF lookup: capacity hangs off the type, not the room)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.room_types (
    type_code     VARCHAR(20)    NOT NULL,
    display_name  NVARCHAR(64)   NOT NULL,
    capacity      INT            NOT NULL,
    CONSTRAINT pk_room_types PRIMARY KEY (type_code),
    CONSTRAINT chk_room_types_capacity CHECK (capacity > 0)
);
GO

-- ---------------------------------------------------------------------
-- rooms
-- ---------------------------------------------------------------------
CREATE TABLE dbo.rooms (
    room_id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_rooms_id     DEFAULT NEWID(),
    number           INT              NOT NULL,
    floor            INT              NOT NULL CONSTRAINT df_rooms_floor  DEFAULT 1,
    type             VARCHAR(20)      NOT NULL,
    rate             DECIMAL(10,2)    NOT NULL,
    is_occupied      BIT              NOT NULL CONSTRAINT df_rooms_occ    DEFAULT 0,
    [condition]      VARCHAR(20)      NOT NULL CONSTRAINT df_rooms_cond   DEFAULT 'Clean',
    maintenance_log  NVARCHAR(MAX)    NULL,
    CONSTRAINT pk_rooms       PRIMARY KEY (room_id),
    CONSTRAINT uk_rooms_number UNIQUE (number),
    CONSTRAINT fk_rooms_type   FOREIGN KEY (type) REFERENCES dbo.room_types (type_code),
    CONSTRAINT chk_rooms_rate      CHECK (rate >= 0),    -- DC-5 / DEF-09
    CONSTRAINT chk_rooms_floor     CHECK (floor >= 0),   -- FR-ROOM-9 / DEF-10
    CONSTRAINT chk_rooms_condition CHECK ([condition] IN ('Clean','NeedsCleaning','OutOfService'))
);
GO

-- ---------------------------------------------------------------------
-- guests   (passport is the natural identifier - mid-term UI feedback)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.guests (
    guest_id    UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_guests_id     DEFAULT NEWID(),
    name        NVARCHAR(120)    NOT NULL,
    contact     NVARCHAR(64)     NULL,                                          -- phone, optional
    passport    VARCHAR(64)      NOT NULL,
    gender      VARCHAR(10)      NOT NULL CONSTRAINT df_guests_gender DEFAULT 'Male',
    is_vip      BIT              NOT NULL CONSTRAINT df_guests_vip    DEFAULT 0,
    stay_count  INT              NOT NULL CONSTRAINT df_guests_stays  DEFAULT 0,
    CONSTRAINT pk_guests           PRIMARY KEY (guest_id),
    CONSTRAINT uk_guests_passport  UNIQUE (passport),
    CONSTRAINT chk_guests_gender   CHECK (gender IN ('Male','Female')),
    CONSTRAINT chk_guests_stays    CHECK (stay_count >= 0)
);
GO

-- ---------------------------------------------------------------------
-- roles + role_permissions   (3NF RBAC: permissions live on the role)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.roles (
    role_id    UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_roles_id     DEFAULT NEWID(),
    name       NVARCHAR(64)     NOT NULL,
    is_system  BIT              NOT NULL CONSTRAINT df_roles_system DEFAULT 0,
    CONSTRAINT pk_roles      PRIMARY KEY (role_id),
    CONSTRAINT uk_roles_name UNIQUE (name)
);
GO

CREATE TABLE dbo.role_permissions (
    role_id   UNIQUEIDENTIFIER NOT NULL,
    resource  VARCHAR(20)      NOT NULL,
    [action]  VARCHAR(10)      NOT NULL,
    CONSTRAINT pk_role_permissions PRIMARY KEY (role_id, resource, [action]),
    CONSTRAINT fk_rp_role          FOREIGN KEY (role_id) REFERENCES dbo.roles (role_id) ON DELETE CASCADE,
    CONSTRAINT chk_rp_resource     CHECK (resource IN ('Rooms','Reservations','MenuItems','Orders','Invoices','Users')),
    CONSTRAINT chk_rp_action       CHECK ([action]  IN ('Create','Read','Update','Delete'))
);
GO

-- ---------------------------------------------------------------------
-- users
-- ---------------------------------------------------------------------
CREATE TABLE dbo.users (
    user_id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_users_id DEFAULT NEWID(),
    username       NVARCHAR(64)     NOT NULL,
    password_hash  NVARCHAR(255)    NOT NULL,                            -- BCrypt / Argon2id (NFR-SEC-1)
    role_id        UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT pk_users         PRIMARY KEY (user_id),
    CONSTRAINT uk_users_username UNIQUE (username),
    CONSTRAINT fk_users_role    FOREIGN KEY (role_id) REFERENCES dbo.roles (role_id)
);
GO

-- ---------------------------------------------------------------------
-- menu_items
-- ---------------------------------------------------------------------
CREATE TABLE dbo.menu_items (
    menu_item_id  UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_menu_id    DEFAULT NEWID(),
    name          NVARCHAR(120)    NOT NULL,
    price         DECIMAL(10,2)    NOT NULL,
    category      NVARCHAR(64)     NOT NULL,
    is_available  BIT              NOT NULL CONSTRAINT df_menu_avail DEFAULT 1,
    description   NVARCHAR(MAX)    NULL,
    image_path    NVARCHAR(500)    NULL,
    CONSTRAINT pk_menu_items PRIMARY KEY (menu_item_id),
    CONSTRAINT chk_menu_price CHECK (price >= 0)
);
GO

-- ---------------------------------------------------------------------
-- reservations   (marriage_certificate_id, NOT path - mid-term feedback)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.reservations (
    reservation_id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_res_id     DEFAULT NEWID(),
    guest_id                 UNIQUEIDENTIFIER NOT NULL,
    room_id                  UNIQUEIDENTIFIER NOT NULL,
    check_in_date            DATE             NOT NULL,
    check_out_date           DATE             NOT NULL,
    status                   VARCHAR(20)      NOT NULL CONSTRAINT df_res_status DEFAULT 'Confirmed',
    marriage_certificate_id  VARCHAR(64)      NULL,
    CONSTRAINT pk_reservations  PRIMARY KEY (reservation_id),
    CONSTRAINT fk_res_guest     FOREIGN KEY (guest_id) REFERENCES dbo.guests (guest_id),
    CONSTRAINT fk_res_room      FOREIGN KEY (room_id)  REFERENCES dbo.rooms  (room_id),
    CONSTRAINT chk_res_status   CHECK (status IN ('Pending','Confirmed','CheckedIn','Cancelled','Completed')),
    CONSTRAINT chk_res_dates    CHECK (check_out_date > check_in_date)       -- DC-2 / DEF-01
);
GO
CREATE INDEX ix_res_guest  ON dbo.reservations(guest_id);
CREATE INDEX ix_res_room   ON dbo.reservations(room_id);
CREATE INDEX ix_res_status ON dbo.reservations(status);
GO

-- ---------------------------------------------------------------------
-- reservation_accompanying
-- ---------------------------------------------------------------------
CREATE TABLE dbo.reservation_accompanying (
    accompanying_id  UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_acc_id       DEFAULT NEWID(),
    reservation_id   UNIQUEIDENTIFIER NOT NULL,
    name             NVARCHAR(120)    NOT NULL,
    gender           VARCHAR(10)      NOT NULL CONSTRAINT df_acc_gender   DEFAULT 'Male',
    age              INT              NOT NULL,
    passport         VARCHAR(64)      NOT NULL CONSTRAINT df_acc_passport DEFAULT '',
    CONSTRAINT pk_accompanying  PRIMARY KEY (accompanying_id),
    CONSTRAINT fk_acc_res       FOREIGN KEY (reservation_id) REFERENCES dbo.reservations (reservation_id) ON DELETE CASCADE,
    CONSTRAINT chk_acc_gender   CHECK (gender IN ('Male','Female')),
    CONSTRAINT chk_acc_age      CHECK (age > 0 AND age <= 120)
);
GO
CREATE INDEX ix_acc_res ON dbo.reservation_accompanying(reservation_id);
GO

-- ---------------------------------------------------------------------
-- stays
-- ---------------------------------------------------------------------
CREATE TABLE dbo.stays (
    stay_id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_stay_id        DEFAULT NEWID(),
    guest_id             UNIQUEIDENTIFIER NOT NULL,
    room_id              UNIQUEIDENTIFIER NOT NULL,
    reservation_id       UNIQUEIDENTIFIER NULL,                                       -- DC-7: at most one
    check_in_date        DATETIME2(0)     NOT NULL,
    expected_check_out   DATETIME2(0)     NOT NULL,
    actual_check_out     DATETIME2(0)     NULL,
    room_charges         DECIMAL(10,2)    NOT NULL CONSTRAINT df_stay_room_chg  DEFAULT 0,
    restaurant_charges   DECIMAL(10,2)    NOT NULL CONSTRAINT df_stay_rest_chg  DEFAULT 0,
    status               VARCHAR(20)      NOT NULL CONSTRAINT df_stay_status    DEFAULT 'Active',
    CONSTRAINT pk_stays        PRIMARY KEY (stay_id),
    CONSTRAINT fk_stay_guest   FOREIGN KEY (guest_id)       REFERENCES dbo.guests       (guest_id),
    CONSTRAINT fk_stay_room    FOREIGN KEY (room_id)        REFERENCES dbo.rooms        (room_id),
    CONSTRAINT fk_stay_res     FOREIGN KEY (reservation_id) REFERENCES dbo.reservations (reservation_id),
    CONSTRAINT chk_stay_status  CHECK (status IN ('Active','CheckedOut')),
    CONSTRAINT chk_stay_actual  CHECK (actual_check_out IS NULL OR actual_check_out >= check_in_date),
    CONSTRAINT chk_stay_charges CHECK (room_charges >= 0 AND restaurant_charges >= 0)
);
GO
CREATE INDEX ix_stay_guest  ON dbo.stays(guest_id);
CREATE INDEX ix_stay_room   ON dbo.stays(room_id);
CREATE INDEX ix_stay_res    ON dbo.stays(reservation_id);
CREATE INDEX ix_stay_status ON dbo.stays(status);
GO

-- ---------------------------------------------------------------------
-- restaurant_orders
-- ---------------------------------------------------------------------
CREATE TABLE dbo.restaurant_orders (
    order_id    UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_ord_id      DEFAULT NEWID(),
    stay_id     UNIQUEIDENTIFIER NOT NULL,
    status      VARCHAR(20)      NOT NULL CONSTRAINT df_ord_status  DEFAULT 'Placed',
    created_at  DATETIME2(0)     NOT NULL CONSTRAINT df_ord_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT pk_restaurant_orders PRIMARY KEY (order_id),
    CONSTRAINT fk_ord_stay   FOREIGN KEY (stay_id) REFERENCES dbo.stays (stay_id) ON DELETE CASCADE,
    CONSTRAINT chk_ord_status CHECK (status IN ('Placed','Preparing','Ready','Served','Cancelled'))
);
GO
CREATE INDEX ix_ord_stay   ON dbo.restaurant_orders(stay_id);
CREATE INDEX ix_ord_status ON dbo.restaurant_orders(status);
CREATE INDEX ix_ord_date   ON dbo.restaurant_orders(created_at);
GO

-- ---------------------------------------------------------------------
-- order_lines
-- ---------------------------------------------------------------------
CREATE TABLE dbo.order_lines (
    line_id       UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_oline_id  DEFAULT NEWID(),
    order_id      UNIQUEIDENTIFIER NOT NULL,
    menu_item_id  UNIQUEIDENTIFIER NOT NULL,
    quantity      INT              NOT NULL CONSTRAINT df_oline_qty DEFAULT 1,
    notes         NVARCHAR(255)    NULL,
    CONSTRAINT pk_order_lines  PRIMARY KEY (line_id),
    CONSTRAINT fk_oline_order  FOREIGN KEY (order_id)     REFERENCES dbo.restaurant_orders (order_id) ON DELETE CASCADE,
    CONSTRAINT fk_oline_item   FOREIGN KEY (menu_item_id) REFERENCES dbo.menu_items        (menu_item_id),
    CONSTRAINT chk_oline_qty   CHECK (quantity >= 1)              -- DC-4 / DEF-14
);
GO
CREATE INDEX ix_oline_order ON dbo.order_lines(order_id);
CREATE INDEX ix_oline_item  ON dbo.order_lines(menu_item_id);
GO

-- ---------------------------------------------------------------------
-- invoices
-- ---------------------------------------------------------------------
CREATE TABLE dbo.invoices (
    invoice_id      UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_inv_id     DEFAULT NEWID(),
    invoice_number  VARCHAR(20)      NOT NULL,
    stay_id         UNIQUEIDENTIFIER NOT NULL,
    guest_id        UNIQUEIDENTIFIER NOT NULL,
    room_id         UNIQUEIDENTIFIER NOT NULL,
    invoice_date    DATETIME2(0)     NOT NULL,
    payment_status  VARCHAR(20)      NOT NULL CONSTRAINT df_inv_status DEFAULT 'Pending',
    payment_method  VARCHAR(20)      NULL,
    payment_date    DATETIME2(0)     NULL,
    CONSTRAINT pk_invoices      PRIMARY KEY (invoice_id),
    CONSTRAINT uk_inv_number    UNIQUE (invoice_number),
    CONSTRAINT fk_inv_stay      FOREIGN KEY (stay_id)  REFERENCES dbo.stays  (stay_id),
    CONSTRAINT fk_inv_guest     FOREIGN KEY (guest_id) REFERENCES dbo.guests (guest_id),
    CONSTRAINT fk_inv_room      FOREIGN KEY (room_id)  REFERENCES dbo.rooms  (room_id),
    CONSTRAINT chk_inv_status   CHECK (payment_status IN ('Pending','Paid','Refunded')),
    CONSTRAINT chk_inv_method   CHECK (payment_method IS NULL
                                       OR payment_method IN ('Cash','CreditCard','DebitCard','BankTransfer')),
    -- DC-6: payment_method is set iff payment_status in (Paid, Refunded)
    CONSTRAINT chk_inv_consistency CHECK (
        (payment_status = 'Pending'
            AND payment_method IS NULL
            AND payment_date IS NULL)
        OR
        (payment_status IN ('Paid','Refunded')
            AND payment_method IS NOT NULL)
    )
);
GO
CREATE INDEX ix_inv_stay   ON dbo.invoices(stay_id);
CREATE INDEX ix_inv_guest  ON dbo.invoices(guest_id);
CREATE INDEX ix_inv_status ON dbo.invoices(payment_status);
CREATE INDEX ix_inv_date   ON dbo.invoices(invoice_date);
GO

-- ---------------------------------------------------------------------
-- invoice_lines
-- ---------------------------------------------------------------------
CREATE TABLE dbo.invoice_lines (
    line_id      UNIQUEIDENTIFIER NOT NULL CONSTRAINT df_iline_id  DEFAULT NEWID(),
    invoice_id   UNIQUEIDENTIFIER NOT NULL,
    description  NVARCHAR(255)    NOT NULL,
    quantity     INT              NOT NULL CONSTRAINT df_iline_qty DEFAULT 1,
    unit_price   DECIMAL(10,2)    NOT NULL,
    category     VARCHAR(20)      NOT NULL,
    CONSTRAINT pk_invoice_lines  PRIMARY KEY (line_id),
    CONSTRAINT fk_iline_inv      FOREIGN KEY (invoice_id) REFERENCES dbo.invoices (invoice_id) ON DELETE CASCADE,
    CONSTRAINT chk_iline_qty     CHECK (quantity >= 1),
    CONSTRAINT chk_iline_price   CHECK (unit_price >= 0),
    CONSTRAINT chk_iline_category CHECK (category IN ('RoomCharge','RestaurantCharge'))
);
GO
CREATE INDEX ix_iline_inv ON dbo.invoice_lines(invoice_id);
GO

-- ---------------------------------------------------------------------
-- Sequence for human-readable invoice numbers
-- (replaces the static Invoice._nextNumber field flagged as
--  StaticTestReport.md SC-04.)
-- ---------------------------------------------------------------------
CREATE SEQUENCE dbo.seq_invoice_number
    AS INT
    START WITH 1001
    INCREMENT BY 1
    NO CYCLE
    CACHE 50;
GO

CREATE OR ALTER PROCEDURE dbo.sp_next_invoice_number
    @next VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @next = CONCAT('INV-', CAST(NEXT VALUE FOR dbo.seq_invoice_number AS VARCHAR(10)));
END;
GO

-- ---------------------------------------------------------------------
-- Trigger: no overlapping reservations on the same room
-- (closes DEF-08 at the data layer.)
-- ---------------------------------------------------------------------
CREATE OR ALTER TRIGGER dbo.trg_reservations_no_overlap
ON dbo.reservations
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM   inserted i
        JOIN   dbo.reservations r
            ON r.room_id        = i.room_id
           AND r.reservation_id <> i.reservation_id
           AND r.status         IN ('Pending','Confirmed','CheckedIn')
           AND i.status         IN ('Pending','Confirmed','CheckedIn')
           AND r.check_in_date  <  i.check_out_date
           AND i.check_in_date  <  r.check_out_date
    )
    BEGIN
        RAISERROR('Reservation overlaps an existing reservation on the same room.', 16, 1);
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    END
END;
GO

-- =====================================================================
-- End of schema.
-- Apply seed data with db/seed_sqlserver.sql.
-- =====================================================================
