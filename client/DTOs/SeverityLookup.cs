namespace client.DTOs;

public class SeverityLookup : ILookupRow
{
    public uint severity_id { get; set; }
    public string severity_name { get; set; } = string.Empty;

    public uint Id => severity_id;

    public string Name
    {
        get => severity_name;
        set => severity_name = value;
    }
}
