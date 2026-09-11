# NSOCry Pro

Tool quản lý nhiều client NSO trên Windows, sử dụng MicroEmulator và vùng RMS riêng cho từng hồ sơ.

## Chức năng hiện tại

- Thêm, sửa và xóa hồ sơ client.
- Mở một hoặc nhiều client cùng lúc.
- Dừng và khởi động lại client.
- Tách dữ liệu RMS theo từng hồ sơ.
- Theo dõi trạng thái và RAM của từng tiến trình.
- Lưu danh sách hồ sơ tự động.

## Yêu cầu

- Windows 10/11 hoặc Windows Server 2019 trở lên.
- .NET 8 Desktop Runtime.
- Java 8 hoặc Java 17 có `javaw.exe` trong PATH.

## Chuẩn bị runtime

Sau khi build, tạo thư mục `runtime` cạnh `NSOCryPro.exe` và chép vào:

```text
runtime/
  microemulator.jar
  game.jar
```

`game.jar` là client NSO có MIDlet chính `GameMidlet`.

## Build

```powershell
dotnet build NSOCryPro.sln -c Release
dotnet publish src/NSOCryPro/NSOCryPro.csproj -c Release -r win-x64 --self-contained false
```

Giai đoạn tiếp theo sẽ bổ sung điều khiển cửa sổ, auto restart, trạng thái nhân vật và các tab auto.

## Cập nhật và build tự động

Nhấp đúp vào `Update-Build.bat`. Script sẽ tự pull mã mới, build bản Release và đặt kết quả trong:

```text
dist/NSOCryPro.exe
```

Trước lần build đầu tiên, đặt `microemulator.jar` và `game.jar` trong thư mục `runtime` tại gốc repository.

## Tài liệu giao diện

Mọi thay đổi GUI phải tuân theo [NSOCry Pro UI Design System](docs/UI-DESIGN-SYSTEM.md).
