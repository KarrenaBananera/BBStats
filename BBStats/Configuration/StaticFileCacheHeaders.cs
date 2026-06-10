using Microsoft.AspNetCore.StaticFiles;

namespace BBStats.Configuration;

public static class StaticFileCacheHeaders
{
	private static readonly string[] ImageExtensions =
		[".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".ico"];

	public static void ApplyLongTermImageCache(StaticFileResponseContext context)
	{
		var path = context.Context.Request.Path.Value;
		if (path is null || !path.StartsWith("/data/", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		if (!ImageExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
		{
			return;
		}

		context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
	}
}
