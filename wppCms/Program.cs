using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using clsCMs.Data;
using clsCms.Models;
using clsCms.Services;
using clsCms.Interfaces;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using wppCms.Middleware;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
{
    new CultureInfo("ar"),
    new CultureInfo("de"),
    new CultureInfo("es"),
    new CultureInfo("en"),
    new CultureInfo("fr"),
    new CultureInfo("it"),
    new CultureInfo("ja"),
    new CultureInfo("ko"),
    new CultureInfo("pt"),
    new CultureInfo("ru"),
    new CultureInfo("zh-CN"),
    new CultureInfo("zh-TW")
};
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});


builder.Services.Configure<AppConfigModel>(builder.Configuration.GetSection("AppSettings"));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity configuration
builder.Services.AddDefaultIdentity<UserModel>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews().AddViewLocalization().AddRazorRuntimeCompilation(); ;

builder.Services.AddScoped<IAuthorServices, AuthorServices>();
builder.Services.AddScoped<IArticleServices, ArticleServices>();
builder.Services.AddScoped<IChannelServices, ChannelServices>();
builder.Services.AddScoped<ITickServices, TickServices>();
builder.Services.AddScoped<IAdvertiserServices, AdvertiserServices>();
builder.Services.AddScoped<ISearchServices, SearchServices>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IBlobStorageServices, BlobStorageServices>();
builder.Services.AddScoped<INotificationServices, NotificationServices>();

var app = builder.Build();

// Apply localization early in the pipeline
var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

// Then apply the custom RequestCultureMiddleware
app.UseMiddleware<RequestCultureMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Default HSTS value of 30 days for production.
}

app.UseHttpsRedirection();
app.UseStaticFiles();


// Routing and Authentication
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint Mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
