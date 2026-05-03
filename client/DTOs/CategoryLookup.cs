namespace client.DTOs;

public class CategoryLookup : ILookupRow
{
    public uint category_id { get; set; }
    public string category_name { get; set; } = string.Empty;

    public uint Id => category_id;

    public string Name
    {
        get => category_name;
        set => category_name = value;
    }
}
