namespace client.DTOs;

public sealed class AdminUser
{
    public uint user_id { get; set; }
    public string first_name { get; set; } = string.Empty;
    public string last_name { get; set; } = string.Empty;
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public uint role_id { get; set; }
    public string role { get; set; } = string.Empty;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? last_login_at { get; set; }
}
