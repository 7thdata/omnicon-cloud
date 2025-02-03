using apiCms.Services;
using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using clsCMs.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.Configure<AppConfigModel>(builder.Configuration.GetSection("AppSettings"));


// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<UserModel, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Log authentication failure events
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log the exception message for debugging
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");

                // Optionally log additional details
                if (context.Exception is SecurityTokenExpiredException)
                {
                    Console.WriteLine("Token has expired.");
                }

                return Task.CompletedTask;
            }
        };
    });

// Add services to the container.
builder.Services.AddScoped<IArticleServices, ArticleServices>();
builder.Services.AddScoped<ISearchServices,SearchServices>();
builder.Services.AddScoped<IChannelServices, ChannelServices>();
builder.Services.AddScoped<IAuthorServices, AuthorServices>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IBlobStorageServices, BlobStorageServices>();
builder.Services.AddScoped<INotificationServices, NotificationServices>();
builder.Services.AddScoped<ITokenService, TokenService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OmniCon.cloud CMS Engine API",
        Version = "v1",
        Description = @"
OmniCon.cloud is an open-source CMS engine designed to support a wide range of content services.

### Key Features:
- **Content Management**: Manage simple text content like blogs or create searchable property listings such as shops, hospitals, etc.
- **E-Commerce Integration**: Extend capabilities to support e-commerce features.
- **All-in-One Solution**: Power your internet services with a unified platform for all your content management needs.

### Open-Source Benefits:
- OmniCon.cloud is completely open source, allowing you to deploy it on your own infrastructure if desired.
- Customize and extend the platform to meet your unique requirements.

### Deployment Options:
- Use our hosted services for convenience and support.
- Self-host on your own infrastructure for full control.

Visit the [OmniCon.cloud GitHub repository](https://github.com/your-repo-link) for source code, installation instructions, and contribution guidelines.
",
        Contact = new OpenApiContact
        {
            Name = "OmniCon.cloud Support",
            Email = "support@omnicon.cloud",
            Url = new Uri("https://omnicon.cloud/contact")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Add JWT Authentication scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OmniCon.cloud CMS Engine API");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}
else
{
    // Optional: Redirect root requests to Swagger UI in production as well
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OmniCon.cloud CMS Engine API");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}

app.UseHttpsRedirection();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
