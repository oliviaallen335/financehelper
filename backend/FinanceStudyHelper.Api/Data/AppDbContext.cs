using FinanceStudyHelper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceStudyHelper.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Lecture> Lectures => Set<Lecture>();
    public DbSet<LectureChunk> LectureChunks => Set<LectureChunk>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lecture>(entity =>
        {
            entity.ToTable("lectures");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(x => x.SourceFile).HasColumnName("source_file").HasMaxLength(255).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<LectureChunk>(entity =>
        {
            entity.ToTable("lecture_chunks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.LectureId).HasColumnName("lecture_id");
            entity.Property(x => x.ChunkIndex).HasColumnName("chunk_index");
            entity.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
            entity.Property(x => x.Embedding).HasColumnName("embedding").HasColumnType("json");
            entity.HasOne(x => x.Lecture)
                .WithMany(x => x.Chunks)
                .HasForeignKey(x => x.LectureId);
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.ToTable("chat_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserLabel).HasColumnName("user_label").HasMaxLength(100);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SessionId).HasColumnName("session_id");
            entity.Property(x => x.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne(x => x.Session)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.SessionId);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.ToTable("quizzes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SessionId).HasColumnName("session_id");
            entity.Property(x => x.Topic).HasColumnName("topic").HasMaxLength(255).IsRequired();
            entity.Property(x => x.QuizJson).HasColumnName("quiz_json").HasColumnType("json").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId);
        });
    }
}
