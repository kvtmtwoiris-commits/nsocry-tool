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
    private static readonly Color Text = Color.FromArgb(15, 23, 42);
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
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1500 };

    public MainForm(JsonProfileStore store, ClientProcessManager processManager)
    {
        _store = store;
        _processManager = processManager;
        _profiles = new BindingList<ClientProfile>(_store.Load());

        Text = "NSOCry Pro";
        BackColor = Bg;
        ForeColor = Text;
        Font = new Font("Segoe UI", 9.5F);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1120, 700);
        Size = new Size(1320, 820);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildUi();
        _timer.Tick += (_, _) => RefreshStatus();
        _timer.Start();
        FormClosing += (_, _) => { Save(); _processManager.Dispose(); };
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 166));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        root.Controls.Add(Header(), 0, 0);
        root.Controls.Add(Metrics(), 0, 1);
        root.Controls.Add(ActionBar(), 0, 2);
        root.Controls.Add(GridCard(), 0, 3);
        root.Controls.Add(SettingsCard(), 0, 4);
        root.Controls.Add(Footer(), 0, 5);
        Controls.Add(root);
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
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = true;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 44;
        _grid.RowTemplate.Height = 44;
        _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(248, 250, 252), ForeColor = Muted,
            SelectionBackColor = Color.FromArgb(248, 250, 252),
            Font = new Font("Segoe UI Semibold", 8.5F), Padding = new Padding(8, 0, 8, 0)
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = White, ForeColor = Text, SelectionBackColor = BlueSoft,
            SelectionForeColor = Text, Font = new Font("Segoe UI", 9.5F),
            Padding = new Padding(8, 0, 8, 0)
        };
        _grid.DataSource = _profiles;
        _grid.Columns.Add(CheckColumn(nameof(ClientProfile.Selected), "", 46));
        _grid.Columns.Add(TextColumn(nameof(ClientProfile.CharacterName), "NHÂN VẬT", 190));
        _grid.Columns.Add(TextColumn(nameof(ClientProfile.Account), "TÀI KHOẢN", 220));
        _grid.Columns.Add(TextColumn(nameof(ClientProfile.Server), "SERVER", 115));
        _grid.Columns.Add(TextColumn("Status", "TRẠNG THÁI", 130, true));
        _grid.Columns.Add(TextColumn("Pid", "PID", 80, true));
        _grid.Columns.Add(TextColumn("Ram", "RAM", 100, true));
        _grid.Columns.Add(CheckColumn(nameof(ClientProfile.AutoRestart), "TỰ KHỞI ĐỘNG", 120));
        _grid.Columns[^1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Columns[e.ColumnIndex].Name != "Status" || e.Value is not string state) return;
            e.CellStyle.ForeColor = state == "RUNNING" ? Green : Red;
            e.CellStyle.Font = new Font("Segoe UI Semibold", 9F);
        };
    }

    private Control SettingsCard()
    {
        var card = new RoundPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = White,
            BorderColor = Line, Radius = 14
        };
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill, Appearance = TabAppearance.FlatButtons,
            ItemSize = new Size(150, 34), SizeMode = TabSizeMode.Fixed,
            Font = new Font("Segoe UI Semibold", 9F), Padding = new Point(14, 5)
        };
        tabs.TabPages.Add(InfoTab("Tổng quan", "Quản lý tập trung",
            "Chọn hồ sơ trong bảng, sau đó mở, dừng hoặc khởi động lại client bằng thanh thao tác."));
        tabs.TabPages.Add(InfoTab("Cấu hình Auto", "Tự động hóa",
            "Thiết lập train, kỹ năng, nhặt vật phẩm và nhiệm vụ sẽ được tích hợp tại đây."));
        tabs.TabPages.Add(InfoTab("Nhật ký", "Theo dõi hoạt động",
            "Lịch sử thao tác và sự kiện của từng client sẽ được hiển thị tại đây."));
        card.Controls.Add(tabs);
        return card;
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

    private static TabPage InfoTab(string title, string heading, string description)
    {
        var page = new TabPage(title) { BackColor = White, ForeColor = Text };
        page.Controls.Add(new Label
        {
            Text = description, Location = new Point(20, 52), Size = new Size(800, 45),
            ForeColor = Muted, Font = new Font("Segoe UI", 9.5F)
        });
        page.Controls.Add(new Label
        {
            Text = heading, AutoSize = true, Location = new Point(20, 17),
            ForeColor = Text, Font = new Font("Segoe UI Semibold", 12F)
        });
        return page;
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
            button.BackColor = White; button.ForeColor = Text;
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
        Text = "0", AutoSize = true, ForeColor = Text,
        Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold)
    };

    private static DataGridViewTextBoxColumn TextColumn(string property, string title, int width, bool readOnly = false) =>
        new() { DataPropertyName = property, Name = property, HeaderText = title, Width = width, ReadOnly = readOnly };

    private static DataGridViewCheckBoxColumn CheckColumn(string property, string title, int width) =>
        new() { DataPropertyName = property, Name = property, HeaderText = title, Width = width, FlatStyle = FlatStyle.Flat };

    private IEnumerable<ClientProfile> SelectedProfiles() => _profiles.Where(profile => profile.Selected);

    private void AddProfile()
    {
        _profiles.Add(new ClientProfile { Account = $"account{_profiles.Count + 1}" });
        Save();
        SetStatus("Đã thêm hồ sơ mới");
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

    private void StartSelected() => RunFor(SelectedProfiles(), _processManager.Start, "Đã mở client được chọn");
    private void StartAll() => RunFor(_profiles, _processManager.Start, "Đã mở tất cả client");
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
            row.Cells["Status"].Value = active ? "RUNNING" : "OFFLINE";
            row.Cells["Pid"].Value = active ? process!.Id : "-";
            row.Cells["Ram"].Value = active ? $"{process!.WorkingSet64 / 1024 / 1024} MB" : "-";
        }
        _total.Text = _profiles.Count.ToString();
        _running.Text = running.ToString();
        _offline.Text = (_profiles.Count - running).ToString();
        _memory.Text = $"{memory / 1024 / 1024} MB";
    }

    private void SetStatus(string message) => _status.Text = $"{DateTime.Now:HH:mm:ss}  •  {message}";

    private enum ButtonStyle { Primary, Normal, Danger }

    private sealed class BufferedGrid : DataGridView
    {
        public BufferedGrid() => DoubleBuffered = true;
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
