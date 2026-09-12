# NSOCry Pro — cầu nối JVM, phiên bản 1

## Phạm vi hiện tại

Đã có kênh TCP hai chiều trên `127.0.0.1`: tool gửi `POLL`, agent trong JVM trả trạng thái thật của đối tượng màn hình. Không dùng OCR, tọa độ hoặc SendKeys. Đây là adapter đọc trạng thái; chưa có lệnh tự đăng nhập/chọn nhân vật. Tùy chọn tự đăng nhập được giữ trong hồ sơ nhưng tạm vô hiệu trên GUI. Thông tin đăng nhập đã lưu không bị xóa.

Không coi tiến trình Java còn chạy, socket còn mở hay màn hình game còn hiển thị là xác nhận nhân vật đang online trên server. Bản này báo **Màn hình game**, không báo **Online**. Chưa có heartbeat ở giao thức game hay xác nhận map/nhân vật từ server.

## Client hỗ trợ

- File tham chiếu: `V9_NsoCry_x1 (1).jar` do người dùng cung cấp.
- SHA-256: `a6dc5c4a6f5314ddd9d8c8f0e03a9077dc3668775533e658562edeb9abe4a3ae`.
- Client khác vẫn mở bình thường nhưng cầu nối báo **Client chưa hỗ trợ** và không đọc các field đã ánh xạ.
- MicroEmulator: bản 2.0.4 đã cung cấp; Java 8 trở lên. Giữ thiết bị Resizable và vùng game 500×300; mỗi hồ sơ có `user.home`/RMS riêng.

## Các điểm đã xác minh bằng bytecode

| Điểm | Ý nghĩa / giới hạn |
|---|---|
| `ca.a:ag` | Canvas chuyển thao tác vẽ và nhập liệu tới lớp điều khiển game. |
| `aY.a:dr` | Đối tượng màn hình được truyền vào `dr.a(dp)` trong phương thức vẽ. |
| `aY.a:aq` | Hộp thoại phủ lên màn hình; báo riêng để không nhầm màn hình nền sẵn sàng thao tác. |
| `cI` | Màn hình menu có dữ liệu tài khoản và thao tác chuyển màn hình. |
| `bJ` | Màn hình quản lý/nhập thông tin tài khoản. Không đồng nhất mọi trạng thái của lớp này với ô login trong ảnh. |
| `cH.F:String[]` | Danh sách tên nhân vật; thao tác chọn lấy `F[q]` gửi qua `cK.P(String)`. |
| `ba` | Màn hình game; chưa đủ chứng cứ kết luận server vẫn kết nối. |
| `bJ.e:db`, `bJ.f:db` | Trường tài khoản/mật khẩu; có setter `db.ah(String)`. Chưa gọi trong bản 1. |

Client đã làm rối tên. Nhiều field cùng tên `a` nhưng khác descriptor; reflection phải chọn theo **cả tên và kiểu**, không dùng `getDeclaredField("a")`. Phương thức cũng có thể trùng tên và tham số nhưng khác kiểu trả về; mọi adapter ghi sau này phải xét đủ signature.

Không bật lệnh login chỉ dựa vào tên nút: ví dụ handler `bJ.a(2000,Object)` lưu trường nhập vào `cI.p/q` và về menu. Luồng `bJ.a()` thiết lập các nút khác, gồm cả hộp thoại thông báo. Cần kiểm chứng luồng thực tế trước khi gọi tự động.

## Giao thức và vòng đời

Mỗi lần mở client tạo listener/cổng tạm và token ngẫu nhiên 256 bit riêng. Truyền cổng/token qua môi trường của tiến trình con; không ghi token, tài khoản hay mật khẩu vào log. Agent kết nối từ JVM bằng `-javaagent`; không sửa JAR game hoặc JAR MicroEmulator.

1. Agent gửi `HELLO\t1\tTOKEN\n`; tool xác thực và trả `OK\n`.
2. Tool gửi `POLL\n` mỗi 750 ms; chỉ một yêu cầu đang chờ.
3. Agent trả `STATE\tPHASE\tBASE64_SCREEN\tBASE64_CHARACTER_NAMES\n`. Tên nhân vật phân cách bằng newline trước khi base64 UTF-8.
4. Mất kết nối, phản hồi quá dài hoặc quá 4 giây không có phản hồi: đánh dấu mất cầu nối. Không tự lặp đăng nhập hay đóng game.
5. Tắt/restart hồ sơ hủy phiên cũ. JVM mới có cổng/token mới, không nhận trạng thái từ lần chạy trước.

Token là phân tách phiên giữa các tiến trình cục bộ, không phải biện pháp chống phần mềm có toàn quyền trên Windows. Giao thức bản 1 chỉ đọc dữ liệu danh sách nhân vật đã cho phép; không xuất field tùy ý, nội dung text nhập hay stack trace của client.

## Build và kiểm tra

Agent viết mới bằng Java, không phân phối mã dịch ngược hoặc thư viện của ZangVPS. Source ở `bridge/src`, JAR được đóng gói base64 trong resource của tool. Người dùng chỉ chạy `Update-Build.bat` như trước; không cần cài thêm JDK. Tool tự trích xuất agent có tên chứa hash phiên bản vào runtime.

Người phát triển dùng JDK 17: `python bridge/build.py`. CI đối chiếu class đã đóng gói với source bằng `--check`, chạy `bridge/test.py` và `tests/BridgeTests`. Test bao phủ field trùng tên/khác descriptor, lớp màn hình, hộp thoại, token, dữ liệu Unicode, phân tách phiên, giới hạn phản hồi và mất kết nối.

Kiểm tra thực tế còn cần trên Windows với game/server: mở hồ sơ mới/cũ, mở ô tài khoản, đăng nhập thủ công tới chọn nhân vật, vào game và ngắt mạng. Đối chiếu cột trạng thái và tooltip với ảnh. Chưa xác nhận trực quan toàn bộ bốn màn hình bằng client đang chạy trên VPS của người dùng.

## Bước tiếp theo của adapter ghi

Xác nhận đủ trạng thái menu mới/cũ, hộp thoại login và lỗi; gọi thao tác trên đúng luồng xử lý game; chờ phản hồi server để xác nhận đăng nhập; chỉ chọn tên nhân vật khớp cấu hình; timeout và dừng khi sai mật khẩu/nhân vật không tồn tại. Không đưa mật khẩu lên dòng lệnh hoặc dùng SendKeys làm phương án dự phòng.
