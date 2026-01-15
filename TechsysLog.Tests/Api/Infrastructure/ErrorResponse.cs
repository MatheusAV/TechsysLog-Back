using System.Text.Json.Serialization;

namespace TechsysLog.Tests.Api.Infrastructure;

public sealed class ErrorResponse
{
    [JsonPropertyName("error")]
    public ErrorBody Error { get; set; } = default!;

    public sealed class ErrorBody
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }
}


