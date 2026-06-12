using Microsoft.AspNetCore.StaticFiles;
namespace BBStats.Configuration;
public static class StaticFileCacheHeaders
{
    private static readonly string[] ImageExtensions =
        [".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".ico"];
    private const string LongTermImageCacheControl = "public,max-age=31536000,immutable";
    public static void ApplyLongTermImageCache(StaticFileResponseContext context)
    {
        var path = context.Context.Request.Path.Value;
        if (path is null)
        {
            return;
        }
        if (!ImageExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        context.Context.Response.Headers.CacheControl = LongTermImageCacheControl;
    }
}