using Shared.Models;

namespace client.DTOs;

public class ReportDetail : Report
{
    public List<FileSummary> Files { get; set; } = [];
}
