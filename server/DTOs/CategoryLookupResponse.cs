namespace server.DTOs;

public sealed record CategoryLookupResponse(
  uint category_id,
  string category_name
);
