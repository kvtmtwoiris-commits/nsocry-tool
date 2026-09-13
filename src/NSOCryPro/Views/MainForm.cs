using System.ComponentModel;
using System.Drawing.Drawing2D;
using NSOCryPro.Models;
using NSOCryPro.Services;

namespace NSOCryPro.Views;

public sealed class MainForm : Form
{
    private static readonly Color Bg = Color.FromArgb(244, 247, 251);
    private static readonly Color White = Color.White;
    private static readonly Color Navy = Color.FromArgb(17, 32, 61);
    private static readonly Color Blue = Color.FromArgb(37, 99, 235);
    private static readonly Color BlueSoft = Color.FromArgb(239, 246, 255);
    private static readonly Color Green = Color.FromArgb(5, 150, 105);
    private static readonly Color GreenSoft = Color.FromArgb(236, 253, 245);
    private static readonly Color Red = Color.FromArgb(225, 29, 72);
    private static readonly Color RedSoft = Color.FromArgb(255, 241, 242);
    private static readonly Color Amber = Color.FromArgb(217, 119, 6);
    private static readonly Color AmberSoft = Color.FromArgb(255, 251, 235);
    private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
    private static readonly Color Muted = Color.FromArgb(100, 116, 139);
    private static readonly Color Line = Color.FromArgb(226, 232, 240);

    private readonly JsonProfileStore _store;
    private readonly ClientProcessManager _processManager;
    private readonly BindingList<ClientProfile> _profiles;
    private readonly DataGridView _grid = new BufferedGrid();
    private readonly Label _total = MetricValue();
    private readonly Label _running = MetricValue();
    private readonly Label _offline = MetricValue();
    private readonly Label _memory = MetricValue();
    private readonly Label _status = new();
    private readonly ModernCheckBox _trainEnabled = new() { Text = "Đánh quái:" };
    private readonly ComboBox _trainMap = new();
    private readonly ModernRadioButton _emptyZone = new() { Text = "Tàn sát map trống" };
    private readonly ModernRadioButton _fixedZone = new() { Text = "Khu vực:" };
    private readonly NumericUpDown _trainZone = new();
    private readonly ModernCheckBox _normalMonsters = new() { Text = "Đánh quái thường" };
    private readonly ModernCheckBox _eliteMonsters = new() { Text = "Đánh TA" };
    private readonly ModernCheckBox _chiefMonsters = new() { Text = "Đánh TL" };
    private bool _loadingTrainSettings;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1500 };

    public MainForm(JsonProfileStore store, ClientProcessManager processManager)
    {
        _store = store;
        _processManager = processManager;
        _profiles = new BindingList<ClientProfile>(_store.Load());

        Text = "NSOCry Pro";
        BackColor = Bg;
        ForeColor = TextPrimary;
        Font = new Font("Segoe UI", 9.5F);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1120, 700);
        Size = new Size(1320, 820);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildUi();
        _timer.Tick += (_, _) => RefreshStatus();
        _timer.Start();
        FormClosing += (_, _) =>
        {
            Save();
            _processManager.Dispose();
        };
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Bg, Padding = new Padding(24, 20, 24, 16),
            ColumnCount = 1, RowCount = 6
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 280));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        root.Controls.Add(Header(), 0, 0);
        root.Controls.Add(Metrics(), 0, 1);
        root.Controls.Add(ActionBar(), 0, 2);
        root.Controls.Add(GridCard(), 0, 3);
        root.Controls.Add(SettingsCard(), 0, 4);
        root.Controls.Add(Footer(), 0, 5);
        Controls.Add(root);
        LoadTrainSettings();
        RefreshStatus();
    }

    private Control Header()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Bg };
        var logo = new RoundPanel { Size = new Size(46, 46), Location = new Point(0, 4), BackColor = Blue, Radius = 13 };
        logo.Controls.Add(new Label
        {
            Text = "N", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = White, Font = new Font("Segoe UI Black", 20F)
        });
        panel.Controls.Add(logo);
        panel.Controls.Add(new Label
        {
            Text = "NSOCry Pro", AutoSize = true, Location = new Point(60, 1),
            ForeColor = Navy, Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold)
        });
        panel.Controls.Add(new Label
        {
            Text = "Bảng điều khiển client Ninja School", AutoSize = true, Location = new Point(63, 40),
            ForeColor = Muted, Font = new Font("Segoe UI", 9.5F)
        });
        var badge = new RoundPanel
        {
            Size = new Size(184, 38), BackColor = GreenSoft, Radius = 19,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        badge.Controls.Add(new Label
        {
            Text = "●  Bokken đang hoạt động", Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = Green,
            Font = new Font("Segoe UI Semibold", 9F)
        });
        panel.Controls.Add(badge);
        panel.Resize += (_, _) => badge.Location = new Point(panel.ClientSize.Width - badge.Width, 8);
        return panel;
    }

    private Control Metrics()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 4, BackColor = Bg,
            Padding = new Padding(0, 4, 0, 14)
        };
        for (var i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.Controls.Add(MetricCard("TỔNG CLIENT", "Tất cả hồ sơ", _total, Blue, BlueSoft), 0, 0);
        layout.Controls.Add(MetricCard("ĐANG CHẠY", "Client hoạt động", _running, Green, GreenSoft), 1, 0);
        layout.Controls.Add(MetricCard("OFFLINE", "Chưa khởi động", _offline, Red, RedSoft), 2, 0);
        layout.Controls.Add(MetricCard("BỘ NHỚ", "RAM đang sử dụng", _memory, Amber, AmberSoft), 3, 0);
        return layout;
    }

    private static Control MetricCard(string title, string note, Label value, Color accent, Color soft)
    {
        var card = new RoundPanel
        {
            Dock = DockStyle.Fill, Margin = new Padding(0, 0, 14, 0),
            BackColor = White, BorderColor = Line, Radius = 14
        };
        var marker = new RoundPanel
        {
            Size = new Size(42, 42), Location = new Point(18, 21),
            BackColor = soft, Radius = 12
        };
        marker.Controls.Add(new Label
        {
            Text = "●", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = accent, Font = new Font("Segoe UI", 14F)
        });
        card.Controls.Add(marker);
        card.Controls.Add(new Label
        {
            Text = title, AutoSize = true, Location = new Point(76, 14),
            ForeColor = Muted, Font = new Font("Segoe UI Semibold", 8.5F)
        });
        value.Location = new Point(74, 33);
        card.Controls.Add(value);
        card.Controls.Add(new Label
        {
            Text = note, AutoSize = true, Location = new Point(76, 65),
            ForeColor = Muted, Font = new Font("Segoe UI", 8.5F)
        });
        return card;
    }

    private Control ActionBar()
    {
        var card = new RoundPanel
        {
            Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(12, 9, 12, 8), BackColor = White,
            BorderColor = Line, Radius = 12
        };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, WrapContents = false };
        flow.Controls.AddRange([
            Button("Mở game", StartSelected, ButtonStyle.Primary),
            Button("Thêm hồ sơ", AddProfile, ButtonStyle.Normal),
            Button("Sửa hồ sơ", EditSelected, ButtonStyle.Normal),
            Button("Xóa", DeleteSelected, ButtonStyle.Danger),
            Separator(),
            Button("Mở tất cả", StartAll, ButtonStyle.Normal),
            Button("Dừng tất cả", StopAll, ButtonStyle.Normal),
            Button("Khởi động lại", RestartSelected, ButtonStyle.Normal),
            Button("Lưu thay đổi", Save, ButtonStyle.Normal)
        ]);
        card.Controls.Add(flow);
        return card;
    }

    private Control GridCard()
    {
        var card = new RoundPanel
        {
            Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 14),
            Padding = new Padding(1), BackColor = White, BorderColor = Line, Radius = 14
        };
        ConfigureGrid();
        card.Controls.Add(_grid);
        return card;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.BorderStyle = BorderStyle.None;
        _grid.BackgroundColor = White;
        _grid.GridColor = Line;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = true;
        _grid.EditMode = DataGridViewEditMode.EditOnEnter;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 46;
        _grid.RowTemplate.Height = 60;
        _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(248, 250, 252), ForeColor = Muted,
            SelectionBackColor = Color.FromArgb(248, 250, 252),
            Font = new Font("Segoe UI Semibold", 8.5F), Padding = new Padding(8, 0, 8, 0)
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = White, ForeColor = TextPrimary, SelectionBackColor = BlueSoft,
            SelectionForeColor = TextPrimary, Font = new Font("Segoe UI", 9.5F),
            Padding = new Padding(8, 0, 8, 0)
        };
        _grid.DataSource = _profiles;
        _grid.Columns.Add(CheckColumn(nameof(ClientProfile.Selected), "", 46));
        _grid.Columns.Add(TextColumn(nameof(ClientProfile.CharacterName), "NHÂN VẬT", 190, true));
        _grid.Columns.Add(TextColumn(nameof(ClientProfile.Account), "TÀI KHOẢN", 220, true));
        _grid.Columns.Add(TextColumn(nameof(ClientProfile.Server), "SERVER", 115, true));
        _grid.Columns.Add(TextColumn("Status", "TRẠNG THÁI", 130, true));
        _grid.Columns.Add(TextColumn("Pid", "PID", 80, true));
        _grid.Columns.Add(TextColumn("Ram", "RAM", 100, true));
        _grid.Columns.Add(CheckColumn(nameof(ClientProfile.AutoLogin), "TỰ ĐĂNG NHẬP", 120));
        _grid.Columns.Add(CheckColumn(nameof(ClientProfile.AutoRestart), "TỰ KHỞI ĐỘNG", 120));
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        foreach (DataGridViewColumn column in _grid.Columns)
        {
            column.FillWeight = column.Width;
            column.MinimumWidth = column.Name == nameof(ClientProfile.Selected) ? 44 : 72;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        _grid.Columns[nameof(ClientProfile.AutoLogin)].HeaderText = "Đăng nhập";
        _grid.Columns[nameof(ClientProfile.AutoRestart)].HeaderText = "Chạy lại";
        _grid.Columns[nameof(ClientProfile.AutoLogin)].MinimumWidth = 100;
        _grid.Columns[nameof(ClientProfile.AutoRestart)].MinimumWidth = 90;
        _grid.Columns["Status"].MinimumWidth = 155;
        _grid.Columns[nameof(ClientProfile.AutoLogin)].HeaderCell.ToolTipText = "Đăng nhập trực tiếp trong JVM sau khi mở client.";
        _grid.Columns[nameof(ClientProfile.CharacterName)].MinimumWidth = 130;
        _grid.Columns[nameof(ClientProfile.Account)].MinimumWidth = 120;
        _grid.Columns[nameof(ClientProfile.AutoRestart)].ReadOnly = true;
        _grid.Columns[nameof(ClientProfile.AutoRestart)].HeaderCell.ToolTipText = "Tự khởi động lại chưa được triển khai.";
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.CellPainting += PaintGridCell;
        _grid.RowPostPaint += (_, e) =>
        {
            using var line = new Pen(Line);
            e.Graphics.DrawLine(line, e.RowBounds.Left, e.RowBounds.Bottom - 1, e.RowBounds.Right, e.RowBounds.Bottom - 1);
        };
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Columns[e.ColumnIndex] is not DataGridViewCheckBoxColumn) EditSelected();
        };
        _grid.SelectionChanged += (_, _) => LoadTrainSettings();
    }

    private void PaintGridCell(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.Graphics is null) return;
        var g = e.Graphics;
        var saved = g.Save();
        try
        {
            g.SetClip(e.CellBounds);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float scale = _grid.DeviceDpi / 96f;
            int D(float value) => Math.Max(1, (int)Math.Round(value * scale));
            var column = _grid.Columns[e.ColumnIndex];
            var bounds = e.CellBounds;
            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            var background = e.RowIndex < 0 ? Color.FromArgb(248, 250, 252) : selected ? BlueSoft : White;
            using var brush = new SolidBrush(background);
            g.FillRectangle(brush, bounds);

            if (e.RowIndex < 0)
            {
                var caption = column.HeaderText;
                var headerBounds = Rectangle.Inflate(bounds, -D(12), 0);
                var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
                if (column is DataGridViewCheckBoxColumn) flags |= TextFormatFlags.HorizontalCenter;
                TextRenderer.DrawText(g, caption, _grid.ColumnHeadersDefaultCellStyle.Font,
                    headerBounds, Muted, flags);
            }
            else if (column is DataGridViewCheckBoxColumn)
            {
                bool unavailable = column.Name == nameof(ClientProfile.AutoRestart);
                bool check = !unavailable && e.FormattedValue is bool value && value;
                int size = D(18);
                var box = new Rectangle(bounds.X + (bounds.Width - size) / 2,
                    bounds.Y + (bounds.Height - size) / 2, size, size);
                using var shape = RoundedRectangle(box, D(5));
                using var fill = new SolidBrush(unavailable ? Bg : check ? Blue : White);
                using var pen = new Pen(check ? Blue : Color.FromArgb(203, 213, 225), scale);
                g.FillPath(fill, shape);
                g.DrawPath(pen, shape);
                if (check)
                {
                    using var tick = new Pen(White, 2 * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                    g.DrawLines(tick, new PointF[] {
                        new(box.X + 4 * scale, box.Y + 9 * scale),
                        new(box.X + 8 * scale, box.Y + 13 * scale),
                        new(box.X + 14 * scale, box.Y + 5 * scale) });
                }
            }
            else if (column.Name == "Status")
            {
                bool active = Equals(e.Value, "Màn hình game");
                var pill = new Rectangle(bounds.X + D(12), bounds.Y + (bounds.Height - D(26)) / 2,
                    bounds.Width - D(24), D(26));
                using var shape = RoundedRectangle(pill, D(13));
                using var fill = new SolidBrush(active ? GreenSoft : Bg);
                g.FillPath(fill, shape);
                TextRenderer.DrawText(g, Convert.ToString(e.Value) ?? "Chờ cầu nối", _grid.Font,
                    pill, active ? Green : Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            }
            else
            {
                var value = Convert.ToString(e.FormattedValue);
                if (string.IsNullOrWhiteSpace(value)) value = "—";
                var textBounds = Rectangle.Inflate(bounds, -D(12), 0);
                TextRenderer.DrawText(g, value, _grid.Font, textBounds,
                    column.Name == nameof(ClientProfile.CharacterName) ? Navy : Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            using var divider = new Pen(Line);
            g.DrawLine(divider, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
            e.Handled = true;
        }
        finally { g.Restore(saved); }
    }

    private static GraphicsPath RoundedRectangle(Rectangle rectangle, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        var arc = new Rectangle(rectangle.X, rectangle.Y, diameter, diameter);
        path.AddArc(arc, 180, 90);
        arc.X = rectangle.Right - diameter; path.AddArc(arc, 270, 90);
        arc.Y = rectangle.Bottom - diameter; path.AddArc(arc, 0, 90);
        arc.X = rectangle.Left; path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    private Control SettingsCard()
    {
        var card = new RoundPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(16, 12, 16, 14), BackColor = White,
            BorderColor = Line, Radius = 14
        };
        var tabs = new ModernTabs(42, 150, 9F);
        tabs.AddPage("Tổng quan", InfoPage("Quản lý tập trung",
            "Chọn hồ sơ trong bảng, sau đó mở, dừng hoặc khởi động lại client bằng thanh thao tác."));
        tabs.AddPage("Cấu hình Auto", AutoConfigurationPage());
        tabs.AddPage("Nhật ký", InfoPage("Theo dõi hoạt động",
            "Lịch sử thao tác và sự kiện của từng client sẽ được hiển thị tại đây."));
        card.Controls.Add(tabs);
        return card;
    }

    private Control AutoConfigurationPage()
    {
        var page = new Panel
        {
            Dock = DockStyle.Fill, BackColor = White, Padding = new Padding(0, 8, 0, 0)
        };
        var autoTabs = new ModernTabs(38, 168, 8.75F);
        autoTabs.Name = "AutoFeatureTabs";
        autoTabs.AddPage("Đánh quái (Train)", TrainPage());
        page.Controls.Add(autoTabs);
        return page;
    }

    private Control TrainPage()
    {
        var page = new Panel
        {
            Dock = DockStyle.Fill, BackColor = White, Padding = new Padding(0, 8, 0, 0)
        };
        var trainTabs = new ModernTabs(38, 132, 8.5F);
        trainTabs.Name = "TrainSettingsTabs";
        trainTabs.AddPage("Cài đặt cơ bản", TrainBasicSection());
        trainTabs.AddPage("Nâng cao", AutoSection("Nâng cao", "Các điều kiện và giới hạn nâng cao."));
        trainTabs.AddPage("Gán skill", AutoSection("Gán skill", "Thiết lập kỹ năng dùng khi train."));
        trainTabs.AddPage("Kiểu đánh quái", AutoSection("Kiểu đánh quái", "Chọn cách tìm và tấn công mục tiêu."));
        trainTabs.AddPage("Kích yên", AutoSection("Kích yên", "Thiết lập kích yên cho chế độ train."));
        page.Controls.Add(trainTabs);
        return page;
    }

    private Control TrainBasicSection()
    {
        var page = new Panel { Dock = DockStyle.Fill, BackColor = White, Padding = new Padding(4, 10, 4, 4) };
        var card = new RoundPanel
        {
            Dock = DockStyle.Fill, BackColor = Bg, BorderColor = Line, Radius = 12,
            Padding = new Padding(16, 10, 16, 10)
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        var mapRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.Transparent, WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0)
        };
        _trainEnabled.Size = new Size(116, 34);
        _trainEnabled.Margin = new Padding(0, 0, 10, 0);
        _trainEnabled.CheckedChanged += (_, _) => SaveTrainEnabled();
        _trainMap.DropDownStyle = ComboBoxStyle.DropDownList;
        _trainMap.FlatStyle = FlatStyle.Flat;
        _trainMap.Font = new Font("Segoe UI", 9.25F);
        _trainMap.Width = 270;
        _trainMap.Height = 34;
        _trainMap.Margin = new Padding(0, 4, 10, 0);
        _trainMap.BackColor = White;
        _trainMap.ForeColor = TextPrimary;
        _trainMap.SelectionChangeCommitted += (_, _) => SaveTrainMap();
        var get = Button("GET", GetCurrentMap, ButtonStyle.Primary);
        get.Width = 72;
        get.AutoSize = false;
        get.Margin = new Padding(0, 0, 12, 0);
        mapRow.Controls.AddRange([_trainEnabled, _trainMap, get, new Label
        {
            Text = "Đọc vị trí hiện tại từ client", AutoSize = true, ForeColor = Muted,
            Font = new Font("Segoe UI", 8.75F), Margin = new Padding(0, 9, 0, 0)
        }]);
        layout.Controls.Add(mapRow, 0, 0);

        var modeRow = OptionRow();
        _emptyZone.Size = new Size(174, 30);
        _emptyZone.CheckedChanged += (_, _) => ChangeTrainMode(true);
        _normalMonsters.Size = new Size(166, 30);
        _normalMonsters.CheckedChanged += (_, _) => SaveTrainOptions();
        modeRow.Controls.AddRange([_emptyZone, _normalMonsters]);
        layout.Controls.Add(modeRow, 0, 1);

        var targetRow = OptionRow();
        _fixedZone.Size = new Size(86, 30);
        _fixedZone.CheckedChanged += (_, _) => ChangeTrainMode(false);
        _trainZone.Minimum = 0;
        _trainZone.Maximum = 30;
        _trainZone.Width = 58;
        _trainZone.Height = 28;
        _trainZone.Font = new Font("Segoe UI", 9F);
        _trainZone.BackColor = White;
        _trainZone.ForeColor = TextPrimary;
        _trainZone.BorderStyle = BorderStyle.FixedSingle;
        _trainZone.Margin = new Padding(0, 2, 20, 0);
        _trainZone.ValueChanged += (_, _) => SaveTrainOptions();
        _eliteMonsters.Size = new Size(104, 30);
        _eliteMonsters.CheckedChanged += (_, _) => SaveTrainOptions();
        _chiefMonsters.Size = new Size(104, 30);
        _chiefMonsters.CheckedChanged += (_, _) => SaveTrainOptions();
        targetRow.Controls.AddRange([_fixedZone, _trainZone, _eliteMonsters, _chiefMonsters]);
        layout.Controls.Add(targetRow, 0, 2);
        card.Controls.Add(layout);
        page.Controls.Add(card);
        return page;
    }

    private static FlowLayoutPanel OptionRow() => new()
    {
        Dock = DockStyle.Fill, BackColor = Color.Transparent, WrapContents = false,
        FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0)
    };

    private ClientProfile? CurrentProfile() => _grid.CurrentRow?.DataBoundItem as ClientProfile;

    private void LoadTrainSettings()
    {
        if (_trainMap.IsDisposed) return;
        _loadingTrainSettings = true;
        try
        {
            var profile = CurrentProfile();
            _trainEnabled.Enabled = profile is not null;
            _trainMap.Enabled = profile is not null;
            _trainEnabled.Checked = profile?.TrainEnabled == true;
            _emptyZone.Enabled = profile is not null;
            _fixedZone.Enabled = profile is not null;
            _normalMonsters.Enabled = profile is not null;
            _eliteMonsters.Enabled = profile is not null;
            _chiefMonsters.Enabled = profile is not null;
            _emptyZone.Checked = profile?.TrainEmptyZone == true;
            _fixedZone.Checked = profile is not null && !profile.TrainEmptyZone;
            _trainZone.Enabled = profile is not null && !profile.TrainEmptyZone;
            _trainZone.Value = Math.Clamp(profile?.TrainZone ?? 0, (int)_trainZone.Minimum, (int)_trainZone.Maximum);
            _normalMonsters.Checked = profile?.TrainNormalMonsters == true;
            _eliteMonsters.Checked = profile?.TrainEliteMonsters == true;
            _chiefMonsters.Checked = profile?.TrainChiefMonsters == true;
            _trainMap.BeginUpdate();
            _trainMap.Items.Clear();
            foreach (var map in MapCatalog.TrainingMaps) _trainMap.Items.Add(map);
            if (profile is null) { _trainMap.SelectedIndex = -1; return; }
            var selected = _trainMap.Items.Cast<MapOption>().FirstOrDefault(map => map.Id == profile.TrainMapId);
            if (selected is null || (!string.IsNullOrWhiteSpace(profile.TrainMapName) && selected.Name != profile.TrainMapName))
            {
                if (selected is not null) _trainMap.Items.Remove(selected);
                selected = new MapOption(profile.TrainMapId, profile.TrainMapName);
                _trainMap.Items.Add(selected);
            }
            _trainMap.SelectedItem = selected;
        }
        finally
        {
            _trainMap.EndUpdate();
            _loadingTrainSettings = false;
        }
    }

    private void SaveTrainEnabled()
    {
        if (_loadingTrainSettings || CurrentProfile() is not { } profile) return;
        profile.TrainEnabled = _trainEnabled.Checked;
        Save();
        SetStatus(profile.TrainEnabled ? "Đã bật cấu hình đánh quái" : "Đã tắt cấu hình đánh quái");
    }

    private void SaveTrainMap()
    {
        if (_loadingTrainSettings || CurrentProfile() is not { } profile || _trainMap.SelectedItem is not MapOption map) return;
        profile.TrainMapId = map.Id;
        profile.TrainMapName = map.Name;
        Save();
        SetStatus($"Đã chọn map {map}");
    }

    private void SaveTrainOptions()
    {
        if (_loadingTrainSettings || CurrentProfile() is not { } profile) return;
        profile.TrainEmptyZone = _emptyZone.Checked;
        profile.TrainZone = (int)_trainZone.Value;
        profile.TrainNormalMonsters = _normalMonsters.Checked;
        profile.TrainEliteMonsters = _eliteMonsters.Checked;
        profile.TrainChiefMonsters = _chiefMonsters.Checked;
        _trainZone.Enabled = !profile.TrainEmptyZone;
        Save();
        SetStatus(profile.TrainEmptyZone ? "Ưu tiên khu trống hoặc ít người nhất" : $"Đã chọn khu vực {profile.TrainZone}");
    }

    private void ChangeTrainMode(bool emptyZone)
    {
        if (_loadingTrainSettings) return;
        var source = emptyZone ? _emptyZone : _fixedZone;
        if (!source.Checked) return;
        _loadingTrainSettings = true;
        try
        {
            _emptyZone.Checked = emptyZone;
            _fixedZone.Checked = !emptyZone;
        }
        finally { _loadingTrainSettings = false; }
        SaveTrainOptions();
    }

    private void GetCurrentMap()
    {
        var profile = CurrentProfile();
        if (profile is null) { SetStatus("Hãy chọn một hồ sơ trước khi lấy map"); return; }
        var snapshot = _processManager.GetSnapshot(profile.Id);
        if (snapshot?.Phase != "GAME_SCREEN" || snapshot.MapId is null || string.IsNullOrWhiteSpace(snapshot.MapName))
        {
            SetStatus("GET cần client đang đăng nhập và đứng trong map");
            return;
        }
        profile.TrainMapId = snapshot.MapId.Value;
        profile.TrainMapName = snapshot.MapName;
        LoadTrainSettings();
        Save();
        SetStatus($"Đã lấy map {profile.TrainMapId}.{profile.TrainMapName} từ client");
    }

    private static Control AutoSection(string title, string description)
    {
        var page = new Panel { Dock = DockStyle.Fill, BackColor = White, Padding = new Padding(4, 12, 4, 4) };
        var content = new RoundPanel
        {
            Dock = DockStyle.Fill, BackColor = Bg, BorderColor = Line, Radius = 12,
            Padding = new Padding(18, 12, 18, 10)
        };
        content.Controls.Add(new Label
        {
            Text = description, Dock = DockStyle.Top, Height = 24,
            ForeColor = Muted, Font = new Font("Segoe UI", 9F)
        });
        content.Controls.Add(new Label
        {
            Text = title, Dock = DockStyle.Top, Height = 27,
            ForeColor = TextPrimary, Font = new Font("Segoe UI Semibold", 11F)
        });
        page.Controls.Add(content);
        return page;
    }

    private static Control InfoPage(string heading, string description)
    {
        var page = new Panel { Dock = DockStyle.Fill, BackColor = White, Padding = new Padding(18, 20, 18, 12) };
        page.Controls.Add(new Label
        {
            Text = description, Dock = DockStyle.Top, Height = 42,
            ForeColor = Muted, Font = new Font("Segoe UI", 9.5F)
        });
        page.Controls.Add(new Label
        {
            Text = heading, Dock = DockStyle.Top, Height = 32,
            ForeColor = TextPrimary, Font = new Font("Segoe UI Semibold", 12F)
        });
        return page;
    }

    private Control Footer()
    {
        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Bg };
        _status.Text = "Sẵn sàng";
        _status.AutoSize = true;
        _status.ForeColor = Muted;
        _status.Location = new Point(2, 9);
        footer.Controls.Add(_status);
        var version = new Label
        {
            Text = "NSOCry Pro  •  v0.2.0", AutoSize = true,
            ForeColor = Muted, Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        footer.Controls.Add(version);
        footer.Resize += (_, _) => version.Location = new Point(footer.ClientSize.Width - version.Width, 9);
        return footer;
    }

    private static Button Button(string caption, Action action, ButtonStyle style)
    {
        var button = new Button
        {
            Text = caption, AutoSize = true, Height = 34, Padding = new Padding(13, 0, 13, 0),
            Margin = new Padding(0, 0, 8, 0), FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9F), Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        if (style == ButtonStyle.Primary)
        {
            button.BackColor = Blue; button.ForeColor = White;
            button.FlatAppearance.BorderColor = Blue;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        }
        else if (style == ButtonStyle.Danger)
        {
            button.BackColor = RedSoft; button.ForeColor = Red;
            button.FlatAppearance.BorderColor = Color.FromArgb(254, 205, 211);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 228, 230);
        }
        else
        {
            button.BackColor = White; button.ForeColor = TextPrimary;
            button.FlatAppearance.BorderColor = Line;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        }
        button.Click += (_, _) => action();
        return button;
    }

    private static Control Separator() => new Panel
    {
        Width = 1, Height = 28, BackColor = Line, Margin = new Padding(4, 2, 12, 0)
    };

    private static Label MetricValue() => new()
    {
        Text = "0", AutoSize = true, ForeColor = TextPrimary,
        Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold)
    };

    private static DataGridViewTextBoxColumn TextColumn(string property, string title, int width, bool readOnly = false) =>
        new() { DataPropertyName = property, Name = property, HeaderText = title, Width = width, ReadOnly = readOnly };

    private static DataGridViewCheckBoxColumn CheckColumn(string property, string title, int width) =>
        new() { DataPropertyName = property, Name = property, HeaderText = title, Width = width, FlatStyle = FlatStyle.Flat };

    private IEnumerable<ClientProfile> SelectedProfiles() => _profiles.Where(profile => profile.Selected);

    private void AddProfile()
    {
        var profile = new ClientProfile();
        using var dialog = new ProfileDialog(profile);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _profiles.Add(profile);
        Save();
        SetStatus("Đã thêm hồ sơ mới");
    }

    private void EditSelected()
    {
        var profile = _grid.CurrentRow?.DataBoundItem as ClientProfile;
        if (profile is null)
        {
            SetStatus("Hãy chọn một hồ sơ để chỉnh sửa");
            return;
        }
        using var dialog = new ProfileDialog(profile);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _grid.Refresh();
        Save();
        SetStatus("Đã cập nhật hồ sơ");
    }

    private void DeleteSelected()
    {
        foreach (var profile in SelectedProfiles().ToArray())
        {
            _processManager.Stop(profile.Id);
            _profiles.Remove(profile);
        }
        Save();
        SetStatus("Đã xóa hồ sơ được chọn");
    }

    private void StartSelected() => RunFor(SelectedProfiles(), StartProfile, "Đã mở client được chọn");
    private void StartAll() => RunFor(_profiles, StartProfile, "Đã mở tất cả client");
    private void RestartSelected() => RunFor(SelectedProfiles(), _processManager.Restart, "Đã khởi động lại client");

    private void StopAll()
    {
        foreach (var profile in _profiles) _processManager.Stop(profile.Id);
        RefreshStatus();
        SetStatus("Đã dừng tất cả client");
    }

    private void RunFor(IEnumerable<ClientProfile> profiles, Action<ClientProfile> action, string message)
    {
        try
        {
            foreach (var profile in profiles) action(profile);
            Save();
            RefreshStatus();
            SetStatus(message);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "NSOCry Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            SetStatus("Có lỗi xảy ra");
        }
    }

    private void Save()
    {
        _grid.EndEdit();
        _store.Save(_profiles);
    }

    private void StartProfile(ClientProfile profile) => _processManager.Start(profile);

    private void RefreshStatus()
    {
        var running = 0;
        long memory = 0;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is not ClientProfile profile) continue;
            var process = _processManager.GetProcess(profile.Id);
            var active = process is not null;
            if (active) { running++; memory += process!.WorkingSet64; }
            SetCellValue(row.Cells["Status"], _processManager.GetStateLabel(profile.Id));
            var snapshot = _processManager.GetSnapshot(profile.Id);
            row.Cells["Status"].ToolTipText = snapshot is null ? "Đang chờ dữ liệu trực tiếp từ giả lập."
                : $"{_processManager.GetStateLabel(profile.Id)}\nTự đăng nhập: {snapshot.Automation}\nLớp màn hình: {snapshot.Screen}\nNhân vật: {snapshot.Characters.Replace('\n', ',')}\nTrạng thái màn hình không thay thế xác nhận kết nối từ server.";
            SetCellValue(row.Cells["Pid"], active ? process!.Id : "-");
            SetCellValue(row.Cells["Ram"], active ? $"{process!.WorkingSet64 / 1024 / 1024} MB" : "-");
        }
        _total.Text = _profiles.Count.ToString();
        _running.Text = running.ToString();
        _offline.Text = (_profiles.Count - running).ToString();
        _memory.Text = $"{memory / 1024 / 1024} MB";
    }

    private void SetStatus(string message) => _status.Text = $"{DateTime.Now:HH:mm:ss}  •  {message}";

    private static void SetCellValue(DataGridViewCell cell, object value)
    {
        if (!Equals(cell.Value, value)) cell.Value = value;
    }



    private enum ButtonStyle { Primary, Normal, Danger }

    private sealed class ModernTabs : Panel
    {
        private readonly FlowLayoutPanel _header;
        private readonly Panel _content;
        private readonly List<(Button Button, Panel Accent, Control Page)> _pages = [];
        private readonly int _headerHeight;
        private readonly int _buttonWidth;
        private readonly float _fontSize;
        private int _selectedIndex = -1;

        public ModernTabs(int headerHeight, int buttonWidth, float fontSize)
        {
            Dock = DockStyle.Fill;
            BackColor = White;
            _headerHeight = headerHeight;
            _buttonWidth = buttonWidth;
            _fontSize = fontSize;

            _content = new Panel { Dock = DockStyle.Fill, BackColor = White };
            var headerBorder = new Panel
            {
                Dock = DockStyle.Top, Height = headerHeight, BackColor = White,
                Padding = new Padding(0, 0, 0, 1)
            };
            _header = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, BackColor = White, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0), Margin = new Padding(0)
            };
            headerBorder.Controls.Add(_header);
            headerBorder.Paint += (_, e) =>
            {
                using var line = new Pen(Line);
                e.Graphics.DrawLine(line, 0, headerBorder.ClientSize.Height - 1,
                    headerBorder.ClientSize.Width, headerBorder.ClientSize.Height - 1);
            };
            Controls.Add(_content);
            Controls.Add(headerBorder);
        }

        public void AddPage(string title, Control page)
        {
            var index = _pages.Count;
            var button = new Button
            {
                Text = title, Width = _buttonWidth, Height = _headerHeight - 1,
                Margin = new Padding(0, 0, 6, 0), Padding = new Padding(8, 0, 8, 2),
                FlatStyle = FlatStyle.Flat, BackColor = White, ForeColor = Muted,
                Font = new Font("Segoe UI Semibold", _fontSize), Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter, UseVisualStyleBackColor = false,
                AutoEllipsis = true
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = BlueSoft;
            button.FlatAppearance.MouseDownBackColor = BlueSoft;
            var accent = new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = Blue, Visible = false };
            button.Controls.Add(accent);
            button.Click += (_, _) => SelectPage(index);

            page.Dock = DockStyle.Fill;
            page.Visible = false;
            _content.Controls.Add(page);
            _header.Controls.Add(button);
            _pages.Add((button, accent, page));
            if (_selectedIndex < 0) SelectPage(0);
        }

        private void SelectPage(int index)
        {
            if (index < 0 || index >= _pages.Count) return;
            _selectedIndex = index;
            for (var i = 0; i < _pages.Count; i++)
            {
                var selected = i == index;
                var item = _pages[i];
                item.Button.BackColor = selected ? BlueSoft : White;
                item.Button.ForeColor = selected ? Blue : Muted;
                item.Accent.Visible = selected;
                item.Page.Visible = selected;
                if (selected) item.Page.BringToFront();
            }
        }
    }

    private sealed class BufferedGrid : DataGridView
    {
        public BufferedGrid() => DoubleBuffered = true;
    }

    private sealed class ModernCheckBox : CheckBox
    {
        public ModernCheckBox()
        {
            AutoSize = false;
            Font = new Font("Segoe UI Semibold", 9F);
            ForeColor = TextPrimary;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Bg);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float scale = DeviceDpi / 96f;
            int size = Math.Max(16, (int)Math.Round(18 * scale));
            var box = new Rectangle(1, (Height - size) / 2, size, size);
            using var shape = RoundedRectangle(box, Math.Max(4, (int)Math.Round(5 * scale)));
            using var fill = new SolidBrush(Checked ? Blue : White);
            using var border = new Pen(Checked ? Blue : Color.FromArgb(203, 213, 225), scale);
            e.Graphics.FillPath(fill, shape);
            e.Graphics.DrawPath(border, shape);
            if (Checked)
            {
                using var tick = new Pen(White, 2 * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                e.Graphics.DrawLines(tick, new PointF[]
                {
                    new(box.X + 4 * scale, box.Y + 9 * scale),
                    new(box.X + 8 * scale, box.Y + 13 * scale),
                    new(box.X + 14 * scale, box.Y + 5 * scale)
                });
            }
            var textBounds = new Rectangle(box.Right + (int)Math.Round(9 * scale), 0,
                Math.Max(0, Width - box.Right - (int)Math.Round(9 * scale)), Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, Enabled ? ForeColor : Muted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            Invalidate();
        }
    }

    private sealed class ModernRadioButton : RadioButton
    {
        public ModernRadioButton()
        {
            AutoSize = false;
            Font = new Font("Segoe UI Semibold", 9F);
            ForeColor = TextPrimary;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            Margin = new Padding(0, 0, 10, 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Bg);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float scale = DeviceDpi / 96f;
            int size = Math.Max(15, (int)Math.Round(17 * scale));
            var circle = new Rectangle(1, (Height - size) / 2, size, size);
            using var fill = new SolidBrush(White);
            using var border = new Pen(Checked ? Blue : Color.FromArgb(148, 163, 184), scale);
            e.Graphics.FillEllipse(fill, circle);
            e.Graphics.DrawEllipse(border, circle);
            if (Checked)
            {
                int dot = Math.Max(7, (int)Math.Round(9 * scale));
                var center = new Rectangle(circle.X + (circle.Width - dot) / 2, circle.Y + (circle.Height - dot) / 2, dot, dot);
                using var selected = new SolidBrush(Blue);
                e.Graphics.FillEllipse(selected, center);
            }
            var textBounds = new Rectangle(circle.Right + (int)Math.Round(8 * scale), 0,
                Math.Max(0, Width - circle.Right - (int)Math.Round(8 * scale)), Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, Enabled ? ForeColor : Muted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            Invalidate();
        }
    }

    private sealed class RoundPanel : Panel
    {
        public int Radius { get; set; } = 12;
        public Color BorderColor { get; set; } = Color.Transparent;

        public RoundPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var shape = Rounded(ClientRectangle, Radius);
            Region = new Region(shape);
            if (BorderColor == Color.Transparent) return;
            using var pen = new Pen(BorderColor);
            using var border = Rounded(Rectangle.Inflate(ClientRectangle, -1, -1), Math.Max(1, Radius - 1));
            e.Graphics.DrawPath(pen, border);
        }

        private static GraphicsPath Rounded(Rectangle rect, int radius)
        {
            var d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            var path = new GraphicsPath();
            if (d <= 1) { path.AddRectangle(rect); return path; }
            var arc = new Rectangle(rect.X, rect.Y, d, d);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d; path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d; path.AddArc(arc, 0, 90);
            arc.X = rect.Left; path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
