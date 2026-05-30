-- =====================================================================
-- Hotel Management System - SQL Server seed data (v2.0)
--
-- Mirrors Data/SeedData.cs so the test suite and the production
-- application see identical fixtures. All UUIDs are deterministic so
-- integration tests can reference them by literal.
--
-- Run order:  schema_sqlserver.sql  ->  seed_sqlserver.sql
--
-- UUID convention used for legibility (all-zero "address book"):
--   roles               : 11111111-...
--   users               : 22222222-...
--   room_types          : 33333333-...
--   rooms               : 44444444-...-0000-0000-00000000XXXX  (XXXX = number)
--   guests              : 55555555-...
--   menu_items          : 66666666-...
--   reservations        : 77777777-...
--   reservation_accomp. : 88888888-...
--   stays               : 99999999-...
--   restaurant_orders   : AAAAAAAA-...
--   order_lines         : BBBBBBBB-...
--   invoices            : CCCCCCCC-...
--   invoice_lines       : DDDDDDDD-...
-- =====================================================================

USE [HotelManagement];
GO

SET XACT_ABORT ON;
BEGIN TRAN;

-- ---------------------------------------------------------------------
-- room_types
-- ---------------------------------------------------------------------
INSERT INTO dbo.room_types (type_code, display_name, capacity) VALUES
    ('Single',    N'Single',     1),
    ('Double',    N'Double',     2),
    ('Suite',     N'Suite',      4),
    ('Deluxe',    N'Deluxe',     4),
    ('Penthouse', N'Penthouse',  6);

-- ---------------------------------------------------------------------
-- roles
-- ---------------------------------------------------------------------
DECLARE @role_super  UNIQUEIDENTIFIER = '11111111-1111-1111-1111-000000000001';
DECLARE @role_mgr    UNIQUEIDENTIFIER = '11111111-1111-1111-1111-000000000002';
DECLARE @role_staff  UNIQUEIDENTIFIER = '11111111-1111-1111-1111-000000000003';

INSERT INTO dbo.roles (role_id, name, is_system) VALUES
    (@role_super, N'SuperAdmin', 1),
    (@role_mgr,   N'Manager',    0),
    (@role_staff, N'Staff',      0);

-- ---------------------------------------------------------------------
-- role_permissions
-- ---------------------------------------------------------------------
-- SuperAdmin: every (resource, action) pair.
INSERT INTO dbo.role_permissions (role_id, resource, [action])
SELECT @role_super, r.resource, a.action
FROM   (VALUES ('Rooms'),('Reservations'),('MenuItems'),
              ('Orders'),('Invoices'),('Users')) AS r(resource)
CROSS JOIN
       (VALUES ('Create'),('Read'),('Update'),('Delete')) AS a(action);

-- Manager: everything except Users.*
INSERT INTO dbo.role_permissions (role_id, resource, [action])
SELECT @role_mgr, r.resource, a.action
FROM   (VALUES ('Rooms'),('Reservations'),('MenuItems'),
              ('Orders'),('Invoices')) AS r(resource)
CROSS JOIN
       (VALUES ('Create'),('Read'),('Update'),('Delete')) AS a(action);

-- Staff: read-only on catalogue, CRUD-light on the operational entities.
INSERT INTO dbo.role_permissions (role_id, resource, [action]) VALUES
    (@role_staff, 'Rooms',        'Read'),
    (@role_staff, 'Reservations', 'Create'),
    (@role_staff, 'Reservations', 'Read'),
    (@role_staff, 'Reservations', 'Update'),
    (@role_staff, 'MenuItems',    'Read'),
    (@role_staff, 'Orders',       'Create'),
    (@role_staff, 'Orders',       'Read'),
    (@role_staff, 'Orders',       'Update'),
    (@role_staff, 'Invoices',     'Read'),
    (@role_staff, 'Invoices',     'Update');

-- ---------------------------------------------------------------------
-- users (password_hash is a placeholder; replace with a real
--        BCrypt/Argon2id hash before any production deployment.)
-- ---------------------------------------------------------------------
INSERT INTO dbo.users (user_id, username, password_hash, role_id) VALUES
    ('22222222-2222-2222-2222-000000000001', N'superadmin',
        N'$2a$REPLACE_WITH_BCRYPT_HASH_superadmin', @role_super),
    ('22222222-2222-2222-2222-000000000002', N'staff',
        N'$2a$REPLACE_WITH_BCRYPT_HASH_staff',      @role_staff);

-- ---------------------------------------------------------------------
-- rooms  (room_id encodes the room number for human legibility)
-- ---------------------------------------------------------------------
INSERT INTO dbo.rooms (room_id, number, floor, type, rate, is_occupied, [condition], maintenance_log) VALUES
    ('44444444-4444-4444-4444-000000000101', 101, 1, 'Single',     99.99, 0, 'Clean',         NULL),
    ('44444444-4444-4444-4444-000000000102', 102, 1, 'Single',     99.99, 0, 'Clean',         NULL),
    ('44444444-4444-4444-4444-000000000201', 201, 2, 'Double',    149.99, 1, 'Clean',         NULL),
    ('44444444-4444-4444-4444-000000000202', 202, 2, 'Double',    149.99, 0, 'Clean',         NULL),
    ('44444444-4444-4444-4444-000000000301', 301, 3, 'Suite',     249.99, 1, 'Clean',         NULL),
    ('44444444-4444-4444-4444-000000000302', 302, 3, 'Suite',     249.99, 0, 'NeedsCleaning', NULL),
    ('44444444-4444-4444-4444-000000000401', 401, 4, 'Deluxe',    349.99, 0, 'Clean',         NULL),
    ('44444444-4444-4444-4444-000000000402', 402, 4, 'Deluxe',    349.99, 0, 'Clean',         NULL),
    ('44444444-4444-4444-4444-000000000501', 501, 5, 'Penthouse', 599.99, 0, 'OutOfService',  N'Plumbing repair scheduled'),
    ('44444444-4444-4444-4444-000000000502', 502, 5, 'Penthouse', 599.99, 0, 'Clean',         NULL);

-- ---------------------------------------------------------------------
-- guests  (passport is the natural identifier)
-- ---------------------------------------------------------------------
DECLARE @g_john    UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000001';
DECLARE @g_jane    UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000002';
DECLARE @g_bob     UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000003';
DECLARE @g_alice   UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000004';
DECLARE @g_charlie UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000005';
DECLARE @g_diana   UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000006';

INSERT INTO dbo.guests (guest_id, name, contact, passport, gender, is_vip, stay_count) VALUES
    (@g_john,    N'John Smith',     N'555-0101', 'P10000001', 'Male',   1, 5),
    (@g_jane,    N'Jane Doe',       N'555-0102', 'P10000002', 'Female', 0, 2),
    (@g_bob,     N'Bob Johnson',    N'555-0103', 'P10000003', 'Male',   0, 1),
    (@g_alice,   N'Alice Williams', N'555-0104', 'P10000004', 'Female', 1, 8),
    (@g_charlie, N'Charlie Brown',  N'555-0105', 'P10000005', 'Male',   0, 0),
    (@g_diana,   N'Diana Prince',   N'555-0106', 'P10000006', 'Female', 0, 1);

-- ---------------------------------------------------------------------
-- menu_items
-- ---------------------------------------------------------------------
DECLARE @m_salmon UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000003';
DECLARE @m_steak  UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000004';
DECLARE @m_esp    UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000011';
DECLARE @m_capp   UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000012';

INSERT INTO dbo.menu_items (menu_item_id, name, price, category, is_available, image_path) VALUES
    ('66666666-6666-6666-6666-000000000001', N'Caesar Salad',       12.99, N'Starters',    1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000002', N'Tomato Soup',         8.99, N'Starters',    1, N'Assets/menu_placeholder.jpg'),
    (@m_salmon,                              N'Grilled Salmon',     24.99, N'Main Course', 1, N'Assets/menu_placeholder.jpg'),
    (@m_steak,                               N'Beef Steak',         32.99, N'Main Course', 1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000005', N'Chicken Pasta',      18.99, N'Main Course', 1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000006', N'Margherita Pizza',   15.99, N'Main Course', 1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000007', N'Chocolate Cake',      9.99, N'Desserts',    1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000008', N'Ice Cream Sundae',    7.99, N'Desserts',    1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000009', N'Tiramisu',           10.99, N'Desserts',    1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000010', N'Fresh Orange Juice',  5.99, N'Beverages',   1, N'Assets/menu_placeholder.jpg'),
    (@m_esp,                                 N'Espresso',            3.99, N'Beverages',   1, N'Assets/menu_placeholder.jpg'),
    (@m_capp,                                N'Cappuccino',          4.99, N'Beverages',   1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000013', N'Mineral Water',       2.99, N'Beverages',   1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000014', N'Club Sandwich',      14.99, N'Snacks',      1, N'Assets/menu_placeholder.jpg'),
    ('66666666-6666-6666-6666-000000000015', N'French Fries',        6.99, N'Snacks',      1, N'Assets/menu_placeholder.jpg');

-- ---------------------------------------------------------------------
-- reservations
-- ---------------------------------------------------------------------
DECLARE @r_charlie UNIQUEIDENTIFIER = '77777777-7777-7777-7777-000000000001';
DECLARE @r_jane    UNIQUEIDENTIFIER = '77777777-7777-7777-7777-000000000002';

-- Charlie Brown -- solo, future reservation in Room 202
INSERT INTO dbo.reservations
    (reservation_id, guest_id, room_id, check_in_date, check_out_date, status)
VALUES
    (@r_charlie, @g_charlie,
     '44444444-4444-4444-4444-000000000202',
     DATEADD(DAY, 2, CAST(SYSDATETIME() AS DATE)),
     DATEADD(DAY, 5, CAST(SYSDATETIME() AS DATE)),
     'Confirmed');

-- Jane Doe + accompanying child Lily, today -> +2 days in Room 101
INSERT INTO dbo.reservations
    (reservation_id, guest_id, room_id, check_in_date, check_out_date, status)
VALUES
    (@r_jane, @g_jane,
     '44444444-4444-4444-4444-000000000101',
     CAST(SYSDATETIME() AS DATE),
     DATEADD(DAY, 2, CAST(SYSDATETIME() AS DATE)),
     'Confirmed');

INSERT INTO dbo.reservation_accompanying
    (accompanying_id, reservation_id, name, gender, age, passport)
VALUES
    ('88888888-8888-8888-8888-000000000001',
     @r_jane, N'Lily Doe', 'Female', 9, 'P10000201');

-- ---------------------------------------------------------------------
-- stays
-- ---------------------------------------------------------------------
DECLARE @stay_active UNIQUEIDENTIFIER = '99999999-9999-9999-9999-000000000001';
DECLARE @stay_past   UNIQUEIDENTIFIER = '99999999-9999-9999-9999-000000000002';

-- John Smith - active stay in Room 201 (-2 days .. +1 day)
INSERT INTO dbo.stays
    (stay_id, guest_id, room_id, check_in_date, expected_check_out, room_charges, status)
VALUES
    (@stay_active, @g_john,
     '44444444-4444-4444-4444-000000000201',
     DATEADD(DAY, -2, SYSDATETIME()),
     DATEADD(DAY,  1, SYSDATETIME()),
     299.98, 'Active');

-- Diana Prince - past completed stay in Room 302 (mirrors INV-1001 in SRS App.B)
INSERT INTO dbo.stays
    (stay_id, guest_id, room_id, check_in_date, expected_check_out, actual_check_out, room_charges, status)
VALUES
    (@stay_past, @g_diana,
     '44444444-4444-4444-4444-000000000302',
     DATEADD(DAY, -7, SYSDATETIME()),
     DATEADD(DAY, -4, SYSDATETIME()),
     DATEADD(DAY, -4, SYSDATETIME()),
     749.97, 'CheckedOut');

-- ---------------------------------------------------------------------
-- restaurant_orders + order_lines
-- ---------------------------------------------------------------------
DECLARE @order_active UNIQUEIDENTIFIER = 'AAAAAAAA-AAAA-AAAA-AAAA-000000000001';

INSERT INTO dbo.restaurant_orders (order_id, stay_id, status, created_at)
VALUES (@order_active, @stay_active, 'Served', SYSDATETIME());

INSERT INTO dbo.order_lines (line_id, order_id, menu_item_id, quantity) VALUES
    ('BBBBBBBB-BBBB-BBBB-BBBB-000000000001', @order_active, @m_salmon, 1),
    ('BBBBBBBB-BBBB-BBBB-BBBB-000000000002', @order_active, @m_esp,    2);

-- ---------------------------------------------------------------------
-- invoices + invoice_lines  (paid invoice for Diana's past stay)
-- ---------------------------------------------------------------------
DECLARE @inv_1001 UNIQUEIDENTIFIER = 'CCCCCCCC-CCCC-CCCC-CCCC-000000001001';

INSERT INTO dbo.invoices
    (invoice_id, invoice_number, stay_id, guest_id, room_id,
     invoice_date, payment_status, payment_method, payment_date)
VALUES
    (@inv_1001, 'INV-1001',
     @stay_past, @g_diana,
     '44444444-4444-4444-4444-000000000302',
     DATEADD(DAY, -4, SYSDATETIME()),
     'Paid', 'CreditCard', DATEADD(DAY, -4, SYSDATETIME()));

INSERT INTO dbo.invoice_lines
    (line_id, invoice_id, description, quantity, unit_price, category)
VALUES
    ('DDDDDDDD-DDDD-DDDD-DDDD-000000001001', @inv_1001, N'Room 302 - Night 1', 1, 249.99, 'RoomCharge'),
    ('DDDDDDDD-DDDD-DDDD-DDDD-000000001002', @inv_1001, N'Room 302 - Night 2', 1, 249.99, 'RoomCharge'),
    ('DDDDDDDD-DDDD-DDDD-DDDD-000000001003', @inv_1001, N'Room 302 - Night 3', 1, 249.99, 'RoomCharge'),
    ('DDDDDDDD-DDDD-DDDD-DDDD-000000001004', @inv_1001, N'Beef Steak',         1,  32.99, 'RestaurantCharge'),
    ('DDDDDDDD-DDDD-DDDD-DDDD-000000001005', @inv_1001, N'Cappuccino',         2,   4.99, 'RestaurantCharge');

-- Advance the invoice-number sequence past the seeded INV-1001 so the
-- next generated invoice starts at INV-1002.
ALTER SEQUENCE dbo.seq_invoice_number RESTART WITH 1002;

COMMIT;
GO

-- =====================================================================
-- Sanity check
-- =====================================================================
SELECT 'rooms'                    AS table_name, COUNT(*) AS rows FROM dbo.rooms
UNION ALL SELECT 'guests',                COUNT(*) FROM dbo.guests
UNION ALL SELECT 'menu_items',            COUNT(*) FROM dbo.menu_items
UNION ALL SELECT 'reservations',          COUNT(*) FROM dbo.reservations
UNION ALL SELECT 'reservation_accomp',    COUNT(*) FROM dbo.reservation_accompanying
UNION ALL SELECT 'stays',                 COUNT(*) FROM dbo.stays
UNION ALL SELECT 'restaurant_orders',     COUNT(*) FROM dbo.restaurant_orders
UNION ALL SELECT 'order_lines',           COUNT(*) FROM dbo.order_lines
UNION ALL SELECT 'invoices',              COUNT(*) FROM dbo.invoices
UNION ALL SELECT 'invoice_lines',         COUNT(*) FROM dbo.invoice_lines
UNION ALL SELECT 'roles',                 COUNT(*) FROM dbo.roles
UNION ALL SELECT 'role_permissions',      COUNT(*) FROM dbo.role_permissions
UNION ALL SELECT 'users',                 COUNT(*) FROM dbo.users
ORDER  BY table_name;
GO
