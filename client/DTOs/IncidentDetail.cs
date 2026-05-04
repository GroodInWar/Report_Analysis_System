namespace client.DTOs;

public class IncidentDetail
{
    public uint incident_id { get; set; }
    public uint CreatedByUserId { get; set; }
    public UserSummary? CreatedByUser { get; set; }
    public uint category_id { get; set; }
    public CategoryLookup? Category { get; set; }
    public uint severity_id { get; set; }
    public SeverityLookup? Severity { get; set; }
    public string incident_title { get; set; } = "";
    public string incident_description { get; set; } = "";
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? resolved_at { get; set; }
    public bool? CanEdit { get; set; }
    public bool? CanDelete { get; set; }
    public bool? CanManageFiles { get; set; }
    public bool? IsOwner { get; set; }
    public List<ReportSummary> Reports { get; set; } = [];
    public List<CommentSummary> Comments { get; set; } = [];
    public List<FileSummary> Files { get; set; } = [];
}
