  USE HotelManagement;

  UPDATE dbo.users SET password_hash = N'superadmin123' WHERE username = N'superadmin';
  UPDATE dbo.users SET password_hash = N'staff123'      WHERE username = N'staff';