namespace NSOCryPro.Models;

public sealed class ClientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool Selected { get; set; } = true;
    public string Account { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string Server { get; set; } = "Bokken";
    public bool AutoLogin { get; set; } = true;
    public bool AutoRestart { get; set; } = true;
}
