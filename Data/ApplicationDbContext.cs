using Microsoft.EntityFrameworkCore;
using myapp.Models;

namespace myapp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }
        public DbSet<Topic> Topics { get; set; }

        // AI Chat System
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<MessageCitation> MessageCitations { get; set; }

        // Quiz & Assessment System
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }

        // Learning Progress & Analytics
        public DbSet<CourseProgress> CourseProgresses { get; set; }
        public DbSet<DocumentProgress> DocumentProgresses { get; set; }
        public DbSet<LearningActivity> LearningActivities { get; set; }
        public DbSet<UserStreak> UserStreaks { get; set; }
        public DbSet<UserPoint> UserPoints { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<SkillMastery> SkillMasteries { get; set; }

        // AI Configuration & Settings
        public DbSet<AICourseSetting> AICourseSettings { get; set; }
        public DbSet<AIUsageLog> AIUsageLogs { get; set; }

        // Notifications & Reminders
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StudyReminder> StudyReminders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User and StudentProfile (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.StudentProfile)
                .WithOne(p => p.User)
                .HasForeignKey<StudentProfile>(p => p.UserId);

            // User and Course (One-to-Many: User creates many Courses)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.User)
                .WithMany() // User doesn't have a navigation property back to Courses
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course and Document (One-to-Many)
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Course)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document and DocumentChunk (One-to-Many)
            modelBuilder.Entity<DocumentChunk>()
                .HasOne(dc => dc.Document)
                .WithMany(d => d.DocumentChunks)
                .HasForeignKey(dc => dc.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course and Topic (One-to-Many)
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Topics)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Topic and ParentTopic (Many-to-One: hierarchical topics)
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.ParentTopic)
                .WithMany(t => t.ChildTopics)
                .HasForeignKey(t => t.ParentTopicId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // --- FIX: Correctly configure the Course-Quiz relationship ---
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Quizzes) // Specify the navigation property in Course
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // AI Chat System Relationships
            // ... (rest of the configurations are likely correct, but will be implicitly checked by the migration)
        }
    }
}
