namespace client.DTOs;

public sealed class RoleLookup : ILookupRow
{
    public uint role_id { get; set; }
    public string role_name { get; set; } = string.Empty;

    public uint Id => role_id;

    public string Name
    {
        get => role_name;
        set => role_name = value;
    }
}
