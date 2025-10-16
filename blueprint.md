# Blueprint: Hệ thống Trợ lý Học tập Thông minh

## 1. Tổng quan & Mục tiêu

Xây dựng một hệ thống trợ lý học tập thông minh cho sinh viên dựa trên nền tảng .NET 8, với các tính năng chính:
- **Chatbot thông minh**: Trò chuyện và giải đáp thắc mắc dựa trên nội dung giáo trình và tài liệu học tập được cung cấp.
- **Tạo Quiz tự động**: Hệ thống có khả năng tự động sinh ra các câu hỏi trắc nghiệm (quiz) từ tài liệu để giúp sinh viên ôn tập và tự đánh giá.
- **Chấm điểm nhanh**: Hỗ trợ chấm điểm và cung cấp phản hồi tức thì cho các bài quiz.
- **Dashboard tiến độ**: Cung cấp một giao diện trực quan (dashboard) để sinh viên có thể theo dõi tiến độ học tập của mình.
- **Quản lý tài liệu**: Cho phép người dùng tải lên, quản lý và xử lý các tài liệu học tập (PDF, DOCX, PPTX,...).

Mục tiêu là tạo ra một ứng dụng giúp cá nhân hóa và nâng cao hiệu quả tự học, lấy cảm hứng từ các nền tảng như StudyFetch.

---

## 2. Thiết kế & Tính năng đã triển khai

### Cấu trúc & Công nghệ
- **Backend**: .NET 8 Web API
- **Kiến trúc**: Clean Architecture (Domain, Application, Infrastructure, Presentation)
- **Cơ sở dữ liệu**: Entity Framework Core với SQLite (cho môi trường dev) và sẵn sàng cho PostgreSQL với `pgvector`.
- **AI & ML**: Tích hợp với Google AI Platform (Vertex AI) và Google Document AI.
- **Frontend**: React với Vite và Tailwind CSS.

### Phong cách & Giao diện
*(Sẽ được cập nhật khi xây dựng giao diện người dùng)*

### Tính năng
- **Module Quản lý Người dùng**:
    - Models: `User`, `StudentProfile`
    - Database: Thiết lập DbContext và tạo bảng `Users`, `StudentProfiles` qua EF Core Migrations.
    - API: `UsersController` với các endpoint cơ bản (`GET all`, `GET by ID`, `POST`).

- **Module Quản lý Tài liệu & Giáo trình**:
    - Models: `Course`, `Document`, `DocumentChunk`, `Topic`.
    - Database: Cập nhật DbContext và tạo bảng `Courses`, `Documents`, `DocumentChunks`, `Topics` qua EF Core Migrations, bao gồm cột `EmbeddingVector` cho `DocumentChunk`.
    - API: `CoursesController` và `DocumentsController` với các endpoint CRUD cơ bản.
    - **Upload Tài liệu**: `DocumentsController` được cập nhật để tải file lên GCS, gọi Document AI để trích xuất văn bản, chia thành chunks, tạo embeddings và lưu vào database.

- **Module AI Chat Assistant**:
    - Models: `ChatSession`, `ChatMessage`, `MessageCitation`.
    - Database: Cập nhật DbContext và tạo bảng tương ứng qua EF Core Migrations.
    - API: `ChatSessionsController` và `ChatMessagesController` với các endpoint CRUD cơ bản.
    - **AI Chat (RAG)**: `ChatMessagesController` được cập nhật để tích hợp `VertexAIService`, thực hiện vector search trên `DocumentChunk` để lấy ngữ cảnh liên quan và tạo câu trả lời.

- **Module Quiz & Đánh giá**:
    - Models: `Question`, `QuestionOption`, `Quiz`, `QuizQuestion`, `QuizAttempt`, `QuizAnswer`.
    - Database: Cập nhật DbContext và tạo bảng tương ứng qua EF Core Migrations.
    - API: `QuizzesController` và `QuestionsController` với các endpoint CRUD cơ bản.
    - **Quiz Generation**: `QuizzesController` được cập nhật với endpoint `POST /api/Quizzes/Generate` để sinh quiz từ tài liệu/chủ đề bằng `VertexAIService` và lưu câu hỏi vào DB.
    - **Quiz Grading**: `QuizAttemptsController` được tạo và tích hợp `VertexAIService` để chấm điểm các câu hỏi tự luận, cùng với `QuizAnswersController` để quản lý câu trả lời.

- **Module Theo dõi Tiến độ & Analytics**:
    - Models: `CourseProgress`, `DocumentProgress`, `LearningActivity`, `UserStreak`, `UserPoint`, `Achievement`, `UserAchievement`, `SkillMastery`.
    - Database: Cập nhật DbContext và tạo bảng tương ứng qua EF Core Migrations.
    - API: `CourseProgressesController`, `DocumentProgressesController`, `LearningActivitiesController`, `UserStreaksController`, `UserPointsController`, `AchievementsController`, `UserAchievementsController`, `SkillMasteriesController` với các endpoint CRUD cơ bản.
    - **Theo dõi Tiến độ**: `CourseProgressesController`, `DocumentProgressesController`, `LearningActivitiesController` được cập nhật để ghi nhận hoạt động học tập, tính toán và hiển thị tiến độ.

- **Module AI Configuration & Settings**:
    - Models: `AICourseSetting`, `AIUsageLog`.
    - Database: Cập nhật DbContext và tạo bảng tương ứng qua EF Core Migrations.
    - API: `AICourseSettingsController`, `AIUsageLogsController` với các endpoint CRUD cơ bản.

- **Module Notifications & Reminders**:
    - Models: `Notification`, `StudyReminder`.
    - Database: Cập nhật DbContext và tạo bảng tương ứng qua EF Core Migrations.
    - API: `NotificationsController` và `StudyRemindersController` với các endpoint CRUD cơ bản.
    - **Notifications & Reminders**: `NotificationsController` được cập nhật để tạo/đánh dấu đã đọc thông báo, và `StudyRemindersController` được cập nhật để tạo/cập nhật/xóa nhắc nhở.

---

## 3. Kế hoạch hiện tại

**Mục tiêu**: Hoàn tất việc mở rộng chức năng AI và chuẩn bị cho các bước tiếp theo.

**Các bước thực hiện:**

1.  **[COMPLETED]** Tất cả các bước từ 1 đến 39 đã được hoàn thành, bao gồm thiết lập dự án, tạo models, cấu hình database, tạo migrations, phát triển các API controllers cho tất cả các module chính, và tích hợp cơ bản với Google Cloud Platform services (GCS, Document AI, Vertex AI).
2.  **[COMPLETED]** **Kiểm tra & Chạy ứng dụng**: Ứng dụng đã chạy thành công trên môi trường phát triển, có thể truy cập Swagger UI và các API endpoints.
3.  **[COMPLETED]** **Frontend Integration (React with Vite & Tailwind CSS)**:
    *   **[COMPLETED]** Khởi tạo dự án React với Vite trong thư mục `frontend`.
    *   **[COMPLETED]** Cài đặt và cấu hình Tailwind CSS.
    *   **[COMPLETED]** Cấu hình `package.json` với các script cần thiết (`dev`, `build`, `preview`).
    *   **[COMPLETED]** Cập nhật `firebase.json` để phục vụ frontend và proxy backend API.
    *   **[COMPLETED]** Tạo một trang React đơn giản để kiểm tra, bao gồm gọi API backend.
4.  **[COMPLETED]** **Mở rộng chức năng AI (Vector Search & Embeddings)**:
    *   **[COMPLETED]** Cài đặt `Pgvector.EntityFrameworkCore`.
    *   **[COMPLETED]** Cập nhật `DocumentChunk` Model với thuộc tính `EmbeddingVector` kiểu `float[]`.
    *   **[COMPLETED]** Cập nhật `ApplicationDbContext` để cấu hình `EmbeddingVector`.
    *   **[COMPLETED]** Tạo Migration mới cho `EmbeddingVector`.
    *   **[COMPLETED]** Cập nhật Database để áp dụng migration.
    *   **[COMPLETED]** Tạo Service để tạo Embeddings (`VertexAIService` được mở rộng với `GenerateEmbeddingAsync`).
    *   **[COMPLETED]** Cập nhật `DocumentsController` để chia văn bản thành chunks và tạo embeddings khi upload tài liệu.
    *   **[COMPLETED]** Triển khai Vector Search (RAG) trong `ChatMessagesController` để sử dụng `EmbeddingVector` tìm kiếm ngữ nghĩa.

---

## 4. Kế hoạch tiếp theo

**Mục tiêu**: Tối ưu hóa và bảo mật ứng dụng.

**Các bước thực hiện:**

1.  **[PENDING]** **Authentication & Authorization**: Triển khai JWT Bearer authentication và role-based authorization.
2.  **[PENDING]** **Background Jobs**: Tích hợp Hangfire để xử lý các tác vụ nền như xử lý tài liệu (chia chunks, tạo embeddings), gửi nhắc nhở định kỳ.
3.  **[PENDING]** **Real-time Features (SignalR)**: Triển khai SignalR cho các tính năng chat real-time hoặc thông báo tức thì.
4.  **[PENDING]** **Cải thiện Prompts AI**: Tối ưu hóa các prompts được sử dụng trong `VertexAIService` để có kết quả tốt hơn cho chat và quiz generation