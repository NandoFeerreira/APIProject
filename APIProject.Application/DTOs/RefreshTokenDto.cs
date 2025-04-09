﻿using System.Text.Json.Serialization;

namespace APIProject.Application.DTOs
{
    public class RefreshTokenDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
