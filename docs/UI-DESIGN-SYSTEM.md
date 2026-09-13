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
- Header 70 px; metric cards 110 px; action bar 64 px; settings 280 px; footer 32 px.

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

- Header cao 46 px; hàng dữ liệu cao 60 px (đơn vị thiết kế tại DPI 100%).
- Header nền #F8FAFC, chữ uppercase 8.5 pt.
- Dữ liệu Segoe UI 9.5 pt.
- Chỉ dùng đường kẻ ngang.
- Tuyệt đối không vẽ viền dọc giữa các cell.
- Không hiển thị focus rectangle quanh cell hiện tại; selection áp dụng cho toàn hàng.
- Timer không được gán lại giá trị cell khi dữ liệu không thay đổi để tránh nhấp nháy.
- Row được chọn dùng nền blue soft.
- Trạng thái lấy từ cầu nối JVM: Menu, Tài khoản, Chọn nhân vật, Màn hình game, Có hộp thoại. Màn hình game dùng nhãn xanh nhạt; các trạng thái còn lại dùng xám. Chờ cầu nối/mất cầu nối phải hiển thị riêng. Không diễn giải thành Online trên server.

### Checkbox

- Luôn vẽ custom, không dùng glyph checkbox mặc định của WinForms.
- Kích thước 18 × 18 px, bo góc 5 px.
- Căn chính giữa theo cả chiều ngang và dọc của cell.
- Chưa chọn: nền trắng, viền #CBD5E1 dày 1.4 px.
- Đã chọn: nền primary blue, dấu tick trắng dày 2 px.
- Click một lần phải cập nhật dữ liệu ngay.

### Settings và footer

- Settings nằm trong card trắng bo góc 14 px.
- Thanh tab chính cao 42 px, mỗi mục rộng 150 px. Vùng cấu hình không được thấp hơn 280 px ở kích thước cửa sổ mặc định.
- `Cấu hình Auto` chứa tab chức năng cấp một. Tab đầu tiên là `Đánh quái (Train)`.
- `Đánh quái (Train)` chứa đúng thứ tự các tab con: `Cài đặt cơ bản`, `Nâng cao`, `Gán skill`, `Kiểu đánh quái`, `Kích yên`.
- Tab cấp chức năng và tab con cao 38 px; nền trắng, tab đang chọn dùng blue soft, chữ primary blue và gạch chân xanh 3 px.
- Nhãn tab phải căn giữa, dùng Segoe UI Semibold 8.5–8.75 pt và cắt bằng dấu ba chấm khi thiếu chiều rộng.
- Dùng `ModernTabs` tự dựng bằng button và content panel; không dùng phần thân hoặc viền nổi của TabControl mặc định.
- Ba cấp điều hướng phải để lại vùng nội dung sử dụng được. Nội dung nằm trong panel nền ứng dụng, viền 1 px và bo góc 12 px; không được cắt chữ ở DPI 100–150%.
- Footer chỉ chứa trạng thái gần nhất bên trái và phiên bản bên phải.

### Cài đặt cơ bản / map đánh quái

- Dùng một hàng điều khiển cố định: checkbox hiện đại `Đánh quái:` rộng 116 px, combobox map rộng 270 px, nút chính `GET` rộng 72 px và chú thích trạng thái ở cuối.
- Tên map hiển thị theo mẫu `ID.Tên map`, ví dụ `1.Trường Hirosaki`.
- Không dùng checkbox mặc định. Ô chọn bo 5 px, 18×18 px, viền xám khi tắt và nền xanh/tick trắng khi bật; chữ căn giữa theo chiều dọc.
- `GET` đọc vị trí của hồ sơ đang chọn. Nếu client chưa vào màn hình game, chỉ báo ở footer và không thay đổi lựa chọn hiện tại.
- Map lấy từ client nhưng chưa có trong danh mục phải được thêm động, không ép về map mặc định.
- Checkbox và map lưu riêng theo hồ sơ; đổi dòng trong bảng phải nạp đúng cấu hình của dòng đó.

### Hộp thoại hồ sơ

- Thêm và sửa tài khoản phải thực hiện trong ProfileDialog.
- Mật khẩu luôn dùng ký tự che; không hiển thị trong bảng client.
- Tùy chọn Tự đăng nhập và Tự khởi động lại đặt ngay dưới nhóm thông tin.
- Nút Lưu hồ sơ dùng primary blue; nút Hủy dùng secondary.
- Mật khẩu chỉ được lưu sau khi mã hóa bằng Windows DPAPI cho tài khoản Windows hiện tại.

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
- [ ] Trạng thái cầu nối đúng với màn hình; không suy ra Online từ tiến trình Java.
- [ ] Nút primary/danger đúng vai trò.
- [ ] Bảng không nhấp nháy khi cập nhật trạng thái.
- [ ] GitHub Actions build thành công.

## Bảng tài khoản — bản tinh chỉnh

- Vẽ thống nhất toàn bộ cell, kể cả header; không gọi trình vẽ viền mặc định.
- Cột co giãn theo tỷ trọng, có chiều rộng tối thiểu; cho phép cuộn ngang khi không đủ chỗ.
- Nhãn cột ngắn: Đăng nhập, Chạy lại; checkbox căn giữa và scale theo DPI.
- Cột Chạy lại tạm chỉ đọc, hiển thị mờ cho đến khi có logic thực thi.
- Nhãn trạng thái cao 26 px, bo tròn 13 px; nội dung hàng có padding ngang 12 px.
- Kiểm tra trực quan trên Windows vẫn cần thiết; build thành công không thay thế kiểm tra hiển thị.


## Cầu nối client v1

- Cột trạng thái tối thiểu 155 px, nhãn dùng phần rộng còn lại sau padding; tooltip có lớp màn hình và danh sách nhân vật.
- Tự đăng nhập bật/tắt được trong bảng và hộp thoại hồ sơ; tooltip trạng thái hiển thị tiến độ gửi login/chọn nhân vật.
- Chi tiết giao thức, ánh xạ và giới hạn ở [CLIENT-BRIDGE.md](CLIENT-BRIDGE.md).
