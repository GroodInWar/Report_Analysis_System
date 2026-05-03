namespace client.DTOs;

public class FileSummary
{
    public uint file_id { get; set; }
    public string file_name { get; set; } = "";
    public string file_hash { get; set; } = "";
    public DateTime uploaded_at { get; set; }
}
