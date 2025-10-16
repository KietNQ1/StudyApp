using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.MemoryStorage;
using myapp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Define CORS policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173") // Allow frontend dev server
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // Important for SignalR
                      });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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
            new string[]{}
        }
    });
});

// Configure DbContext with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key is not configured. Please add a JWT secret key.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Configure Hangfire - Using In-Memory Storage
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());

builder.Services.AddHangfireServer();

// Add SignalR services
builder.Services.AddSignalR();

// Configure Application Services
builder.Services.AddTransient<IBackgroundJobService, BackgroundJobService>();

// Configure Google Cloud Storage Service
var gcsBucketName = builder.Configuration["GoogleCloudStorage:BucketName"];
if (string.IsNullOrEmpty(gcsBucketName))
{
    throw new InvalidOperationException("GoogleCloudStorage:BucketName is not configured.");
}
builder.Services.AddSingleton(new GoogleCloudStorageService(gcsBucketName));

// Configure Google Document AI Service
var docAIProjectId = builder.Configuration["GoogleDocumentAI:ProjectId"];
var docAILocation = builder.Configuration["GoogleDocumentAI:Location"];
var docAIProcessorId = builder.Configuration["GoogleDocumentAI:ProcessorId"];

if (string.IsNullOrEmpty(docAIProjectId) || string.IsNullOrEmpty(docAILocation) || string.IsNullOrEmpty(docAIProcessorId))
{
    throw new InvalidOperationException("GoogleDocumentAI configuration is incomplete.");
}
builder.Services.AddSingleton(new DocumentProcessorService(docAIProjectId, docAILocation, docAIProcessorId));

// Configure Google Vertex AI Service
var vertexAIProjectId = builder.Configuration["GoogleVertexAI:ProjectId"];
var vertexAILocation = builder.Configuration["GoogleVertexAI:Location"];
var vertexAIPublisher = builder.Configuration["GoogleVertexAI:Publisher"];
var vertexAITextGenerationModel = builder.Configuration["GoogleVertexAI:Model"];
var vertexAIEmbeddingModel = builder.Configuration["GoogleVertexAI:EmbeddingModel"];

if (string.IsNullOrEmpty(vertexAIProjectId) || string.IsNullOrEmpty(vertexAILocation) || string.IsNullOrEmpty(vertexAIPublisher) || string.IsNullOrEmpty(vertexAITextGenerationModel) || string.IsNullOrEmpty(vertexAIEmbeddingModel))
{
    throw new InvalidOperationException("GoogleVertexAI configuration is incomplete.");
}
builder.Services.AddSingleton(new VertexAIService(vertexAIProjectId, vertexAILocation, vertexAIPublisher, vertexAITextGenerationModel, vertexAIEmbeddingModel));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS policy
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

// Add Hangfire Dashboard
app.UseHangfireDashboard();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.MapGet("/", () => "Welcome to Study Assistant API!");

app.Run();
