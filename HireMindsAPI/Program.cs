using System.Text;
using System.Threading.RateLimiting;
using HireMindsAPI.Models;
using HireMindsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using HireMindsAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ========== 1. DATABASE - Register AppDbContext with MySQL ==========
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!)
);

// ========== 2. JWT AUTHENTICATION ==========
// Read JWT settings from appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    // Set JWT as the default authentication scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // What to validate in every incoming token
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // Expected values
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ========== 3. SWAGGER with JWT Support ==========
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HireMinds API",
        Version = "v1",
        Description = "Smart Interview Preparation & Hiring Platform API"
    });

    // Add JWT Bearer button in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here. Example: eyJhbGciOi..."
    });

    // Make Swagger send the token with every request
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

// ========== 4. CORS - Allow Angular frontend ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ========== 5. MEMORY CACHE ==========
builder.Services.AddMemoryCache();

// ========== 6. RATE LIMITING (built-in .NET 8) ==========
builder.Services.AddRateLimiter(options =>
{
    // Fixed Window: Max 100 requests per 1 minute per IP address
    options.AddFixedWindowLimiter("Fixed", config =>
    {
        config.PermitLimit = 100;              // Max 100 requests
        config.Window = TimeSpan.FromMinutes(1); // Per 1 minute
        config.QueueLimit = 0;                 // Don't queue extra requests
    });

    // When rate limit is hit, return 429 Too Many Requests
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ========== 7. REGISTER SERVICES (Scoped = one instance per request) ==========
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<SubmissionService>();

// ========== 8. REGISTER HttpClient for AI Service ==========
builder.Services.AddHttpClient<AIFeedbackService>();

// ========== 9. CONTROLLERS ==========
builder.Services.AddControllers();

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE (order matters!) ==========

// Global Exception Handler — MUST be first so it catches ALL errors
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger UI (available in all environments for now)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HireMinds API v1");
});

// Serve static files (for resumes)
app.UseStaticFiles();

// Enable CORS
app.UseCors("AllowAngular");

// Rate Limiting
app.UseRateLimiter();

// Authentication must come BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

app.Run();
