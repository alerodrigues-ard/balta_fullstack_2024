using System;

namespace Fina.Api;

public class ApiConfiguration
{
    public const string UserId = "ale_ard@outlook.com";
    public static string ConnectionString { get; set; } = string.Empty;
    public static string CorsPolicyName = "wasm";   // O nome é indiferente
}
