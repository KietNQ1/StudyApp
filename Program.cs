using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Services;
using System.Text.Json.Serialization; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JsonSerializer to ignore object cycles
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseAuthorization();

app.MapControllers(); // Map API controllers

// Add a simple endpoint for the root path to avoid 404 on '/'
app.MapGet("/", () => "Welcome to Study Assistant API!");

app.Run();
