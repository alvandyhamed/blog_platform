using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Application.Auth;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger برای REST
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IdentityService.Api",
        Version = "v1"
    });

    // تعریف Bearer برای دکمه Authorize
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,   // مهم: Http + bearer
        Scheme = "bearer",                // حتماً lowercase
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme"
    });

    // بگو همه‌ی endpointها می‌تونن از این schema استفاده کنن
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


// gRPC
builder.Services.AddGrpc();

// Infrastructure
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Application services
builder.Services.AddScoped<GoogleAuthService>();

// TODO: بعداً GoogleOAuthService واقعی رو پیاده می‌کنیم
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IGoogleOAuthService, DummyGoogleOAuthService>();
}
else
{
    builder.Services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>();
}

// TODO: بعداً اینجا DI برای لایه های Application/Infrastructure رو اضافه می‌کنیم
// مثل:
// builder.Services.AddScoped<IUserRepository, UserRepository>();

// JWT Auth (الان کانفیگ رو ساده می‌ذاریم، بعداً کاملش می‌کنیم)
var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"];

if (!string.IsNullOrWhiteSpace(signingKey))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

if (!string.IsNullOrWhiteSpace(signingKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// REST Controllers
app.MapControllers();

// gRPC endpoints (فعلاً placeholder، بعداً سرویس واقعی اضافه می‌کنیم)
// app.MapGrpcService<IdentityGrpcService>();
// app.MapGet("/_proto", () => "TODO: expose proto or info");

app.Run();