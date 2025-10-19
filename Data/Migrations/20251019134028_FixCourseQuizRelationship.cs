using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseQuizRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIUsageLogs_Users_UserId",
                table: "AIUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Courses_CourseId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Documents_DocumentId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Users_UserId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseProgresses_Users_UserId",
                table: "CourseProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentProgresses_Users_UserId",
                table: "DocumentProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningActivities_Courses_CourseId",
                table: "LearningActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningActivities_Documents_DocumentId",
                table: "LearningActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningActivities_Users_UserId",
                table: "LearningActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageCitations_DocumentChunks_ChunkId",
                table: "MessageCitations");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageCitations_Documents_DocumentId",
                table: "MessageCitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Documents_DocumentId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Topics_TopicId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_QuestionOptions_SelectedOptionId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_Questions_QuestionId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Questions_QuestionId",
                table: "QuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Topics_TopicId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_SkillMasteries_Users_UserId",
                table: "SkillMasteries");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyReminders_Courses_CourseId",
                table: "StudyReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPoints_Courses_CourseId",
                table: "UserPoints");

            migrationBuilder.AddForeignKey(
                name: "FK_AIUsageLogs_Users_UserId",
                table: "AIUsageLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Courses_CourseId",
                table: "ChatSessions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Documents_DocumentId",
                table: "ChatSessions",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Users_UserId",
                table: "ChatSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseProgresses_Users_UserId",
                table: "CourseProgresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProgresses_Users_UserId",
                table: "DocumentProgresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningActivities_Courses_CourseId",
                table: "LearningActivities",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningActivities_Documents_DocumentId",
                table: "LearningActivities",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningActivities_Users_UserId",
                table: "LearningActivities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageCitations_DocumentChunks_ChunkId",
                table: "MessageCitations",
                column: "ChunkId",
                principalTable: "DocumentChunks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageCitations_Documents_DocumentId",
                table: "MessageCitations",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Documents_DocumentId",
                table: "Questions",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Topics_TopicId",
                table: "Questions",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_QuestionOptions_SelectedOptionId",
                table: "QuizAnswers",
                column: "SelectedOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_Questions_QuestionId",
                table: "QuizAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Questions_QuestionId",
                table: "QuizQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Topics_TopicId",
                table: "Quizzes",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SkillMasteries_Users_UserId",
                table: "SkillMasteries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyReminders_Courses_CourseId",
                table: "StudyReminders",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPoints_Courses_CourseId",
                table: "UserPoints",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIUsageLogs_Users_UserId",
                table: "AIUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Courses_CourseId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Documents_DocumentId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Users_UserId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseProgresses_Users_UserId",
                table: "CourseProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentProgresses_Users_UserId",
                table: "DocumentProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningActivities_Courses_CourseId",
                table: "LearningActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningActivities_Documents_DocumentId",
                table: "LearningActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningActivities_Users_UserId",
                table: "LearningActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageCitations_DocumentChunks_ChunkId",
                table: "MessageCitations");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageCitations_Documents_DocumentId",
                table: "MessageCitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Documents_DocumentId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Topics_TopicId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_QuestionOptions_SelectedOptionId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_Questions_QuestionId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Questions_QuestionId",
                table: "QuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Topics_TopicId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_SkillMasteries_Users_UserId",
                table: "SkillMasteries");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyReminders_Courses_CourseId",
                table: "StudyReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPoints_Courses_CourseId",
                table: "UserPoints");

            migrationBuilder.AddForeignKey(
                name: "FK_AIUsageLogs_Users_UserId",
                table: "AIUsageLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Courses_CourseId",
                table: "ChatSessions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Documents_DocumentId",
                table: "ChatSessions",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Users_UserId",
                table: "ChatSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseProgresses_Users_UserId",
                table: "CourseProgresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProgresses_Users_UserId",
                table: "DocumentProgresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningActivities_Courses_CourseId",
                table: "LearningActivities",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningActivities_Documents_DocumentId",
                table: "LearningActivities",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningActivities_Users_UserId",
                table: "LearningActivities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageCitations_DocumentChunks_ChunkId",
                table: "MessageCitations",
                column: "ChunkId",
                principalTable: "DocumentChunks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageCitations_Documents_DocumentId",
                table: "MessageCitations",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Courses_CourseId",
                table: "Questions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Documents_DocumentId",
                table: "Questions",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Topics_TopicId",
                table: "Questions",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_QuestionOptions_SelectedOptionId",
                table: "QuizAnswers",
                column: "SelectedOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_Questions_QuestionId",
                table: "QuizAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_UserId",
                table: "QuizAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Questions_QuestionId",
                table: "QuizQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Topics_TopicId",
                table: "Quizzes",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SkillMasteries_Users_UserId",
                table: "SkillMasteries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyReminders_Courses_CourseId",
                table: "StudyReminders",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPoints_Courses_CourseId",
                table: "UserPoints",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
