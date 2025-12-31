using System.Text;
using ArticleService.Application.Abstractions;
using ArticleService.Infrastructure;
using ArticleService.Infrastructure.Abstractions;
using ArticleService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ArticleService.Application.Services;
using Microsoft.OpenApi.Models;
using IdentityService.Grpc;
using Amazon.S3;
using ArticleService.Application.Abstractions;
using ArticleService.Infrastructure.Options;
using ArticleService.Infrastructure.Services;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var builder = WebApplication.CreateBuilder(args);
// Bind Minio options
builder.Services.Configure<MinioOptions>(
    builder.Configuration.GetSection("Minio"));

// ساخت کلاینت S3 برای MinIO
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MinioOptions>>().Value;

    var config = new AmazonS3Config
    {
        ServiceURL = opts.Endpoint,   // "http://localhost:9000"
        ForcePathStyle = true         // برای MinIO مهمه
    };

    return new AmazonS3Client(opts.AccessKey, opts.SecretKey, config);
});

// سرویس مدیا
builder.Services.AddScoped<IMediaStorageService, S3MediaStorageService>();
var configuration = builder.Configuration;

// Controllers
builder.Services.AddControllers();

// Swagger پایه
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ArticleService API",
        Version = "v1"
    });

    // --- تعریف Security برای Bearer ---
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter JWT Bearer token",

        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});
builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["IdentityService:GrpcUrl"]
        ?? throw new Exception("IdentityService:GrpcUrl not configured"));
});

// ConnectionFactory برای articles_db
var articlesConnectionString = configuration.GetConnectionString("ArticlesDb")
    ?? throw new InvalidOperationException("Connection string 'ArticlesDb' is not configured.");

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new NpgsqlConnectionFactory(articlesConnectionString));

// Repository
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleAppService>();

// JWT Authentication (هم‌خوان با IdentityService)
var jwtSection = configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"]
                 ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();