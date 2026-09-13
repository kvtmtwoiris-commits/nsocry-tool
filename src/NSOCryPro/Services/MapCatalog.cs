using NSOCryPro.Models;

namespace NSOCryPro.Services;

public static class MapCatalog
{
    // Các map thế giới thường dùng để train. Map đặc biệt vẫn được GET và thêm động từ client.
    public static IReadOnlyList<MapOption> TrainingMaps { get; } = new MapOption[]
    {
        new(0, "Nhà thi đấu Haruna"), new(1, "Trường Hirosaki"), new(2, "Khu luyện tập"),
        new(3, "Động Hachi"), new(4, "Rừng đào Sakura"), new(5, "Rừng trúc Ura"),
        new(6, "Thác Kitajima"), new(7, "Rừng Mishima"), new(8, "Sông Watamaro"),
        new(9, "Nghĩa địa Izuko"), new(10, "Làng Kojin"), new(11, "Miếu Kamo"),
        new(12, "Miếu Oboko"), new(13, "Rừng gỗ Kouji"), new(14, "Rừng Aokigahara"),
        new(15, "Vách núi Ito"), new(16, "Thung lũng Taira"), new(17, "Làng Sanzu"),
        new(18, "Sân đền Orochi"), new(19, "Ngôi đền Orochi"), new(20, "Chân thác Kitajima"),
        new(21, "Đồi Fumimen"), new(22, "Làng Tone"), new(23, "Vách Ichidai"),
        new(24, "Đỉnh Ichidai"), new(25, "Đồi Kokoro"), new(26, "Cánh đồng Fuki"),
        new(27, "Trường Haruna"), new(28, "Ký túc xá Haruna"), new(29, "Hang Aka"),
        new(30, "Suối Akagi"), new(31, "Bờ biển Oura"), new(32, "Làng chài"),
        new(33, "Rừng Moshio"), new(34, "Đảo Hebi"), new(35, "Hang Meiro"),
        new(36, "Động Kisei"), new(37, "Núi Hashigoto"), new(38, "Làng Chakumi"),
        new(39, "Sông băng Yamato"), new(40, "Cánh đồng Hiya"), new(41, "Khu đá đỏ Akai"),
        new(42, "Khu đá đỏ Aiko"), new(43, "Làng Echigo"), new(44, "Đỉnh Okama"),
        new(45, "Hang núi Kurai"), new(46, "Hồ Stuki"), new(47, "Hẻm núi Takana"),
        new(48, "Làng Oshin"), new(49, "Đền Amaterasu"), new(50, "Rừng Kanashii"),
        new(51, "Rừng Toge"), new(52, "Rừng Kappa"), new(53, "Động Tamatamo"),
        new(54, "Đền Harumoto"), new(55, "Phòng ẩn Ounio"), new(56, "Nhà thi đấu Ookaza"),
        new(57, "Sân sau miếu Oboko"), new(58, "Sân sau đền Orochi"), new(59, "Mũi Hone"),
        new(60, "Cửa hang Aka"), new(61, "Cửa biển Kawaguchi"), new(62, "Hang Chi"),
        new(63, "Hang Ha"), new(64, "Hang Kugyou"), new(65, "Mũi Nuranura"),
        new(66, "Khe núi Chorochoro"), new(67, "Núi Ontake"), new(68, "Núi Anzen"),
        new(69, "Vách Ainodake"), new(70, "Thung lũng chết"), new(71, "Rừng già"),
        new(72, "Trường Ookaza")
    };
}
