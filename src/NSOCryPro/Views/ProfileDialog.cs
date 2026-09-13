using NSOCryPro.Models;
using NSOCryPro.Services;

namespace NSOCryPro.Views;

public sealed class ProfileDialog : Form
{
    private readonly TextBox _account = Field();
    private readonly TextBox _password = Field();
    private readonly TextBox _character = Field();
    private readonly TextBox _server = Field();
    private readonly CheckBox _autoLogin = new();
    private readonly CheckBox _autoRestart = new();

    public ProfileDialog(ClientProfile profile)
    {
        Text = profile.Account.Length == 0 ? "Thêm hồ sơ" : "Chỉnh sửa hồ sơ";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(440, 480);
        BackColor = Color.FromArgb(244, 247, 251);
        Font = new Font("Segoe UI", 9.5F);

        _account.Text = profile.Account;
        _password.Text = CredentialProtector.Unprotect(profile.EncryptedPassword);
        _password.UseSystemPasswordChar = true;
        _character.Text = profile.CharacterName;
        _server.Text = profile.Server;
        _autoLogin.Text = "Tự động đăng nhập và chọn nhân vật";
        _autoLogin.Checked = profile.AutoLogin;
        _autoLogin.AutoSize = true;
        _autoRestart.Text = "Tự khởi động lại khi client bị đóng";
        _autoRestart.Checked = profile.AutoRestart;
        _autoRestart.AutoSize = true;

        var title = new Label
        {
            Text = "Thông tin client",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 16F),
            ForeColor = Color.FromArgb(17, 32, 61),
            Location = new Point(24, 20)
        };
        var form = new TableLayoutPanel
        {
            Location = new Point(24, 70), Size = new Size(392, 292),
            ColumnCount = 1, RowCount = 8
        };
        form.RowStyles.Clear();
        for (var i = 0; i < 8; i++)
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 25 : 48));
        AddField(form, 0, "Tài khoản", _account);
        AddField(form, 2, "Mật khẩu", _password);
        AddField(form, 4, "Tên nhân vật", _character);
        AddField(form, 6, "Server", _server);

        _autoLogin.Location = new Point(27, 370);
        _autoRestart.Location = new Point(27, 399);

        var save = DialogButton("Lưu hồ sơ", true);
        save.Location = new Point(294, 434);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_account.Text))
            {
                MessageBox.Show("Hãy nhập tài khoản.", "NSOCry Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            profile.Account = _account.Text.Trim();
            profile.EncryptedPassword = CredentialProtector.Protect(_password.Text);
            profile.CharacterName = _character.Text.Trim();
            profile.Server = string.IsNullOrWhiteSpace(_server.Text) ? "Bokken" : _server.Text.Trim();
            profile.AutoLogin = _autoLogin.Checked;
            profile.AutoRestart = _autoRestart.Checked;
            DialogResult = DialogResult.OK;
            Close();
        };
        var cancel = DialogButton("Hủy", false);
        cancel.Location = new Point(202, 434);
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange([title, form, _autoLogin, _autoRestart, cancel, save]);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static TextBox Field() => new()
    {
        Dock = DockStyle.Fill,
        BorderStyle = BorderStyle.FixedSingle,
        Font = new Font("Segoe UI", 10F),
        Margin = new Padding(0, 0, 0, 10)
    };

    private static void AddField(TableLayoutPanel form, int row, string label, Control field)
    {
        form.Controls.Add(new Label
        {
            Text = label, Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI Semibold", 9F)
        }, 0, row);
        form.Controls.Add(field, 0, row + 1);
    }

    private static Button DialogButton(string text, bool primary)
    {
        var button = new Button
        {
            Text = text, Size = new Size(primary ? 122 : 82, 34),
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9F),
            BackColor = primary ? Color.FromArgb(37, 99, 235) : Color.White,
            ForeColor = primary ? Color.White : Color.FromArgb(15, 23, 42)
        };
        button.FlatAppearance.BorderColor = primary
            ? Color.FromArgb(37, 99, 235)
            : Color.FromArgb(203, 213, 225);
        return button;
    }
}
