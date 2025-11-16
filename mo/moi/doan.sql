CREATE DATABASE QL_BanGaRan;
GO
USE QL_BanGaRan;
GO

------------------------------------------------------
-- 1️⃣ BẢNG TÀI KHOẢN
------------------------------------------------------
CREATE TABLE TaiKhoan (
    MaTK INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap NVARCHAR(50) UNIQUE NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(20) CHECK (VaiTro IN ('Admin', 'KhachHang')) NOT NULL,
    TrangThai BIT DEFAULT 1
);

------------------------------------------------------
-- 2️⃣ BẢNG LOẠI SẢN PHẨM
------------------------------------------------------
CREATE TABLE LoaiSanPham (
    MaLoai INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(100)
);

------------------------------------------------------
-- 3️⃣ BẢNG SẢN PHẨM
------------------------------------------------------
CREATE TABLE SanPham (
    MaSP INT IDENTITY(1,1) PRIMARY KEY,
    TenSP NVARCHAR(100),
    Gia DECIMAL(10,2),
    MoTa NVARCHAR(255),
    SoLuong INT,
    Anh NVARCHAR(255),
    MaLoai INT FOREIGN KEY REFERENCES LoaiSanPham(MaLoai)
);

------------------------------------------------------
-- 4️⃣ BẢNG NHÂN VIÊN (ADMIN)
------------------------------------------------------
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100),
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    DienThoai NVARCHAR(20),
    Email NVARCHAR(100),
    MaTK INT FOREIGN KEY REFERENCES TaiKhoan(MaTK)
);

------------------------------------------------------
-- 5️⃣ BẢNG KHÁCH HÀNG
------------------------------------------------------
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100),
    DiaChi NVARCHAR(200),
    DienThoai NVARCHAR(20),
    Email NVARCHAR(100),
    MaTK INT FOREIGN KEY REFERENCES TaiKhoan(MaTK)
);

------------------------------------------------------
-- 6️⃣ BẢNG HÓA ĐƠN
------------------------------------------------------
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap DATE,
    TongTien DECIMAL(10,2),
    MaKH INT FOREIGN KEY REFERENCES KhachHang(MaKH),
    MaNV INT FOREIGN KEY REFERENCES NhanVien(MaNV)
);

------------------------------------------------------
-- 7️⃣ BẢNG CHI TIẾT HÓA ĐƠN
------------------------------------------------------
CREATE TABLE ChiTietHoaDon (
    MaCTHD INT IDENTITY(1,1) PRIMARY KEY,
    MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaSP INT FOREIGN KEY REFERENCES SanPham(MaSP),
    SoLuong INT,
    DonGia DECIMAL(10,2)
);


------------------------------------------------------
-- 🔟 DỮ LIỆU MẪU
------------------------------------------------------

-- Tài khoản admin
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro)
VALUES
('admin', 'admin123', 'Admin'),
('admin2', 'password', 'Admin');

-- Tài khoản khách hàng
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro)
VALUES
('an.nguyen', '123456', 'KhachHang'),
('le.hong', '123456', 'KhachHang'),
('tran.vy', '123456', 'KhachHang'),
('pham.nam', '123456', 'KhachHang'),
('do.ha', '123456', 'KhachHang');

-- Loại sản phẩm
INSERT INTO LoaiSanPham (TenLoai)
VALUES 
(N'Gà Rán'),
(N'Mỳ Ý'),
(N'Nước Uống'),
(N'Burger'),
(N'Combo Gia Đình'),
(N'Tráng Miệng');

-- Sản phẩm (5 sản phẩm mỗi loại)
INSERT INTO SanPham (TenSP, Gia, MoTa, SoLuong, Anh, MaLoai) VALUES
-- Gà Rán
(N'Gà Rán Giòn Cay', 45000, N'Gà rán giòn vị cay đặc trưng', 100, 'ga1.jpg', 1),
(N'Gà Rán Không Cay', 43000, N'Gà rán giòn không cay', 120, 'ga2.jpg', 1),
(N'Cánh Gà BBQ', 48000, N'Cánh gà sốt BBQ', 90, 'ga3.jpg', 1),
(N'Gà Popcorn', 40000, N'Gà viên nhỏ giòn', 150, 'ga4.jpg', 1),

-- Mỳ Ý
(N'Mỳ Ý Sốt Bò Bằm', 50000, N'Mỳ Ý với sốt bò bằm đậm vị', 80, 'myy1.jpg', 2),
(N'Mỳ Ý Gà Viên', 49000, N'Mỳ Ý kèm gà viên', 85, 'myy2.jpg', 2),

-- Nước Uống
(N'Mirinda', 18000, N'Nước giải khát có gas', 200, 'drink1.jpg', 3),
(N'Pepsi', 18000, N'Nước ngọt có gas', 200, 'drink2.jpg', 3),
(N'Seven Up', 18000, N'Nước chanh tươi mát', 180, 'drink3.jpg', 3),
(N'Trà Đào', 22000, N'Trà đào ngọt nhẹ', 150, 'drink4.jpg', 3),
(N'Nước suối', 15000, N'Nước tinh khiết từ thiên nhiên', 100, 'drink5.jpg', 3),

-- Burger
(N'Burger Gà Giòn Cay', 52000, N'Burger nhân gà cay', 80, 'burger1.jpg', 4),
(N'Burger Bò Phô Mai', 55000, N'Burger bò phô mai', 90, 'burger2.jpg', 4),
(N'Hot Dog', 50000, N'Hot dog', 100, 'burger3.jpg', 4),
(N'Burger Tôm', 56000, N'Burger nhân tôm', 70, 'burger4.jpg', 4),


-- Combo Gia Đình
(N'Combo Hay ho ', 189000, N'Dành cho 3-4 người', 50, 'Combo1.jpg', 5),
(N'Combo Gia đình ', 249000, N'Cho 5-6 người', 40, 'Combo1.jpg', 5),
(N'Combo Party ', 299000, N'Cho nhóm bạn', 35, 'Combo1.jpg', 5),

(N'Kem Dâu', 19000, N'Kem vị dâu ngọt mát', 100, 'trangmieng1.jpg', 6),
(N'Kem Chocolate', 20000, N'Kem vị chocolate béo ngậy', 100, 'trangmieng2.jpg', 6);


-- Nhân viên (liên kết tài khoản admin)
INSERT INTO NhanVien (HoTen, NgaySinh, GioiTinh, DienThoai, Email, MaTK)
VALUES 
(N'Nguyễn Văn Nam', '1990-04-12', N'Nam', '0909000111', 'namnv@jollibee.vn', 1),
(N'Trần Thị Hoa', '1995-08-25', N'Nữ', '0909111222', 'hoatt@jollibee.vn', 2);

-- Khách hàng (liên kết tài khoản khách)
INSERT INTO KhachHang (HoTen, DiaChi, DienThoai, Email, MaTK)
VALUES 
(N'Nguyễn An', N'Quận 1, TP.HCM', '0905111222', 'anan@gmail.com', 3),
(N'Lê Hồng', N'Quận 3, TP.HCM', '0905222333', 'lehong@gmail.com', 4),
(N'Trần Vy', N'Quận 5, TP.HCM', '0905333444', 'tranvy@gmail.com', 5),
(N'Phạm Nam', N'Thủ Đức, TP.HCM', '0905444555', 'phamnam@gmail.com', 6),
(N'Đỗ Hà', N'Bình Thạnh, TP.HCM', '0905555666', 'doha@gmail.com', 7);

INSERT INTO HoaDon (NgayLap, TongTien, MaKH, MaNV)
VALUES 
(GETDATE(), 0, 1, 1),
(GETDATE(), 0, 2, 2);

-- Chi tiết hóa đơn
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia)
VALUES
(1, 1, 2, 45000),
(1, 3, 1, 48000),
(2, 7, 3, 18000),
(2, 12, 2, 52000);

-----------------------------------------------------
-- ⚙️ HÀM (FUNCTION)
-----------------------------------------------------
-- Hàm tính tổng tiền hóa đơn
CREATE FUNCTION fn_TinhTongTien(@MaHD INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Tong DECIMAL(10,2);
    SELECT @Tong = SUM(SoLuong * DonGia)
    FROM ChiTietHoaDon
    WHERE MaHD = @MaHD;
    RETURN ISNULL(@Tong, 0);
END;
GO


-----------------------------------------------------
-- ⚙️ THỦ TỤC (STORED PROCEDURE)
-----------------------------------------------------

-- 1️⃣ Thêm sản phẩm mới
CREATE PROCEDURE sp_ThemSanPham
    @TenSP NVARCHAR(100),
    @Gia DECIMAL(10,2),
    @MoTa NVARCHAR(255),
    @SoLuong INT,
    @Anh NVARCHAR(255),
    @MaLoai INT
AS
BEGIN
    INSERT INTO SanPham (TenSP, Gia, MoTa, SoLuong, Anh, MaLoai)
    VALUES (@TenSP, @Gia, @MoTa, @SoLuong, @Anh, @MaLoai);
END;
GO


-- 2️⃣ Cập nhật thông tin sản phẩm
CREATE PROCEDURE sp_SuaSanPham
    @MaSP INT,
    @TenSP NVARCHAR(100),
    @Gia DECIMAL(10,2),
    @MoTa NVARCHAR(255),
    @SoLuong INT,
    @Anh NVARCHAR(255),
    @MaLoai INT
AS
BEGIN
    UPDATE SanPham
    SET TenSP = @TenSP,
        Gia = @Gia,
        MoTa = @MoTa,
        SoLuong = @SoLuong,
        Anh = @Anh,
        MaLoai = @MaLoai
    WHERE MaSP = @MaSP;
END;
GO


-- 3️⃣ Xóa sản phẩm
CREATE PROCEDURE sp_XoaSanPham
    @MaSP INT
AS
BEGIN
    DELETE FROM SanPham WHERE MaSP = @MaSP;
END;
GO


-- 4️⃣ Tạo hóa đơn mới
CREATE PROCEDURE sp_TaoHoaDon
    @MaKH INT,
    @MaNV INT
AS
BEGIN
    INSERT INTO HoaDon (NgayLap, TongTien, MaKH, MaNV)
    VALUES (GETDATE(), 0, @MaKH, @MaNV);
END;
GO


-- 5️⃣ Thêm chi tiết hóa đơn
CREATE PROCEDURE sp_ThemChiTietHoaDon
    @MaHD INT,
    @MaSP INT,
    @SoLuong INT,
    @DonGia DECIMAL(10,2)
AS
BEGIN
    INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia)
    VALUES (@MaHD, @MaSP, @SoLuong, @DonGia);

    -- Cập nhật tổng tiền trong hóa đơn
    DECLARE @TongTien DECIMAL(10,2);
    SELECT @TongTien = dbo.fn_TinhTongTien(@MaHD);
    UPDATE HoaDon SET TongTien = @TongTien WHERE MaHD = @MaHD;
END;
GO


-----------------------------------------------------
-- ⚙️ TRIGGER
-----------------------------------------------------

-- 1️⃣ Khi thêm chi tiết hóa đơn => Giảm số lượng sản phẩm trong kho
CREATE TRIGGER trg_GiamSoLuongSauKhiBan
ON ChiTietHoaDon
AFTER INSERT
AS
BEGIN
    UPDATE s
    SET s.SoLuong = s.SoLuong - i.SoLuong
    FROM SanPham s
    JOIN inserted i ON s.MaSP = i.MaSP;
END;
GO


-- 2️⃣ Khi thêm sản phẩm vào giỏ hàng => Kiểm tra số lượng tồn



-- 3️⃣ Trigger tự cập nhật tổng tiền hóa đơn khi thêm chi tiết
CREATE TRIGGER trg_CapNhatTongTienHoaDon
ON ChiTietHoaDon
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    DECLARE @MaHD INT;

    -- Lấy mã hóa đơn bị ảnh hưởng
    SELECT TOP 1 @MaHD = COALESCE(i.MaHD, d.MaHD)
    FROM inserted i
    FULL JOIN deleted d ON i.MaCTHD = d.MaCTHD;

    -- Cập nhật tổng tiền
    UPDATE HoaDon
    SET TongTien = dbo.fn_TinhTongTien(@MaHD)
    WHERE MaHD = @MaHD;
END;
GO
