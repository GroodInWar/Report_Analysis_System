namespace client.DTOs;

public class CommentSummary
{
    public uint comment_id { get; set; }
    public uint user_id { get; set; }
    public UserSummary? User { get; set; }
    public string comment_text { get; set; } = "";
    public DateTime created_at { get; set; }
    public DateTime? updated_at { get; set; }
}
