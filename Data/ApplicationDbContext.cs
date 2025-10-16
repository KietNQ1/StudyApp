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
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of User when Course is deleted

            // Course and Document (One-to-Many)
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Course)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Delete documents when course is deleted

            // Document and DocumentChunk (One-to-Many)
            modelBuilder.Entity<DocumentChunk>()
                .HasOne(dc => dc.Document)
                .WithMany(d => d.DocumentChunks)
                .HasForeignKey(dc => dc.DocumentId)
                .OnDelete(DeleteBehavior.Cascade); // Delete chunks when document is deleted

            // Course and Topic (One-to-Many)
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Topics)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Delete topics when course is deleted

            // Topic and ParentTopic (Many-to-One: hierarchical topics)
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.ParentTopic)
                .WithMany(t => t.ChildTopics)
                .HasForeignKey(t => t.ParentTopicId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete on parent topic

            // AI Chat System Relationships
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Keep chat sessions even if user is deleted (or handle explicitly)

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Course)
                .WithMany()
                .HasForeignKey(cs => cs.CourseId)
                .IsRequired(false) // CourseId is nullable
                .OnDelete(DeleteBehavior.SetNull); // Set CourseId to null if Course is deleted

            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Document)
                .WithMany()
                .HasForeignKey(cs => cs.DocumentId)
                .IsRequired(false) // DocumentId is nullable
                .OnDelete(DeleteBehavior.SetNull); // Set DocumentId to null if Document is deleted

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.ChatSession)
                .WithMany(cs => cs.ChatMessages)
                .HasForeignKey(cm => cm.SessionId)
                .OnDelete(DeleteBehavior.Cascade); // Delete messages when session is deleted

            modelBuilder.Entity<MessageCitation>()
                .HasOne(mc => mc.ChatMessage)
                .WithMany(cm => cm.MessageCitations)
                .HasForeignKey(mc => mc.MessageId)
                .OnDelete(DeleteBehavior.Cascade); // Delete citations when message is deleted

            modelBuilder.Entity<MessageCitation>()
                .HasOne(mc => mc.Document)
                .WithMany()
                .HasForeignKey(mc => mc.DocumentId)
                .OnDelete(DeleteBehavior.Restrict); // Keep document even if citation is deleted (or handle explicitly)

            modelBuilder.Entity<MessageCitation>()
                .HasOne(mc => mc.DocumentChunk)
                .WithMany()
                .HasForeignKey(mc => mc.ChunkId)
                .IsRequired(false) // ChunkId is nullable
                .OnDelete(DeleteBehavior.SetNull); // Set ChunkId to null if Chunk is deleted

            // Quiz & Assessment System Relationships
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Course)
                .WithMany()
                .HasForeignKey(q => q.CourseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Topic)
                .WithMany()
                .HasForeignKey(q => q.TopicId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Document)
                .WithMany()
                .HasForeignKey(q => q.DocumentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<QuestionOption>()
                .HasOne(qo => qo.Question)
                .WithMany(q => q.QuestionOptions)
                .HasForeignKey(qo => qo.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany()
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Topic)
                .WithMany()
                .HasForeignKey(q => q.TopicId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Creator)
                .WithMany()
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict); // Keep quiz even if creator user is deleted

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Question)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Question might be used in other quizzes

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Quiz)
                .WithMany(q => q.QuizAttempts)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.User)
                .WithMany()
                .HasForeignKey(qa => qa.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Keep attempt even if user is deleted

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(qans => qans.QuizAttempt)
                .WithMany(qa => qa.QuizAnswers)
                .HasForeignKey(qans => qans.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(qans => qans.Question)
                .WithMany(q => q.QuizAnswers)
                .HasForeignKey(qans => qans.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Keep question even if answer is deleted

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(qans => qans.SelectedOption)
                .WithMany()
                .HasForeignKey(qans => qans.SelectedOptionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); // Set option to null if option is deleted

            // Learning Progress & Analytics Relationships
            modelBuilder.Entity<CourseProgress>()
                .HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseProgress>()
                .HasOne(cp => cp.Course)
                .WithMany()
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentProgress>()
                .HasOne(dp => dp.User)
                .WithMany()
                .HasForeignKey(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentProgress>()
                .HasOne(dp => dp.Document)
                .WithMany()
                .HasForeignKey(dp => dp.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearningActivity>()
                .HasOne(la => la.User)
                .WithMany()
                .HasForeignKey(la => la.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LearningActivity>()
                .HasOne(la => la.Course)
                .WithMany()
                .HasForeignKey(la => la.CourseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LearningActivity>()
                .HasOne(la => la.Document)
                .WithMany()
                .HasForeignKey(la => la.DocumentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserStreak>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPoint>()
                .HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPoint>()
                .HasOne(up => up.Course)
                .WithMany()
                .HasForeignKey(up => up.CourseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany()
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Restrict); // Achievement definitions should remain

            modelBuilder.Entity<SkillMastery>()
                .HasOne(sm => sm.User)
                .WithMany()
                .HasForeignKey(sm => sm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SkillMastery>()
                .HasOne(sm => sm.Course)
                .WithMany()
                .HasForeignKey(sm => sm.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SkillMastery>()
                .HasOne(sm => sm.Topic)
                .WithMany()
                .HasForeignKey(sm => sm.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            // AI Configuration & Settings Relationships
            modelBuilder.Entity<AICourseSetting>()
                .HasOne(acs => acs.Course)
                .WithMany()
                .HasForeignKey(acs => acs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AIUsageLog>()
                .HasOne(aul => aul.User)
                .WithMany()
                .HasForeignKey(aul => aul.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notifications & Reminders Relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete notifications if user is deleted

            modelBuilder.Entity<StudyReminder>()
                .HasOne(sr => sr.User)
                .WithMany()
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete reminders if user is deleted

            modelBuilder.Entity<StudyReminder>()
                .HasOne(sr => sr.Course)
                .WithMany()
                .HasForeignKey(sr => sr.CourseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); // Set CourseId to null if Course is deleted
        }
    }
}
