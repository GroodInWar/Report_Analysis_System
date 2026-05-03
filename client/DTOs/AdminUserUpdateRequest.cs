namespace client.DTOs;

public sealed class AdminUserUpdateRequest
{
    public string first_name { get; set; } = string.Empty;
    public string last_name { get; set; } = string.Empty;
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public uint role_id { get; set; }
}
