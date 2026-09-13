namespace NSOCryPro.Models;

public sealed record MapOption(int Id, string Name)
{
    public override string ToString() => $"{Id}.{Name}";
}
