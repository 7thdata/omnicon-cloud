using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clsCms.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdCampaignPerformanceDaily",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    CPM = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    CPC = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCampaignId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdGroupId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCampaignPerformanceDaily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdCampaignPerformanceHourly",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    CPM = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    CPC = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCampaignId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdGroupId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCampaignPerformanceHourly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdCampaignPerformanceMonthly",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    CPM = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    CPC = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCampaignId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdGroupId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCampaignPerformanceMonthly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdClicks",
                columns: table => new
                {
                    ClickId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdCampaignId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdGroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Os = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceBrand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdClicks", x => x.ClickId);
                });

            migrationBuilder.CreateTable(
                name: "AdCreativePerformanceDaily",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    CPM = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    CPC = table.Column<int>(type: "int", nullable: false),
                    AdGreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCreativePerformanceDaily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdCreativePerformanceHourly",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    CPM = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    CPC = table.Column<int>(type: "int", nullable: false),
                    AdGreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCreativePerformanceHourly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdCreativePerformanceMonthly",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    CPM = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    CPC = table.Column<int>(type: "int", nullable: false),
                    AdGreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCreativePerformanceMonthly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdImpressions",
                columns: table => new
                {
                    ImpressionId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdCampaignId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdGroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Os = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceBrand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    CPM = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdImpressions", x => x.ImpressionId);
                });

            migrationBuilder.CreateTable(
                name: "ArticleImpressions",
                columns: table => new
                {
                    ImpressionId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FolderId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImpressionTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Os = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleImpressions", x => x.ImpressionId);
                });

            migrationBuilder.CreateTable(
                name: "ArticleImpressionsDaily",
                columns: table => new
                {
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    TotalImpressions = table.Column<int>(type: "int", nullable: false),
                    UniqueUsers = table.Column<int>(type: "int", nullable: false),
                    AverageImpressionDuration = table.Column<double>(type: "float", nullable: false),
                    TopReferrer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopCity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleImpressionsDaily", x => new { x.Tick, x.OrganizationId, x.ChannelId, x.ArticleId });
                });

            migrationBuilder.CreateTable(
                name: "ArticleImpressionsHourly",
                columns: table => new
                {
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    TotalImpressions = table.Column<int>(type: "int", nullable: false),
                    UniqueUsers = table.Column<int>(type: "int", nullable: false),
                    AverageImpressionDuration = table.Column<double>(type: "float", nullable: false),
                    TopReferrer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopCity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleImpressionsHourly", x => new { x.Tick, x.OrganizationId, x.ChannelId, x.ArticleId });
                });

            migrationBuilder.CreateTable(
                name: "ArticleImpressionsMonthly",
                columns: table => new
                {
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    TotalImpressions = table.Column<int>(type: "int", nullable: false),
                    UniqueUsers = table.Column<int>(type: "int", nullable: false),
                    AverageImpressionDuration = table.Column<double>(type: "float", nullable: false),
                    TopReferrer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopCity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleImpressionsMonthly", x => new { x.Tick, x.OrganizationId, x.ChannelId, x.ArticleId });
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NickName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IconImage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Introduction = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    Suspended = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StorageSpace = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganizationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelMemberships",
                columns: table => new
                {
                    MembershipId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    InvitedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InvitedBy = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Accepted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    Rejected = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsRejected = table.Column<bool>(type: "bit", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    IsEditor = table.Column<bool>(type: "bit", nullable: false),
                    IsReviewer = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Archived = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelMemberships", x => x.MembershipId);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PermaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ArticleCount = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    PublicCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMemberships",
                columns: table => new
                {
                    MemberId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Joined = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMemberships", x => x.MemberId);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganizationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganizationDescription = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrganizationLogo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrganizationWebsite = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrganizationEmail = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrganizationPhone = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrganizationAddress = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrganizationCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganizationState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrganizationCountry = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdClickDailyModel",
                columns: table => new
                {
                    AdClickId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdClickDailyModel", x => x.AdClickId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdClickHourlyModel",
                columns: table => new
                {
                    AdClickId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdClickHourlyModel", x => x.AdClickId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdClickModel",
                columns: table => new
                {
                    AdClickId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdClickModel", x => x.AdClickId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdClickMonthlyModel",
                columns: table => new
                {
                    AdClickId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdClickMonthlyModel", x => x.AdClickId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdImpressionDailyModel",
                columns: table => new
                {
                    AdImpressionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdImpressionDailyModel", x => x.AdImpressionId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdImpressionHourlyModel",
                columns: table => new
                {
                    AdImpressionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdImpressionHourlyModel", x => x.AdImpressionId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdImpressionModel",
                columns: table => new
                {
                    AdImpressionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdImpressionModel", x => x.AdImpressionId);
                });

            migrationBuilder.CreateTable(
                name: "PublisherAdImpressionMonthlyModel",
                columns: table => new
                {
                    AdImpressionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdCreativeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tick = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherAdImpressionMonthlyModel", x => x.AdImpressionId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsDaily_OrganizationId_Tick",
                table: "ArticleImpressionsDaily",
                columns: new[] { "OrganizationId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsDaily_Tick",
                table: "ArticleImpressionsDaily",
                column: "Tick");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsDaily_Tick_OrganizationId_ChannelId_ArticleId",
                table: "ArticleImpressionsDaily",
                columns: new[] { "Tick", "OrganizationId", "ChannelId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsHourly_OrganizationId_Tick",
                table: "ArticleImpressionsHourly",
                columns: new[] { "OrganizationId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsHourly_Tick",
                table: "ArticleImpressionsHourly",
                column: "Tick");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsHourly_Tick_OrganizationId_ChannelId_ArticleId",
                table: "ArticleImpressionsHourly",
                columns: new[] { "Tick", "OrganizationId", "ChannelId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsMonthly_OrganizationId_Tick",
                table: "ArticleImpressionsMonthly",
                columns: new[] { "OrganizationId", "Tick" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsMonthly_Tick",
                table: "ArticleImpressionsMonthly",
                column: "Tick");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImpressionsMonthly_Tick_OrganizationId_ChannelId_ArticleId",
                table: "ArticleImpressionsMonthly",
                columns: new[] { "Tick", "OrganizationId", "ChannelId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdCampaignPerformanceDaily");

            migrationBuilder.DropTable(
                name: "AdCampaignPerformanceHourly");

            migrationBuilder.DropTable(
                name: "AdCampaignPerformanceMonthly");

            migrationBuilder.DropTable(
                name: "AdClicks");

            migrationBuilder.DropTable(
                name: "AdCreativePerformanceDaily");

            migrationBuilder.DropTable(
                name: "AdCreativePerformanceHourly");

            migrationBuilder.DropTable(
                name: "AdCreativePerformanceMonthly");

            migrationBuilder.DropTable(
                name: "AdImpressions");

            migrationBuilder.DropTable(
                name: "ArticleImpressions");

            migrationBuilder.DropTable(
                name: "ArticleImpressionsDaily");

            migrationBuilder.DropTable(
                name: "ArticleImpressionsHourly");

            migrationBuilder.DropTable(
                name: "ArticleImpressionsMonthly");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChannelMemberships");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "OrganizationMemberships");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "PublisherAdClickDailyModel");

            migrationBuilder.DropTable(
                name: "PublisherAdClickHourlyModel");

            migrationBuilder.DropTable(
                name: "PublisherAdClickModel");

            migrationBuilder.DropTable(
                name: "PublisherAdClickMonthlyModel");

            migrationBuilder.DropTable(
                name: "PublisherAdImpressionDailyModel");

            migrationBuilder.DropTable(
                name: "PublisherAdImpressionHourlyModel");

            migrationBuilder.DropTable(
                name: "PublisherAdImpressionModel");

            migrationBuilder.DropTable(
                name: "PublisherAdImpressionMonthlyModel");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
