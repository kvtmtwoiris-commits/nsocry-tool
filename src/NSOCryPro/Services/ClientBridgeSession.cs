using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace NSOCryPro.Services;

public sealed record ClientBridgeSnapshot(string Phase, string Screen, string Characters, string Automation, DateTimeOffset ReceivedAt)
{
    public string Label => Phase switch
    {
        "STARTING" => "Đang khởi tạo",
        "MENU" => "Menu",
        "ACCOUNT_SCREEN" => "Tài khoản",
        "CHARACTER_SELECT" => "Chọn nhân vật",
        "GAME_SCREEN" => "Màn hình game",
        "DIALOG" => "Có hộp thoại",
        "TRANSITION" => "Chuyển màn",
        "UNSUPPORTED" => "Client chưa hỗ trợ",
        "ADAPTER_ERROR" => "Lỗi đọc client",
        "DISCONNECTED" => "Mất cầu nối",
        _ => "Màn hình khác"
    };
}

/// <summary>One authenticated, loopback-only channel per client launch; credentials are never put on the process command line.</summary>
public sealed class ClientBridgeSession : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _worker;
    private readonly string? _loginCommand;
    private ClientBridgeSnapshot? _snapshot;
    private bool _disposed;
    public string Token { get; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    public int Port { get; }
    public ClientBridgeSnapshot? Snapshot => Volatile.Read(ref _snapshot);

    public ClientBridgeSession(string? account = null, string? password = null, string? character = null)
    {
        if (!string.IsNullOrWhiteSpace(account) && !string.IsNullOrEmpty(password))
            _loginCommand = $"LOGIN\t{Encode(account)}\t{Encode(password)}\t{Encode(character ?? string.Empty)}";
        _listener.Start(4);
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _worker = RunAsync();
    }

    private async Task RunAsync()
    {
        var ct = _stop.Token;
        try
        {
            using var startup = CancellationTokenSource.CreateLinkedTokenSource(ct);
            startup.CancelAfter(TimeSpan.FromSeconds(45));
            while (!startup.IsCancellationRequested)
            {
                using var client = await _listener.AcceptTcpClientAsync(startup.Token);
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, new UTF8Encoding(false), false, 1024, true);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, true) { AutoFlush = true, NewLine = "\n" };
                string? hello;
                try { hello = await ReadLineAsync(reader, startup.Token, 200); }
                catch (IOException) { continue; }
                catch (OperationCanceledException) when (!startup.IsCancellationRequested) { continue; }
                if (hello != $"HELLO\t1\t{Token}") continue;
                _listener.Stop();
                await writer.WriteLineAsync("OK".AsMemory(), ct);
                if (_loginCommand is not null)
                {
                    await writer.WriteLineAsync(_loginCommand.AsMemory(), ct);
                    if (await ReadLineAsync(reader, ct, 32) != "ACK") throw new IOException("Bridge login setup failed.");
                }
                while (!ct.IsCancellationRequested)
                {
                    await writer.WriteLineAsync("POLL".AsMemory(), ct);
                    var line = await ReadLineAsync(reader, ct, 8192);
                    if (line is null) break;
                    var parts = line.Split('\t');
                    if (parts.Length != 5 || parts[0] != "STATE") throw new IOException("Invalid bridge response.");
                    var screen = Encoding.UTF8.GetString(Convert.FromBase64String(parts[2]));
                    var characters = Encoding.UTF8.GetString(Convert.FromBase64String(parts[3]));
                    var automation = Encoding.UTF8.GetString(Convert.FromBase64String(parts[4]));
                    Volatile.Write(ref _snapshot, new(parts[1], screen, characters, automation, DateTimeOffset.UtcNow));
                    await Task.Delay(750, ct);
                }
                break;
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException or ObjectDisposedException or FormatException) { }
        finally
        {
            _listener.Stop();
            Volatile.Write(ref _snapshot, new("DISCONNECTED", "", "", "", DateTimeOffset.UtcNow));
        }
    }

    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static async Task<string?> ReadLineAsync(StreamReader reader, CancellationToken ct, int limit)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(4));
        var result = new StringBuilder();
        var buffer = new char[1];
        for (var i = 0; i <= limit; i++)
        {
            if (await reader.ReadAsync(buffer.AsMemory(), deadline.Token) == 0) return null;
            if (buffer[0] == '\n') return result.ToString().TrimEnd('\r');
            result.Append(buffer[0]);
        }
        throw new IOException("Bridge response too large.");
    }

    public static string InstallAgent(string runtimeDirectory)
    {
        using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("NSOCryPro.BridgeBase64")
            ?? throw new InvalidOperationException("Thiếu cầu nối trong bản build.");
        using var reader = new StreamReader(resource);
        var bytes = Convert.FromBase64String(reader.ReadToEnd());
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        Directory.CreateDirectory(runtimeDirectory);
        var path = Path.Combine(runtimeDirectory, $"nsocry-bridge-{hash[..16]}.jar");
        if (!File.Exists(path) || !SHA256.HashData(File.ReadAllBytes(path)).SequenceEqual(SHA256.HashData(bytes)))
            File.WriteAllBytes(path, bytes);
        return path;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _stop.Cancel();
        _listener.Stop();
        // Dispose the CTS only after outstanding reads finish observing cancellation.
        _ = _worker.ContinueWith(_ => _stop.Dispose(), TaskScheduler.Default);
    }
}
