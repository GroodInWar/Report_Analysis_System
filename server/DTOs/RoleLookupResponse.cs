namespace server.DTOs;

public sealed record RoleLookupResponse(
  uint role_id,
  string role_name
);