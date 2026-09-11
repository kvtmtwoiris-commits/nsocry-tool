# NSOCry Pro UI Design System

Đây là chuẩn giao diện bắt buộc của NSOCry Pro. Mọi thay đổi GUI phải đối chiếu tài liệu này để giữ sản phẩm đồng bộ.

## 1. Định hướng

- Phong cách dashboard Windows hiện đại, sáng, sạch và tập trung vào dữ liệu.
- Ưu tiên khả năng đọc trên Windows VPS và thao tác nhanh với nhiều client.
- Không sử dụng giao diện WinForms mặc định nếu control đó phá vỡ thiết kế chung.
- Không dùng emoji làm icon vì kết quả khác nhau giữa các phiên bản Windows.
- Font duy nhất: Segoe UI.

## 2. Bảng màu

| Vai trò | RGB | Hex |
|---|---:|---:|
| Nền ứng dụng | 244, 247, 251 | #F4F7FB |
| Nền thẻ | 255, 255, 255 | #FFFFFF |
| Tiêu đề navy | 17, 32, 61 | #11203D |
| Primary blue | 37, 99, 235 | #2563EB |
| Blue soft | 239, 246, 255 | #EFF6FF |
| Success green | 5, 150, 105 | #059669 |
| Success soft | 236, 253, 245 | #ECFDF5 |
| Error red | 225, 29, 72 | #E11D48 |
| Error soft | 255, 241, 242 | #FFF1F2 |
| Warning amber | 217, 119, 6 | #D97706 |
| Text chính | 15, 23, 42 | #0F172A |
| Text phụ | 100, 116, 139 | #64748B |
| Đường viền | 226, 232, 240 | #E2E8F0 |

## 3. Bố cục

- Kích thước mặc định: 1320 × 820 px.
- Kích thước tối thiểu: 1120 × 700 px.
- Lề ngoài: trái/phải 24 px, trên 20 px, dưới 16 px.
- Thứ tự: header → metric cards → action bar → client table → settings → footer.
- Header 70 px; metric cards 110 px; action bar 64 px; settings 166 px; footer 32 px.

## 4. Thành phần

### Header

- Logo 46 × 46 px, bo góc 13 px, nền primary blue.
- Tên sản phẩm: Segoe UI Semibold 21 pt, màu navy.
- Mô tả: Segoe UI 9.5 pt, màu text phụ.
- Badge server cao 38 px, bo tròn 19 px, dùng màu success.

### Metric card

- Nền trắng, viền 1 px, bo góc 14 px.
- Khoảng cách giữa card 14 px.
- Giá trị Segoe UI Semibold 18 pt.
- Mỗi card giữ đúng màu ngữ nghĩa đã định nghĩa.

### Nút thao tác

- Cao 34 px, Segoe UI Semibold 9 pt.
- Padding ngang 13 px; khoảng cách 8 px.
- Primary nền xanh, chữ trắng và chỉ dùng cho Mở game.
- Secondary nền trắng, viền xám.
- Danger nền đỏ nhạt, chữ đỏ và chỉ dùng cho hành động xóa.

### Bảng client

- Header và row cao 44 px.
- Header nền #F8FAFC, chữ uppercase 8.5 pt.
- Dữ liệu Segoe UI 9.5 pt.
- Chỉ dùng đường kẻ ngang.
- Row được chọn dùng nền blue soft.
- RUNNING màu success; OFFLINE màu error.

### Checkbox

- Luôn vẽ custom, không dùng glyph checkbox mặc định của WinForms.
- Kích thước 18 × 18 px, bo góc 5 px.
- Căn chính giữa theo cả chiều ngang và dọc của cell.
- Chưa chọn: nền trắng, viền #CBD5E1 dày 1.4 px.
- Đã chọn: nền primary blue, dấu tick trắng dày 2 px.
- Click một lần phải cập nhật dữ liệu ngay.

### Settings và footer

- Settings nằm trong card trắng bo góc 14 px.
- Tab cao 34 px, rộng 150 px.
- Footer chỉ chứa trạng thái gần nhất bên trái và phiên bản bên phải.

## 5. Quy tắc đồng bộ

- Mọi màu phải lấy từ palette, không thêm màu gần giống.
- Khoảng cách dùng bội số 4 px; ưu tiên 8, 12, 16, 20 và 24.
- Card chính bo góc 14 px; toolbar 12 px; control nhỏ 5–13 px.
- Không trộn dark theme và light theme.
- Không thay Segoe UI bằng font khác.
- Không để checkbox, button hoặc selection quay lại kiểu hệ thống mặc định.
- Mọi thay đổi phải build thành công trên GitHub Actions trước khi bàn giao.

## 6. Checklist phát hành

- [ ] Hiển thị tốt ở 100%, 125% và 150% DPI.
- [ ] Không có control bị cắt ở kích thước tối thiểu.
- [ ] Checkbox 18 px và nằm đúng giữa cell.
- [ ] Màu RUNNING/OFFLINE đúng chuẩn.
- [ ] Nút primary/danger đúng vai trò.
- [ ] Bảng không nhấp nháy khi cập nhật trạng thái.
- [ ] GitHub Actions build thành công.
