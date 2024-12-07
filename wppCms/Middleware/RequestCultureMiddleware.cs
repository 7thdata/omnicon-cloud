using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace wppCms.Middleware
{
    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestLocalizationOptions _localizationOptions;

        public RequestCultureMiddleware(RequestDelegate next, IOptions<RequestLocalizationOptions> localizationOptions)
        {
            _next = next;
            _localizationOptions = localizationOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract culture from URL path (e.g., "/en" or "/ja")
            var cultureQuery = context.Request.Path.Value
                .Split('/')
                .FirstOrDefault(c => _localizationOptions.SupportedCultures
                    .Any(supported => supported.Name.Equals(c, StringComparison.OrdinalIgnoreCase)));

            var culture = cultureQuery ?? _localizationOptions.DefaultRequestCulture.Culture.Name;

            // Set culture for the current request
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            await _next(context);
        }
    }
}
