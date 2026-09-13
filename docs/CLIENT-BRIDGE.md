# NSOCry Pro — cầu nối JVM, phiên bản 1

## Phạm vi hiện tại

Kênh TCP hai chiều trên `127.0.0.1` đọc trạng thái thật và điều khiển luồng đăng nhập ngay trong JVM. Không dùng OCR, tọa độ hoặc SendKeys. Khi bật Tự đăng nhập, tool chuyển thông tin qua phiên đã xác thực; agent gọi action đăng nhập sẵn có của client và chọn chính xác tên nhân vật trong cấu hình.

Không coi tiến trình Java còn chạy, socket còn mở hay màn hình game còn hiển thị là xác nhận nhân vật đang online trên server. Bản này báo **Màn hình game**, không báo **Online**. Khi đang ở màn hình game, cầu nối đọc ID và tên map hiện tại do client đã nhận từ server; chưa có heartbeat ở giao thức game.

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
| `dg.X:short`, `dg.hT:String` | ID và tên map hiện tại. Nút GET chỉ nhận hai field đã cho phép này khi `ba` đang là màn hình hoạt động. |
| `bJ.e:db`, `bJ.f:db` | Trường tài khoản/mật khẩu; agent điền qua `db.ah(String)` nếu client đang ở màn hình này. |

Client đã làm rối tên. Nhiều field cùng tên `a` nhưng khác descriptor; reflection phải chọn theo **cả tên và kiểu**, không dùng `getDeclaredField("a")`. Phương thức cũng có thể trùng tên và tham số nhưng khác kiểu trả về; mọi adapter ghi sau này phải xét đủ signature.

Handler `bJ.a(2000,Object)` lưu trường nhập vào `cI.p/q` và về menu. Ở menu, agent đặt `cI.p/q` rồi gọi `cI.a(1003,Object)`; client tự lưu RMS và gửi login bằng `cK.c(...)`. Khi `cH.F` xuất hiện, agent đặt chỉ số `cH.q` theo tên cấu hình rồi gọi `cH.a(1000,Object)`, cùng đường lệnh chọn nhân vật của client.

## Giao thức và vòng đời

Mỗi lần mở client tạo listener/cổng tạm và token ngẫu nhiên 256 bit riêng. Truyền cổng/token qua môi trường của tiến trình con; không ghi token, tài khoản hay mật khẩu vào log. Agent kết nối từ JVM bằng `-javaagent`; không sửa JAR game hoặc JAR MicroEmulator.

1. Agent gửi `HELLO\t1\tTOKEN\n`; tool xác thực và trả `OK\n`.
2. Nếu bật tự đăng nhập, tool gửi một lệnh `LOGIN` chứa ba trường base64 UTF-8 và chờ `ACK`. Mật khẩu không nằm trong dòng lệnh tiến trình hoặc biến môi trường.
3. Tool gửi `POLL\n` mỗi 750 ms; chỉ một yêu cầu đang chờ.
4. Agent trả trạng thái màn hình, tên nhân vật, tiến độ tự đăng nhập, ID map và tên map. Chuỗi được mã hóa base64 UTF-8.
4. Mất kết nối, phản hồi quá dài hoặc quá 4 giây không có phản hồi: đánh dấu mất cầu nối. Không tự lặp đăng nhập hay đóng game.
5. Tắt/restart hồ sơ hủy phiên cũ. JVM mới có cổng/token mới, không nhận trạng thái từ lần chạy trước.

Token là phân tách phiên giữa các tiến trình cục bộ, không phải biện pháp chống phần mềm có toàn quyền trên Windows. Giao thức bản 1 chỉ đọc danh sách nhân vật và map đã cho phép; không xuất field tùy ý, nội dung text nhập hay stack trace của client.

## Cấu hình map đánh quái

Danh sách chọn sẵn gồm các map thế giới 0–72 thường dùng để train. Checkbox, ID và tên map được lưu riêng trong từng hồ sơ. Nút **GET** lấy `dg.X` + `dg.hT` từ đúng phiên client của hồ sơ đang chọn; map sự kiện hoặc map đặc biệt ngoài danh sách được thêm động và vẫn được lưu. GET chỉ khả dụng khi client đã vào màn hình game. Checkbox hiện lưu cấu hình mục tiêu; logic tự tìm và đánh quái được triển khai ở bước automation riêng.

Cấu hình train còn lưu chế độ chọn khu, khu cố định và ba loại mục tiêu: quái thường, tinh anh (TA), thủ lĩnh (TL). `Tàn sát map trống` có nghĩa là ưu tiên khu 0 người, nếu không có thì chọn khu ít người nhất. Ở thay đổi hiện tại đây là cấu hình giao diện đã lưu; lệnh yêu cầu danh sách khu, đếm người và đổi khu trong client sẽ được nối riêng sau khi adapter được kiểm chứng.

## Build và kiểm tra

Agent viết mới bằng Java, không phân phối mã dịch ngược hoặc thư viện của ZangVPS. Source ở `bridge/src`, JAR được đóng gói base64 trong resource của tool. Người dùng chỉ chạy `Update-Build.bat` như trước; không cần cài thêm JDK. Tool tự trích xuất agent có tên chứa hash phiên bản vào runtime.

Người phát triển dùng JDK 17: `python bridge/build.py`. CI đối chiếu class đã đóng gói với source bằng `--check`, chạy `bridge/test.py` và `tests/BridgeTests`. Test bao phủ field trùng tên/khác descriptor, lớp màn hình, hộp thoại, token, dữ liệu Unicode, phân tách phiên, giới hạn phản hồi và mất kết nối.

Kiểm tra thực tế còn cần trên Windows với game/server: mở hồ sơ mới/cũ, mở ô tài khoản, đăng nhập thủ công tới chọn nhân vật, vào game và ngắt mạng. Đối chiếu cột trạng thái và tooltip với ảnh. Chưa xác nhận trực quan toàn bộ bốn màn hình bằng client đang chạy trên VPS của người dùng.

## Giới hạn cần kiểm tra thực tế

Agent chỉ gửi login một lần trong mỗi lần mở client. Hộp thoại lỗi không bị tự đóng; sai mật khẩu sẽ dừng để người dùng đọc lỗi. Nếu tên nhân vật cấu hình không khớp danh sách server, trạng thái là `CHARACTER_NOT_FOUND` và không tự chọn nhân vật khác. Cần đối chiếu thực tế trên Windows với cả hồ sơ mới và hồ sơ đã có RMS.
