using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace BBStats.Pages;

public static class PaginationQuery
{
	public const string Key = "page";

	public static int ReadPageNumber(HttpRequest request, int defaultPage = 1)
	{
		if (request.Query.TryGetValue(Key, out var values)
		    && int.TryParse(values.FirstOrDefault(), out var page)
		    && page >= 1)
		{
			return page;
		}

		return defaultPage;
	}
}

public static class PageQueryUrl
{
	public static string? WithQueryPage(
		IUrlHelper url,
		string pagePath,
		int pageNumber,
		object? routeValues = null)
	{
		var href = routeValues is null
			? url.Page(pagePath)
			: url.Page(pagePath, routeValues);

		if (string.IsNullOrEmpty(href) || pageNumber <= 1)
		{
			return href;
		}

		return QueryHelpers.AddQueryString(href, PaginationQuery.Key, pageNumber.ToString());
	}
}
