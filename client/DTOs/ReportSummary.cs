namespace client.DTOs;

public class ReportSummary
{
    public uint report_id { get; set; }
    public string title { get; set; } = "";
    public string status { get; set; } = "";
    public DateTime submitted_at { get; set; }
}
