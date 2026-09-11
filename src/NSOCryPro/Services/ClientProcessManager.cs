using System.Diagnostics;
using NSOCryPro.Models;

namespace NSOCryPro.Services;

public sealed class ClientProcessManager : IDisposable
{
    private readonly string _runtimeDirectory;
    private readonly Dictionary<Guid, Process> _processes = [];

    public ClientProcessManager(string runtimeDirectory) => _runtimeDirectory = runtimeDirectory;

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

        var classPath = $"{emulatorJar};{gameJar}";
        var startInfo = new ProcessStartInfo
        {
            FileName = "javaw.exe",
            Arguments = $"-Duser.home=\"{profileHome}\" -cp \"{classPath}\" org.microemu.app.Main GameMidlet",
            WorkingDirectory = _runtimeDirectory,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Không thể mở client.");
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => _processes.Remove(profile.Id);
        _processes[profile.Id] = process;
    }

    public void Stop(Guid id)
    {
        if (!TryGetRunningProcess(id, out var process)) return;
        process.CloseMainWindow();
        if (!process.WaitForExit(1500)) process.Kill(true);
        process.Dispose();
        _processes.Remove(id);
    }

    public void Restart(ClientProfile profile)
    {
        Stop(profile.Id);
        Start(profile);
    }

    private bool TryGetRunningProcess(Guid id, out Process process)
    {
        if (_processes.TryGetValue(id, out process!) && !process.HasExited) return true;
        _processes.Remove(id);
        process = null!;
        return false;
    }

    public void Dispose()
    {
        foreach (var id in _processes.Keys.ToArray()) Stop(id);
    }
}

