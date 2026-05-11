-- =====================================================================
-- Hotel Management System - MySQL Schema (v1.1)
-- Target:  MySQL 8.0+ (utf8mb4 / InnoDB)
-- Source:  SRS.md  §3.5 Logical Data Model
--
-- v1.1 changes vs v1.0:
--   * rooms.floor             (new column)
--   * guests.passport          (new column)
--   * guests.gender            (new column)
--   * menu_items.image_path    (new column)
--   * reservations.marriage_certificate_path (new column)
--   * reservation_accompanying (new table — per-person party data)
-- =====================================================================

DROP DATABASE IF EXISTS hotel_management;

CREATE DATABASE hotel_management CHARACTER
SET
    utf8mb4 COLLATE utf8mb4_unicode_ci;

USE hotel_management;

-- ---------------------------------------------------------------------
-- USERS
-- ---------------------------------------------------------------------
CREATE TABLE users (
    id INT NOT NULL AUTO_INCREMENT,
    username VARCHAR(64) NOT NULL,
    password_hash VARCHAR(255) NOT NULL, -- BCrypt/Argon2id hash (NFR-SEC-1)
    role ENUM('Staff', 'Manager') NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_users_username (username)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- ROOMS  (room_number is the natural key)
-- ---------------------------------------------------------------------
CREATE TABLE rooms (
  number          INT             NOT NULL,
  floor           INT             NOT NULL DEFAULT 1,
  type            ENUM('Single','Double','Suite','Deluxe','Penthouse') NOT NULL,
  rate            DECIMAL(10,2)   NOT NULL,
  is_occupied     BOOLEAN         NOT NULL DEFAULT FALSE,
  `condition`     ENUM('Clean','NeedsCleaning','OutOfService') NOT NULL DEFAULT 'Clean',
  maintenance_log TEXT            NULL,
  PRIMARY KEY (number),
  CONSTRAINT chk_rooms_rate_nonneg  CHECK (rate >= 0),
  CONSTRAINT chk_rooms_floor_nonneg CHECK (floor >= 0)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- GUESTS
-- ---------------------------------------------------------------------
CREATE TABLE guests (
    id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(120) NOT NULL,
    contact VARCHAR(64) NOT NULL,
    passport VARCHAR(64) NOT NULL DEFAULT '',
    gender ENUM(
        'Unspecified',
        'Male',
        'Female'
    ) NOT NULL DEFAULT 'Unspecified',
    is_vip BOOLEAN NOT NULL DEFAULT FALSE,
    stay_count INT NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    CONSTRAINT chk_guests_stay_count_nonneg CHECK (stay_count >= 0)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- MENU ITEMS
-- ---------------------------------------------------------------------
CREATE TABLE menu_items (
    id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(120) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    category VARCHAR(64) NOT NULL,
    is_available BOOLEAN NOT NULL DEFAULT TRUE,
    description TEXT NULL,
    image_path VARCHAR(500) NULL,
    PRIMARY KEY (id),
    CONSTRAINT chk_menu_items_price_nonneg CHECK (price >= 0)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- RESERVATIONS
-- ---------------------------------------------------------------------
CREATE TABLE reservations (
    id INT NOT NULL AUTO_INCREMENT,
    guest_id INT NOT NULL,
    room_number INT NOT NULL,
    check_in_date DATE NOT NULL,
    check_out_date DATE NOT NULL,
    status ENUM(
        'Pending',
        'Confirmed',
        'CheckedIn',
        'Cancelled',
        'Completed'
    ) NOT NULL DEFAULT 'Confirmed',
    marriage_certificate_path VARCHAR(500) NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_res_guest FOREIGN KEY (guest_id) REFERENCES guests (id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_res_room FOREIGN KEY (room_number) REFERENCES rooms (number) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_res_dates CHECK (
        check_out_date > check_in_date
    ),
    KEY ix_res_guest (guest_id),
    KEY ix_res_room (room_number),
    KEY ix_res_status (status)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- RESERVATION ACCOMPANYING GUESTS  (one row per accompanying person)
-- ---------------------------------------------------------------------
CREATE TABLE reservation_accompanying (
    id INT NOT NULL AUTO_INCREMENT,
    reservation_id INT NOT NULL,
    name VARCHAR(120) NOT NULL,
    gender ENUM(
        'Unspecified',
        'Male',
        'Female'
    ) NOT NULL DEFAULT 'Unspecified',
    age INT NOT NULL,
    passport VARCHAR(64) NOT NULL DEFAULT '',
    PRIMARY KEY (id),
    CONSTRAINT fk_acc_reservation FOREIGN KEY (reservation_id) REFERENCES reservations (id) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT chk_acc_age CHECK (
        age > 0
        AND age <= 120
    ),
    KEY ix_acc_reservation (reservation_id)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- STAYS
-- ---------------------------------------------------------------------
CREATE TABLE stays (
    id INT NOT NULL AUTO_INCREMENT,
    guest_id INT NOT NULL,
    room_number INT NOT NULL,
    reservation_id INT NULL, -- DC-7: at most one source reservation
    check_in_date DATETIME NOT NULL,
    expected_check_out DATETIME NOT NULL,
    actual_check_out DATETIME NULL,
    room_charges DECIMAL(10, 2) NOT NULL DEFAULT 0,
    restaurant_charges DECIMAL(10, 2) NOT NULL DEFAULT 0,
    status ENUM('Active', 'CheckedOut') NOT NULL DEFAULT 'Active',
    PRIMARY KEY (id),
    CONSTRAINT fk_stay_guest FOREIGN KEY (guest_id) REFERENCES guests (id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_stay_room FOREIGN KEY (room_number) REFERENCES rooms (number) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_stay_res FOREIGN KEY (reservation_id) REFERENCES reservations (id) ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT chk_stay_actual_after_in CHECK (
        actual_check_out IS NULL
        OR actual_check_out >= check_in_date
    ),
    CONSTRAINT chk_stay_charges_nonneg CHECK (
        room_charges >= 0
        AND restaurant_charges >= 0
    ),
    KEY ix_stay_guest (guest_id),
    KEY ix_stay_room (room_number),
    KEY ix_stay_status (status),
    KEY ix_stay_res (reservation_id)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- RESTAURANT ORDERS
-- ---------------------------------------------------------------------
CREATE TABLE restaurant_orders (
    id INT NOT NULL AUTO_INCREMENT,
    stay_id INT NOT NULL,
    status ENUM(
        'Placed',
        'Preparing',
        'Ready',
        'Served',
        'Cancelled'
    ) NOT NULL DEFAULT 'Placed',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_orders_stay FOREIGN KEY (stay_id) REFERENCES stays (id) ON UPDATE CASCADE ON DELETE CASCADE,
    KEY ix_orders_stay (stay_id),
    KEY ix_orders_status (status),
    KEY ix_orders_date (created_at)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- ORDER LINES
-- ---------------------------------------------------------------------
CREATE TABLE order_lines (
    id INT NOT NULL AUTO_INCREMENT,
    order_id INT NOT NULL,
    menu_item_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    notes VARCHAR(255) NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_olines_order FOREIGN KEY (order_id) REFERENCES restaurant_orders (id) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_olines_item FOREIGN KEY (menu_item_id) REFERENCES menu_items (id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_olines_qty CHECK (quantity >= 1),
    KEY ix_olines_order (order_id),
    KEY ix_olines_item (menu_item_id)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- INVOICES  (invoice_number is the natural key, e.g. 'INV-1001')
-- ---------------------------------------------------------------------
CREATE TABLE invoices (
    invoice_number VARCHAR(20) NOT NULL,
    stay_id INT NOT NULL,
    guest_id INT NOT NULL,
    room_number INT NOT NULL,
    invoice_date DATETIME NOT NULL,
    payment_status ENUM('Pending', 'Paid', 'Refunded') NOT NULL DEFAULT 'Pending',
    payment_method ENUM(
        'Cash',
        'CreditCard',
        'DebitCard',
        'BankTransfer'
    ) NULL,
    payment_date DATETIME NULL,
    PRIMARY KEY (invoice_number),
    CONSTRAINT fk_inv_stay FOREIGN KEY (stay_id) REFERENCES stays (id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_inv_guest FOREIGN KEY (guest_id) REFERENCES guests (id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_inv_room FOREIGN KEY (room_number) REFERENCES rooms (number) ON UPDATE CASCADE ON DELETE RESTRICT,
    -- DC-6: payment_method is set iff payment_status in (Paid, Refunded)
    CONSTRAINT chk_inv_payment_consistency CHECK (
        (
            payment_status = 'Pending'
            AND payment_method IS NULL
            AND payment_date IS NULL
        )
        OR (
            payment_status IN ('Paid', 'Refunded')
            AND payment_method IS NOT NULL
        )
    ),
    KEY ix_inv_stay (stay_id),
    KEY ix_inv_guest (guest_id),
    KEY ix_inv_status (payment_status),
    KEY ix_inv_date (invoice_date)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
-- INVOICE LINES
-- ---------------------------------------------------------------------
CREATE TABLE invoice_lines (
    id INT NOT NULL AUTO_INCREMENT,
    invoice_id VARCHAR(20) NOT NULL,
    description VARCHAR(255) NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    unit_price DECIMAL(10, 2) NOT NULL,
    category ENUM(
        'RoomCharge',
        'RestaurantCharge'
    ) NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_ilines_inv FOREIGN KEY (invoice_id) REFERENCES invoices (invoice_number) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT chk_ilines_qty CHECK (quantity >= 1),
    CONSTRAINT chk_ilines_price CHECK (unit_price >= 0),
    KEY ix_ilines_inv (invoice_id)
) ENGINE = InnoDB;

-- =====================================================================
-- SEED DATA  (mirrors Data/SeedData.cs from the WinForms app)
-- =====================================================================

INSERT INTO
    users (username, password_hash, role)
VALUES (
        'admin',
        '$2a$replace_with_bcrypt_hash_admin',
        'Manager'
    ),
    (
        'staff',
        '$2a$replace_with_bcrypt_hash_staff',
        'Staff'
    );

INSERT INTO rooms (number, floor, type, rate, is_occupied, `condition`, maintenance_log) VALUES
  (101, 1, 'Single',    99.99,  FALSE, 'Clean',          NULL),
  (102, 1, 'Single',    99.99,  FALSE, 'Clean',          NULL),
  (201, 2, 'Double',   149.99,  TRUE,  'Clean',          NULL),
  (202, 2, 'Double',   149.99,  FALSE, 'Clean',          NULL),
  (301, 3, 'Suite',    249.99,  TRUE,  'Clean',          NULL),
  (302, 3, 'Suite',    249.99,  FALSE, 'NeedsCleaning',  NULL),
  (401, 4, 'Deluxe',   349.99,  FALSE, 'Clean',          NULL),
  (402, 4, 'Deluxe',   349.99,  FALSE, 'Clean',          NULL),
  (501, 5, 'Penthouse',599.99,  FALSE, 'OutOfService',   'Plumbing repair scheduled'),
  (502, 5, 'Penthouse',599.99,  FALSE, 'Clean',          NULL);

INSERT INTO
    guests (
        name,
        contact,
        passport,
        gender,
        is_vip,
        stay_count
    )
VALUES (
        'John Smith',
        '555-0101',
        'P10000001',
        'Male',
        TRUE,
        5
    ),
    (
        'Jane Doe',
        '555-0102',
        'P10000002',
        'Female',
        FALSE,
        2
    ),
    (
        'Bob Johnson',
        '555-0103',
        'P10000003',
        'Male',
        FALSE,
        1
    ),
    (
        'Alice Williams',
        '555-0104',
        'P10000004',
        'Female',
        TRUE,
        8
    ),
    (
        'Charlie Brown',
        '555-0105',
        'P10000005',
        'Male',
        FALSE,
        0
    ),
    (
        'Diana Prince',
        '555-0106',
        'P10000006',
        'Female',
        FALSE,
        1
    );

-- All menu items point to the shared placeholder until a manager uploads one.
INSERT INTO
    menu_items (
        name,
        price,
        category,
        is_available,
        image_path
    )
VALUES (
        'Caesar Salad',
        12.99,
        'Starters',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Tomato Soup',
        8.99,
        'Starters',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Grilled Salmon',
        24.99,
        'Main Course',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Beef Steak',
        32.99,
        'Main Course',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Chicken Pasta',
        18.99,
        'Main Course',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Margherita Pizza',
        15.99,
        'Main Course',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Chocolate Cake',
        9.99,
        'Desserts',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Ice Cream Sundae',
        7.99,
        'Desserts',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Tiramisu',
        10.99,
        'Desserts',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Fresh Orange Juice',
        5.99,
        'Beverages',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Espresso',
        3.99,
        'Beverages',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Cappuccino',
        4.99,
        'Beverages',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Mineral Water',
        2.99,
        'Beverages',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'Club Sandwich',
        14.99,
        'Snacks',
        TRUE,
        'Assets/menu_placeholder.jpg'
    ),
    (
        'French Fries',
        6.99,
        'Snacks',
        TRUE,
        'Assets/menu_placeholder.jpg'
    );

-- A confirmed future reservation (Charlie Brown — solo, no accompanying party)
INSERT INTO
    reservations (
        guest_id,
        room_number,
        check_in_date,
        check_out_date,
        status
    )
VALUES (
        5,
        202,
        DATE_ADD (CURDATE (), INTERVAL 2 DAY),
        DATE_ADD (CURDATE (), INTERVAL 5 DAY),
        'Confirmed'
    );

-- A reservation with one accompanying child (Jane Doe + daughter Lily) — capacity 2 fits 1A+1C.
INSERT INTO
    reservations (
        guest_id,
        room_number,
        check_in_date,
        check_out_date,
        status
    )
VALUES (
        2,
        101,
        CURDATE (),
        DATE_ADD (CURDATE (), INTERVAL 2 DAY),
        'Confirmed'
    );

SET @ res_jane = LAST_INSERT_ID ();

INSERT INTO
    reservation_accompanying (
        reservation_id,
        name,
        gender,
        age,
        passport
    )
VALUES (
        @ res_jane,
        'Lily Doe',
        'Female',
        9,
        'P10000201'
    );

-- Active stay used by sample order + invoice below
INSERT INTO
    stays (
        guest_id,
        room_number,
        check_in_date,
        expected_check_out,
        room_charges,
        status
    )
VALUES (
        1,
        201,
        DATE_ADD (NOW(), INTERVAL -2 DAY),
        DATE_ADD (NOW(), INTERVAL 1 DAY),
        299.98,
        'Active'
    );

INSERT INTO
    restaurant_orders (stay_id, status, created_at)
VALUES (1, 'Served', NOW());

INSERT INTO
    order_lines (
        order_id,
        menu_item_id,
        quantity
    )
VALUES (1, 3, 1), -- Grilled Salmon
    (1, 11, 2);
-- Espresso

-- Past completed stay + paid invoice (mirrors INV-1001 in SRS appendix B)
INSERT INTO
    stays (
        guest_id,
        room_number,
        check_in_date,
        expected_check_out,
        actual_check_out,
        room_charges,
        status
    )
VALUES (
        6,
        302,
        DATE_ADD (NOW(), INTERVAL -7 DAY),
        DATE_ADD (NOW(), INTERVAL -4 DAY),
        DATE_ADD (NOW(), INTERVAL -4 DAY),
        749.97,
        'CheckedOut'
    );

INSERT INTO
    invoices (
        invoice_number,
        stay_id,
        guest_id,
        room_number,
        invoice_date,
        payment_status,
        payment_method,
        payment_date
    )
VALUES (
        'INV-1001',
        2,
        6,
        302,
        DATE_ADD (NOW(), INTERVAL -4 DAY),
        'Paid',
        'CreditCard',
        DATE_ADD (NOW(), INTERVAL -4 DAY)
    );

INSERT INTO
    invoice_lines (
        invoice_id,
        description,
        quantity,
        unit_price,
        category
    )
VALUES (
        'INV-1001',
        'Room 302 - Night 1',
        1,
        249.99,
        'RoomCharge'
    ),
    (
        'INV-1001',
        'Room 302 - Night 2',
        1,
        249.99,
        'RoomCharge'
    ),
    (
        'INV-1001',
        'Room 302 - Night 3',
        1,
        249.99,
        'RoomCharge'
    ),
    (
        'INV-1001',
        'Beef Steak',
        1,
        32.99,
        'RestaurantCharge'
    ),
    (
        'INV-1001',
        'Cappuccino',
        2,
        4.99,
        'RestaurantCharge'
    );