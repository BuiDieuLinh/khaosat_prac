using khaosat_api.Data;
using khaosat_api.Repositories;
using khaosat_api.Repositories.Interfaces;
using khaosat_api.Services;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Survey API",
        Version = "v1"
    });

    // Cấu hình Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT Token theo dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Áp dụng cho toàn bộ API
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<SqlConnectionFactory>();

// Repositories
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<ISurveyElementRepository, SurveyElementRepository>();
builder.Services.AddScoped<ISurveyElementOptionRepository, SurveyElementOptionRepository>();
builder.Services.AddScoped<ISurveyResponseRepository, SurveyResponseRepository>();
builder.Services.AddScoped<ISurveyAnswerRepository, SurveyAnswerRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ISurveyTargetRepository, SurveyTargetRepository>();
builder.Services.AddScoped<ISurveyParticipantRepository, SurveyParticipantRepository>();
builder.Services.AddScoped<ISurveyAccessRepository, SurveyAccessRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Services
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

// Configure JWT Authentication
var secretKey = builder.Configuration["Jwt:Secret"] ?? "SuperSecretKeyMustBeAtLeast32BytesLong1234567890!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "khaosat_api",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "khaosat_fe",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenPermissionVersion = context.Principal?.FindFirst("PermissionVersion")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenPermissionVersion))
            {
                context.Response.Headers["X-Auth-Reason"] = "InvalidToken";
                context.Fail("Token không hợp lệ.");
                return;
            }

            var employeeRepository = context.HttpContext.RequestServices.GetRequiredService<IEmployeeRepository>();
            var dbPermissionVersion = await employeeRepository.GetByIdAsync(Guid.Parse(userId));

            if (dbPermissionVersion == null || !dbPermissionVersion.PermissionVersion.HasValue)
            {
                context.Response.Headers["X-Auth-Reason"] = "UserNotFound";
                context.Fail("Người dùng không tồn tại hoặc thông tin phân quyền chưa khởi tạo.");
                return;
            }

            if (dbPermissionVersion.PermissionVersion.Value.ToString() != tokenPermissionVersion)
            {
                context.Response.Headers["X-Auth-Reason"] = "PermissionChanged";
                context.Fail("Quyền đã thay đổi. Vui lòng đăng nhập lại.");
                return;
            }
        },
        OnChallenge = context =>
        {
            if (!context.Response.Headers.ContainsKey("X-Auth-Reason"))
            {
                context.Response.Headers["X-Auth-Reason"] = "TokenExpired";
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
