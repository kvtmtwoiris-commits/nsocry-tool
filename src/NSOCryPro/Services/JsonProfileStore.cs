using System.Text.Json;
using NSOCryPro.Models;

namespace NSOCryPro.Services;

public sealed class JsonProfileStore
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonProfileStore(string path) => _path = path;

    public List<ClientProfile> Load()
    {
        if (!File.Exists(_path)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<ClientProfile>>(File.ReadAllText(_path), _options) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<ClientProfile> profiles)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporaryPath = _path + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(profiles, _options));
        File.Move(temporaryPath, _path, true);
    }
}

