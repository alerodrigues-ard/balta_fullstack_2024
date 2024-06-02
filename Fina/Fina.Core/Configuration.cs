using System.Data.Common;
using System.Net;

namespace Fina.Core;

public class Configuration
{
    public const int DefaultStatusCode = 200;   // HTTP Status
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 25;

    public static string BackendUrl { get; set; } = string.Empty;
    public static string FrontendUrl { get; set; } = string.Empty;
}
