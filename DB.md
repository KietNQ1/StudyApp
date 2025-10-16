# Phân tích Database & Tech Stack cho Hệ thống Trợ lý Học tập Thông minh

## Yêu cầu ban đầu

Cùng tôi phân tích db cho 1 hệ thống trợ lý học tập thông minh hỗ trợ sinh viên. Có khả năng chat theo giáo trình, sinh câu hỏi quiz, chấm điểm nhanh, đồng thời cung cấp dashboard theo dõi tiến độ học tập. Đây là ứng dụng giúp việc tự học trở nên cá nhân hóa và hiệu quả hơn. 1 hệ thống khá giống với StudyFetch

**Tech Stack:** .NET 8 API

---

## 🎯 Các Module Chính

### 1. **Quản lý Người dùng & Xác thực**
### 2. **Quản lý Tài liệu & Giáo trình**
### 3. **AI Chat Assistant**
### 4. **Quiz & Đánh giá**
### 5. **Theo dõi Tiến độ & Analytics**

---

## 📊 Thiết kế Database Schema

### **1. User Management**

```sql
-- Bảng người dùng
users
- id (PK)
- email (unique)
- password_hash
- full_name
- avatar_url
- role (student/teacher/admin)
- subscription_tier (free/premium/enterprise)
- created_at
- updated_at
- last_login_at

-- Hồ sơ sinh viên
student_profiles
- id (PK)
- user_id (FK -> users)
- grade_level
- major/field_of_study
- learning_style (visual/auditory/kinesthetic)
- goals
- preferences (JSON)
```

### **2. Content & Document Management**

```sql
-- Giáo trình/Khóa học
courses
- id (PK)
- user_id (FK -> users) -- người tạo
- title
- description
- subject
- thumbnail_url
- is_public
- created_at
- updated_at

-- Tài liệu học tập
documents
- id (PK)
- course_id (FK -> courses)
- title
- file_type (pdf/docx/pptx/video/audio)
- file_url
- file_size
- processing_status (pending/processing/completed/failed)
- extracted_text (TEXT) -- cho full-text search
- page_count
- uploaded_at
- processed_at

-- Chunks của tài liệu (cho RAG/Vector search)
document_chunks
- id (PK)
- document_id (FK -> documents)
- content (TEXT)
- chunk_index
- page_number
- embedding_vector (VECTOR) -- cho semantic search
- token_count
- metadata (JSON)

-- Topics/Chương học
topics
- id (PK)
- course_id (FK -> courses)
- parent_topic_id (FK -> topics) -- cho cấu trúc cây
- title
- description
- order_index
- estimated_time_minutes
```

### **3. AI Chat System**

```sql
-- Phiên chat
chat_sessions
- id (PK)
- user_id (FK -> users)
- course_id (FK -> courses, nullable)
- document_id (FK -> documents, nullable)
- title -- auto-generated từ tin nhắn đầu
- created_at
- updated_at
- last_message_at

-- Tin nhắn chat
chat_messages
- id (PK)
- session_id (FK -> chat_sessions)
- role (user/assistant/system)
- content (TEXT)
- tokens_used
- model_version
- created_at

-- Citations/Nguồn trích dẫn
message_citations
- id (PK)
- message_id (FK -> chat_messages)
- document_id (FK -> documents)
- chunk_id (FK -> document_chunks)
- page_number
- quote_text
- relevance_score
```

### **4. Quiz & Assessment System**

```sql
-- Ngân hàng câu hỏi
questions
- id (PK)
- course_id (FK -> courses)
- topic_id (FK -> topics, nullable)
- document_id (FK -> documents, nullable) -- nếu sinh từ tài liệu
- question_type (multiple_choice/true_false/short_answer/essay)
- question_text (TEXT)
- difficulty_level (easy/medium/hard)
- points
- explanation (TEXT)
- generated_by_ai (boolean)
- created_at

-- Các lựa chọn cho câu hỏi trắc nghiệm
question_options
- id (PK)
- question_id (FK -> questions)
- option_text
- is_correct
- order_index

-- Bài quiz/kiểm tra
quizzes
- id (PK)
- course_id (FK -> courses)
- topic_id (FK -> topics, nullable)
- created_by (FK -> users)
- title
- description
- time_limit_minutes
- passing_score
- shuffle_questions
- shuffle_options
- is_published
- created_at

-- Câu hỏi trong quiz
quiz_questions
- id (PK)
- quiz_id (FK -> quizzes)
- question_id (FK -> questions)
- order_index
- points_override

-- Lần làm bài
quiz_attempts
- id (PK)
- quiz_id (FK -> quizzes)
- user_id (FK -> users)
- started_at
- submitted_at
- time_spent_seconds
- score
- total_points
- percentage
- status (in_progress/completed/abandoned)

-- Câu trả lời của sinh viên
quiz_answers
- id (PK)
- attempt_id (FK -> quiz_attempts)
- question_id (FK -> questions)
- selected_option_id (FK -> question_options, nullable)
- text_answer (TEXT, nullable)
- is_correct
- points_earned
- ai_feedback (TEXT) -- cho câu hỏi tự luận
- answered_at
```

### **5. Learning Progress & Analytics**

```sql
-- Tiến độ học tập theo khóa học
course_progress
- id (PK)
- user_id (FK -> users)
- course_id (FK -> courses)
- enrollment_date
- last_accessed_at
- completion_percentage
- time_spent_minutes
- status (not_started/in_progress/completed)

-- Tiến độ theo tài liệu
document_progress
- id (PK)
- user_id (FK -> users)
- document_id (FK -> documents)
- current_page
- total_pages
- completion_percentage
- time_spent_minutes
- last_accessed_at
- is_completed

-- Hoạt động học tập
learning_activities
- id (PK)
- user_id (FK -> users)
- course_id (FK -> courses, nullable)
- document_id (FK -> documents, nullable)
- activity_type (read/chat/quiz/review)
- duration_minutes
- metadata (JSON) -- chi tiết cụ thể
- created_at

-- Streak & Gamification
user_streaks
- id (PK)
- user_id (FK -> users)
- current_streak_days
- longest_streak_days
- last_activity_date
- total_study_days

-- Điểm số & Thành tựu
user_points
- id (PK)
- user_id (FK -> users)
- course_id (FK -> courses, nullable)
- points_earned
- reason (quiz_completion/daily_streak/etc)
- created_at

achievements
- id (PK)
- name
- description
- badge_icon_url
- criteria (JSON)
- points_reward

user_achievements
- id (PK)
- user_id (FK -> users)
- achievement_id (FK -> achievements)
- earned_at

-- Phân tích điểm yếu/mạnh
skill_mastery
- id (PK)
- user_id (FK -> users)
- course_id (FK -> courses)
- topic_id (FK -> topics)
- mastery_level (0-100)
- questions_attempted
- questions_correct
- last_practiced_at
- needs_review (boolean)
```

### **6. AI Configuration & Settings**

```sql
-- Cấu hình AI cho từng khóa học
ai_course_settings
- id (PK)
- course_id (FK -> courses)
- model_name
- temperature
- max_tokens
- system_prompt (TEXT)
- rag_enabled
- rag_top_k
- updated_at

-- Lịch sử sử dụng AI (cho billing)
ai_usage_logs
- id (PK)
- user_id (FK -> users)
- feature_type (chat/quiz_generation/grading)
- tokens_consumed
- cost_usd
- created_at
```

### **7. Notifications & Reminders**

```sql
notifications
- id (PK)
- user_id (FK -> users)
- type (quiz_due/streak_reminder/achievement/etc)
- title
- message
- is_read
- action_url
- created_at

study_reminders
- id (PK)
- user_id (FK -> users)
- course_id (FK -> courses, nullable)
- reminder_time (TIME)
- days_of_week (JSON array)
- is_active
```

---

## 🔍 Indexes Quan trọng

```sql
-- Performance indexes
CREATE INDEX idx_documents_course ON documents(course_id);
CREATE INDEX idx_documents_status ON documents(processing_status);
CREATE INDEX idx_chat_sessions_user ON chat_sessions(user_id);
CREATE INDEX idx_chat_messages_session ON chat_messages(session_id);
CREATE INDEX idx_quiz_attempts_user ON quiz_attempts(user_id, quiz_id);
CREATE INDEX idx_learning_activities_user_date ON learning_activities(user_id, created_at);

-- Full-text search
CREATE INDEX idx_documents_fulltext ON documents USING GIN(to_tsvector('english', extracted_text));
CREATE INDEX idx_questions_fulltext ON questions USING GIN(to_tsvector('english', question_text));

-- Vector search (nếu dùng pgvector)
CREATE INDEX idx_chunks_embedding ON document_chunks USING ivfflat (embedding_vector vector_cosine_ops);
```

---

## 🚀 Tech Stack Đề Xuất cho .NET 8 API

### **1. Backend Core (.NET 8)**

#### **Framework & Architecture**
```csharp
// API Framework
- ASP.NET Core Web API 8.0
- Minimal APIs hoặc Controller-based (tuỳ preference)
- Clean Architecture / Vertical Slice Architecture

// ORM & Database Access
- Entity Framework Core 8.0 (Code First)
- Dapper (cho complex queries, performance critical)
- Npgsql.EntityFrameworkCore.PostgreSQL (PostgreSQL driver)
```

#### **NuGet Packages quan trọng**
```xml
<!-- Database -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.*" />
<PackageReference Include="Pgvector.EntityFrameworkCore" Version="0.2.*" />
<PackageReference Include="Dapper" Version="2.1.*" />

<!-- Authentication & Authorization -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.*" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.*" />

<!-- API Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.*" />
<PackageReference Include="Scalar.AspNetCore" Version="1.0.*" /> <!-- Modern alternative to Swagger -->

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.*" />

<!-- Mapping -->
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.*" />
<PackageReference Include="Mapster" Version="7.4.*" /> <!-- Faster alternative -->

<!-- Caching -->
<PackageReference Include="StackExchange.Redis" Version="2.7.*" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.*" />

<!-- Background Jobs -->
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.*" />
<PackageReference Include="Hangfire.PostgreSql" Version="1.20.*" />

<!-- Logging & Monitoring -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.*" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.*" />
<PackageReference Include="Serilog.Sinks.Seq" Version="7.0.*" />

<!-- Health Checks -->
<PackageReference Include="AspNetCore.HealthChecks.Npgsql" Version="8.0.*" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.*" />

<!-- Real-time -->
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.*" />

<!-- Rate Limiting -->
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.*" />

<!-- File Processing -->
<PackageReference Include="iTextSharp.LGPLv2.Core" Version="3.4.*" /> <!-- PDF -->
<PackageReference Include="DocumentFormat.OpenXml" Version="3.0.*" /> <!-- Word, Excel -->
```

---

### **2. Google Cloud Platform Services (Khuyên dùng!)**

#### **🌟 Google Cloud Storage (GCS)**
**Thay thế:** AWS S3, Azure Blob Storage
**Use case:** Lưu trữ files (PDF, DOCX, video, audio)

```csharp
// NuGet Package
<PackageReference Include="Google.Cloud.Storage.V1" Version="4.8.*" />

// Implementation
using Google.Cloud.Storage.V1;

public class GoogleCloudStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName = "your-study-app-bucket";

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        using var stream = file.OpenReadStream();
        var objectName = $"{folder}/{Guid.NewGuid()}_{file.FileName}";
        
        await _storageClient.UploadObjectAsync(
            _bucketName, 
            objectName, 
            file.ContentType, 
            stream
        );
        
        return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
    }
}
```

**Ưu điểm:**
- ✅ Rẻ hơn S3 khoảng 20-30%
- ✅ Tốc độ cao, đặc biệt ở châu Á
- ✅ Tích hợp tốt với Vertex AI
- ✅ CDN miễn phí (Cloud CDN)

---

#### **🤖 Google Vertex AI (AI Platform)**
**Thay thế:** OpenAI, Anthropic, Azure OpenAI
**Use case:** AI Chat, Quiz Generation, Grading

```csharp
// NuGet Package
<PackageReference Include="Google.Cloud.AIPlatform.V1" Version="3.10.*" />

// Implementation với Gemini
using Google.Cloud.AIPlatform.V1;

public class VertexAIService
{
    private readonly PredictionServiceClient _client;
    
    public async Task<string> ChatWithDocument(string prompt, string context)
    {
        var request = new PredictRequest
        {
            Endpoint = "projects/YOUR_PROJECT/locations/us-central1/endpoints/...",
            Instances = 
            {
                new()
                {
                    ["prompt"] = $"Context: {context}\n\nQuestion: {prompt}"
                }
            }
        };
        
        var response = await _client.PredictAsync(request);
        return response.Predictions[0].ToString();
    }
}
```

**Models có thể dùng:**
- **Gemini 2.0 Flash** - Nhanh, rẻ, tốt cho chat
- **Gemini 1.5 Pro** - Context lớn (1M tokens), tốt cho RAG
- **Gemini 1.5 Flash** - Balance giữa giá và performance

**Ưu điểm:**
- ✅ Gemini 1.5 Pro có 1M tokens context (tốt cho RAG)
- ✅ Rẻ hơn GPT-4 đáng kể
- ✅ Multimodal (text, image, video, audio)
- ✅ Grounding with Google Search
- ✅ Free tier hào phóng

---

#### **🔍 Google Document AI**
**Use case:** Extract text từ PDF, images, handwriting

```csharp
<PackageReference Include="Google.Cloud.DocumentAI.V1" Version="3.5.*" />

public class DocumentProcessorService
{
    private readonly DocumentProcessorServiceClient _client;
    
    public async Task<string> ExtractTextFromPDF(byte[] fileContent)
    {
        var rawDocument = new RawDocument
        {
            Content = Google.Protobuf.ByteString.CopyFrom(fileContent),
            MimeType = "application/pdf"
        };
        
        var request = new ProcessRequest
        {
            Name = "projects/.../processors/...",
            RawDocument = rawDocument
        };
        
        var response = await _client.ProcessDocumentAsync(request);
        return response.Document.Text;
    }
}
```

**Ưu điểm:**
- ✅ OCR cực tốt cho tiếng Việt
- ✅ Hiểu layout phức tạp (tables, forms)
- ✅ Extract metadata (title, author, date)

---

#### **📊 Google Cloud Firestore (Optional)**
**Use case:** Real-time notifications, chat messages

```csharp
<PackageReference Include="Google.Cloud.Firestore" Version="3.7.*" />

// Real-time listener cho chat
public class FirestoreChatService
{
    private readonly FirestoreDb _db;
    
    public async Task ListenToChatMessages(string sessionId, Action<ChatMessage> onNewMessage)
    {
        var query = _db.Collection("chat_sessions")
            .Document(sessionId)
            .Collection("messages")
            .OrderBy("created_at");
            
        var listener = query.Listen(snapshot =>
        {
            foreach (var change in snapshot.Changes)
            {
                if (change.ChangeType == DocumentChange.Type.Added)
                {
                    var message = change.Document.ConvertTo<ChatMessage>();
                    onNewMessage(message);
                }
            }
        });
    }
}
```

---

#### **🎬 Google Cloud Video Intelligence**
**Use case:** Phân tích video giáo trình, tạo transcript

```csharp
<PackageReference Include="Google.Cloud.VideoIntelligence.V1" Version="3.6.*" />

public class VideoAnalysisService
{
    public async Task<string> TranscribeVideo(string gcsUri)
    {
        var client = VideoIntelligenceServiceClient.Create();
        
        var request = new AnnotateVideoRequest
        {
            InputUri = gcsUri,
            Features = { Feature.SpeechTranscription }
        };
        
        var operation = await client.AnnotateVideoAsync(request);
        var response = await operation.PollUntilCompletedAsync();
        
        // Extract transcript
        var transcript = response.Result.AnnotationResults[0]
            .SpeechTranscriptions[0]
            .Alternatives[0]
            .Transcript;
            
        return transcript;
    }
}
```

---

#### **🔐 Google Cloud Secret Manager**
**Use case:** Quản lý API keys, connection strings

```csharp
<PackageReference Include="Google.Cloud.SecretManager.V1" Version="2.4.*" />

public class SecretManagerService
{
    public async Task<string> GetSecretAsync(string secretId)
    {
        var client = SecretManagerServiceClient.Create();
        var secretName = new SecretVersionName("your-project", secretId, "latest");
        
        var response = await client.AccessSecretVersionAsync(secretName);
        return response.Payload.Data.ToStringUtf8();
    }
}
```

---

### **3. AI/ML Libraries cho .NET**

#### **Vector Search & Embeddings**
```csharp
// Semantic Kernel (Microsoft)
<PackageReference Include="Microsoft.SemanticKernel" Version="1.4.*" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.Postgres" Version="1.4.*" />

// ML.NET
<PackageReference Include="Microsoft.ML" Version="3.0.*" />
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.17.*" />

// OpenAI SDK (nếu dùng OpenAI)
<PackageReference Include="Azure.AI.OpenAI" Version="1.0.*" />

// Langchain alternative cho .NET
<PackageReference Include="LangChain" Version="0.13.*" />
```

#### **Example: RAG Implementation với Semantic Kernel**
```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

public class RAGService
{
    private readonly IKernel _kernel;
    private readonly ISemanticTextMemory _memory;
    
    public async Task<string> ChatWithRAG(string question, string courseId)
    {
        // 1. Search relevant chunks
        var memories = await _memory.SearchAsync(
            collection: $"course_{courseId}",
            query: question,
            limit: 5,
            minRelevanceScore: 0.7
        ).ToListAsync();
        
        // 2. Build context
        var context = string.Join("\n\n", memories.Select(m => m.Metadata.Text));
        
        // 3. Generate answer
        var prompt = $@"
            Context: {context}
            
            Question: {question}
            
            Answer based on the context provided:";
            
        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }
}
```

---

### **4. Database Setup**

#### **PostgreSQL với pgvector**
```sql
-- Install pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Entity Framework migration
public class DocumentChunk
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Content { get; set; }
    
    [Column(TypeName = "vector(1536)")]
    public float[] EmbeddingVector { get; set; }
}

// DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresExtension("vector");
    
    modelBuilder.Entity<DocumentChunk>()
        .HasIndex(e => e.EmbeddingVector)
        .HasMethod("ivfflat");
}
```

---

### **5. Background Jobs**

```csharp
// Hangfire setup
public void ConfigureServices(IServiceCollection services)
{
    services.AddHangfire(config => config
        .UsePostgreSqlStorage(Configuration.GetConnectionString("Default")));
    
    services.AddHangfireServer();
}

// Background job examples
public class BackgroundJobService
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessDocument(int documentId)
    {
        // 1. Download file from GCS
        // 2. Extract text with Document AI
        // 3. Create chunks
        // 4. Generate embeddings
        // 5. Store in database
    }
    
    [Cron("0 9 * * *")] // Daily at 9 AM
    public async Task SendStudyReminders()
    {
        // Send notifications
    }
}
```

---

### **6. Real-time Features (SignalR)**

```csharp
public class ChatHub : Hub
{
    public async Task SendMessage(string sessionId, string message)
    {
        // Process with AI
        var response = await _aiService.GenerateResponse(message);
        
        // Broadcast to group
        await Clients.Group(sessionId).SendAsync("ReceiveMessage", response);
    }
    
    public async Task JoinChatSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }
}
```

---

## 📦 Project Structure Đề Xuất

```
StudyAssistant.API/
├── src/
│   ├── StudyAssistant.API/              # API Layer
│   │   ├── Controllers/
│   │   ├── Hubs/                        # SignalR
│   │   ├── Filters/
│   │   └── Program.cs
│   │
│   ├── StudyAssistant.Application/      # Business Logic
│   │   ├── Services/
│   │   │   ├── AI/
│   │   │   │   ├── VertexAIService.cs
│   │   │   │   ├── RAGService.cs
│   │   │   │   └── QuizGeneratorService.cs
│   │   │   ├── Documents/
│   │   │   │   ├── DocumentProcessorService.cs
│   │   │   │   └── StorageService.cs
│   │   │   └── Analytics/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Interfaces/
│   │
│   ├── StudyAssistant.Domain/           # Domain Models
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Events/
│   │
│   ├── StudyAssistant.Infrastructure/   # Data Access
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   ├── ExternalServices/
│   │   │   ├── GoogleCloud/
│   │   │   └── Redis/
│   │   └── BackgroundJobs/
│   │
│   └── StudyAssistant.Shared/           # Shared
│       ├── Constants/
│       ├── Exceptions/
│       └── Extensions/
│
└── tests/
    ├── StudyAssistant.UnitTests/
    └── StudyAssistant.IntegrationTests/
```

---


### **Stack cuối cùng:**
- **.NET 8 API** - Backend
- **PostgreSQL + pgvector** - Database
- **Redis** - Cache
- **Google Cloud Storage** - File storage
- **Vertex AI (Gemini)** - AI/ML
- **Document AI** - OCR
- **SignalR** - Real-time
- **Hangfire** - Background jobs