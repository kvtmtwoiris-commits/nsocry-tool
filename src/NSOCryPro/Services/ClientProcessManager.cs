using System.Diagnostics;
using System.Security.Cryptography;
using NSOCryPro.Models;

namespace NSOCryPro.Services;

public sealed class ClientProcessManager : IDisposable
{
    private const int GameWidth = 500;
    private const int GameHeight = 300;
    private const string ResizableDevice = "org/microemu/device/resizable/device.xml";
    private readonly string _runtimeDirectory;
    private readonly Dictionary<Guid, Process> _processes = [];
    private readonly Dictionary<Guid, ClientBridgeSession> _bridges = [];
    private const string SupportedClient = "A6DC5C4A6F5314DDD9D8C8F0E03A9077DC3668775533E658562EDEB9ABE4A3AE";

    public ClientProcessManager(string runtimeDirectory) => _runtimeDirectory = Path.GetFullPath(runtimeDirectory);

    public bool IsRunning(Guid id) => TryGetRunningProcess(id, out _);

    public Process? GetProcess(Guid id) => TryGetRunningProcess(id, out var process) ? process : null;

    public void Start(ClientProfile profile)
    {
        if (IsRunning(profile.Id)) return;

        var emulatorJar = Path.Combine(_runtimeDirectory, "microemulator.jar");
        var gameJar = Path.Combine(_runtimeDirectory, "game.jar");
        if (!File.Exists(emulatorJar) || !File.Exists(gameJar))
            throw new FileNotFoundException("Hãy đặt microemulator.jar và game.jar vào thư mục runtime.");

        var profileHome = Path.Combine(_runtimeDirectory, "profiles", profile.Id.ToString("N"));
        Directory.CreateDirectory(profileHome);

        var agentJar = ClientBridgeSession.InstallAgent(_runtimeDirectory);
        using var gameFile = File.OpenRead(gameJar);
        bool supported = Convert.ToHexString(SHA256.HashData(gameFile)) == SupportedClient;
        var password = profile.AutoLogin ? CredentialProtector.Unprotect(profile.EncryptedPassword) : null;
        var bridge = new ClientBridgeSession(profile.AutoLogin ? profile.Account : null, password, profile.CharacterName);
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "javaw.exe",
                WorkingDirectory = _runtimeDirectory,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            foreach (var argument in new[] {
                $"-Duser.home={profileHome}", $"-javaagent:{agentJar}", "-cp", $"{emulatorJar};{gameJar}",
                "org.microemu.app.Main", "--device", ResizableDevice, "--resizableDevice",
                GameWidth.ToString(), GameHeight.ToString(), "GameMidlet" })
                startInfo.ArgumentList.Add(argument);
            startInfo.Environment["NSOCRY_BRIDGE_PORT"] = bridge.Port.ToString();
            startInfo.Environment["NSOCRY_BRIDGE_TOKEN"] = bridge.Token;
            startInfo.Environment["NSOCRY_BRIDGE_SUPPORTED"] = supported ? "1" : "0";
            var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Không thể mở client.");
            _processes[profile.Id] = process;
            _bridges[profile.Id] = bridge;
        }
        catch
        {
            bridge.Dispose();
            throw;
        }
    }

    public ClientBridgeSnapshot? GetSnapshot(Guid id) => _bridges.TryGetValue(id, out var bridge) ? bridge.Snapshot : null;

    public string GetStateLabel(Guid id)
    {
        if (!IsRunning(id)) return "Đã dừng";
        var snapshot = GetSnapshot(id);
        if (snapshot is null) return "Chờ cầu nối";
        return DateTimeOffset.UtcNow - snapshot.ReceivedAt > TimeSpan.FromSeconds(5)
            ? "Cầu nối không phản hồi" : snapshot.Label;
    }

    public void Stop(Guid id)
    {
        if (!TryGetRunningProcess(id, out var process)) return;
        process.CloseMainWindow();
        if (!process.WaitForExit(1500)) process.Kill(true);
        process.Dispose();
        _processes.Remove(id);
        if (_bridges.Remove(id, out var bridge)) bridge.Dispose();
    }

    public void Restart(ClientProfile profile)
    {
        Stop(profile.Id);
        Start(profile);
    }

    private bool TryGetRunningProcess(Guid id, out Process process)
    {
        if (_processes.TryGetValue(id, out process!) && !process.HasExited) return true;
        if (_processes.Remove(id, out var exited)) exited.Dispose();
        if (_bridges.Remove(id, out var bridge)) bridge.Dispose();
        process = null!;
        return false;
    }

    public void Dispose()
    {
        foreach (var id in _processes.Keys.ToArray()) Stop(id);
    }
}
