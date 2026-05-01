namespace client.DTOs;

public class UserSummary
{
    public uint user_id { get; set; }
    public string first_name { get; set; } = "";
    public string last_name { get; set; } = "";
    public string username { get; set; } = "";
    public string email { get; set; } = "";
}
