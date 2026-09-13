using System.Net.Sockets;
using System.Text;
using NSOCryPro.Services;

static async Task WaitFor(Func<bool> condition)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
    while (!condition()) await Task.Delay(20, timeout.Token);
}

static async Task<TcpClient> Connect(ClientBridgeSession session, string token)
{
    var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", session.Port);
    await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes($"HELLO\t1\t{token}\n"));
    return client;
}

static async Task SendState(ClientBridgeSession session, string phase, string character, int mapId, string mapName)
{
    using var client = await Connect(session, session.Token);
    using var reader = new StreamReader(client.GetStream());
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(8));
    if (await reader.ReadLineAsync(deadline.Token) != "OK") throw new Exception("Handshake failed");
    if (await reader.ReadLineAsync(deadline.Token) != "POLL") throw new Exception("No poll");
    var line = $"STATE\t{phase}\tY0g=\t{Convert.ToBase64String(Encoding.UTF8.GetBytes(character))}\tTE9HSU5fU0VOVA==\t{mapId}\t{Convert.ToBase64String(Encoding.UTF8.GetBytes(mapName))}\n";
    await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(line));
    await WaitFor(() => session.Snapshot?.Phase == phase);
    if (session.Snapshot?.Characters != character) throw new Exception("Account data mixed");
    if (session.Snapshot?.Automation != "LOGIN_SENT") throw new Exception("Automation state lost");
    if (session.Snapshot?.MapId != mapId || session.Snapshot?.MapName != mapName) throw new Exception("Map data mixed");
}

using var first = new ClientBridgeSession();
using var second = new ClientBridgeSession();
if (first.Token == second.Token || first.Port == second.Port) throw new Exception("Shared launch identity");
using (var wrong = await Connect(first, second.Token))
{
    using var reader = new StreamReader(wrong.GetStream());
    using var timeout = new CancellationTokenSource(5000);
    if (await reader.ReadLineAsync(timeout.Token) is not null) throw new Exception("Wrong token accepted");
}
await Task.WhenAll(SendState(first, "GAME_SCREEN", "nhân vật A", 1, "Trường Hirosaki"),
    SendState(second, "GAME_SCREEN", "nhân vật B", 22, "Làng Tone"));
await WaitFor(() => first.Snapshot?.Phase == "DISCONNECTED" && second.Snapshot?.Phase == "DISCONNECTED");

using var oversized = new ClientBridgeSession();
using (var client = await Connect(oversized, oversized.Token))
{
    using var reader = new StreamReader(client.GetStream());
    using var timeout = new CancellationTokenSource(5000);
    await reader.ReadLineAsync(timeout.Token);
    await reader.ReadLineAsync(timeout.Token);
    await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(new string('X', 9000) + "\n"));
    await WaitFor(() => oversized.Snapshot?.Phase == "DISCONNECTED");
}

var runtime = Path.Combine(Path.GetTempPath(), "nsocry-bridge-test-" + Guid.NewGuid().ToString("N"));
try
{
    var jar = ClientBridgeSession.InstallAgent(runtime);
    if (!File.Exists(jar) || ClientBridgeSession.InstallAgent(runtime) != jar) throw new Exception("Agent extraction failed");
}
finally { if (Directory.Exists(runtime)) Directory.Delete(runtime, true); }
Console.WriteLine("Bridge tests passed: per-client map isolation, authentication, Unicode, disconnect, size limit, embedded agent.");
