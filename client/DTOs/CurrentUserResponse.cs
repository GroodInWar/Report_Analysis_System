using System;

namespace client.DTOs;

public class CurrentUserResponse
{
  public uint user_id { get; set; }

  public string username { get; set; } = string.Empty;

  public string email { get; set; } = string.Empty;

  public string role { get; set; } = string.Empty;
}
