namespace server.DTOs;

public record SeverityLookupResponse(
  uint severity_id,
  string severity_name
);
