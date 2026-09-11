using System.ComponentModel;
using System.Diagnostics;
using NSOCryPro.Models;
using NSOCryPro.Services;

namespace NSOCryPro.Views;

public sealed class MainForm : Form
{
    private readonly JsonProfileStore _store;
    private readonly ClientProcessManager _processManager;
    private readonly BindingList<ClientProfile> _profiles;
    private readonly DataGridView _grid = new();
    private readonly Label _summary = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1500 };

    public MainForm(JsonProfileStore store, ClientProcessManager processManager)
    {
        _store = store;
        _processManager = processManager;
        _profiles = new BindingList<ClientProfile>(_store.Load());

        Text = "NSOCry Pro";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 580);
        Size = new Size(1120, 680);

        BuildInterface();
        _timer.Tick += (_, _) => RefreshStatus();
        _timer.Start();
        FormClosing += (_, _) => { Save(); _processManager.Dispose(); };
    }

    private void BuildInterface()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 48, Padding = new Padding(8), WrapContents = false
        };
        toolbar.Controls.AddRange([
            Button("Mở game", StartSelected), Button("Thêm", AddProfile), Button("Xóa", DeleteSelected),
            Button("Cập nhật", Save), Button("Mở tất cả", StartAll), Button("Dừng tất cả", StopAll),
            Button("Restart", RestartSelected)
        ]);
        _summary.AutoSize = true;
        _summary.Margin = new Padding(20, 8, 0, 0);
        toolbar.Controls.Add(_summary);

        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = true;
        _grid.DataSource = _profiles;
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(ClientProfile.Selected), HeaderText = "Chọn", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ClientProfile.CharacterName), HeaderText = "Nhân vật", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ClientProfile.Account), HeaderText = "Tài khoản", Width = 210 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ClientProfile.Server), HeaderText = "Server", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", ReadOnly = true, Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cpu", HeaderText = "CPU", ReadOnly = true, Width = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ram", HeaderText = "RAM", ReadOnly = true, Width = 90 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(ClientProfile.AutoRestart), HeaderText = "Tự chạy lại", Width = 90 });

        var tabs = new TabControl { Dock = DockStyle.Bottom, Height = 190 };
        tabs.TabPages.Add(Tab("Cơ bản", "Quản lý tiến trình client và hồ sơ tài khoản."));
        tabs.TabPages.Add(Tab("Cài đặt Auto", "Khối điều khiển auto sẽ được nối với client ở giai đoạn tiếp theo."));
        tabs.TabPages.Add(Tab("Nhật ký", "Nhật ký chạy client sẽ hiển thị tại đây."));

        Controls.Add(_grid);
        Controls.Add(tabs);
        Controls.Add(toolbar);
        RefreshStatus();
    }

    private static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 30 };
        button.Click += (_, _) => action();
        return button;
    }

    private static TabPage Tab(string title, string content)
    {
        var page = new TabPage(title);
        page.Controls.Add(new Label { Text = content, Dock = DockStyle.Fill, Padding = new Padding(12) });
        return page;
    }

    private IEnumerable<ClientProfile> SelectedProfiles() => _profiles.Where(p => p.Selected);

    private void AddProfile()
    {
        _profiles.Add(new ClientProfile { Account = $"account{_profiles.Count + 1}" });
        Save();
    }

    private void DeleteSelected()
    {
        foreach (var profile in SelectedProfiles().ToArray())
        {
            _processManager.Stop(profile.Id);
            _profiles.Remove(profile);
        }
        Save();
    }

    private void StartSelected() => RunFor(SelectedProfiles(), _processManager.Start);
    private void StartAll() => RunFor(_profiles, _processManager.Start);
    private void RestartSelected() => RunFor(SelectedProfiles(), _processManager.Restart);

    private void StopAll()
    {
        foreach (var profile in _profiles) _processManager.Stop(profile.Id);
        RefreshStatus();
    }

    private void RunFor(IEnumerable<ClientProfile> profiles, Action<ClientProfile> action)
    {
        try
        {
            foreach (var profile in profiles) action(profile);
            Save();
            RefreshStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "NSOCry Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is not ClientProfile profile) continue;
            var process = _processManager.GetProcess(profile.Id);
            var isRunning = process is not null;
            if (isRunning) running++;
            row.Cells["Status"].Value = isRunning ? "RUNNING" : "OFFLINE";
            row.Cells["Ram"].Value = isRunning ? $"{process!.WorkingSet64 / 1024 / 1024} MB" : "-";
            row.Cells["Cpu"].Value = isRunning ? "ON" : "-";
        }
        _summary.Text = $"Client: {_profiles.Count} | Đang chạy: {running} | Offline: {_profiles.Count - running}";
    }
}

