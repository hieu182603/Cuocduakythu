using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<McqPart> McqParts { get; set; }
        public DbSet<McqQuestion> McqQuestions { get; set; }
        public DbSet<McqOption> McqOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── mcq_parts ──
            modelBuilder.Entity<McqPart>(entity =>
            {
                entity.ToTable("mcq_parts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PartName).HasColumnName("part_name").IsRequired();
                entity.Property(e => e.Difficulty).HasColumnName("difficulty").HasDefaultValue("medium");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            });

            // ── mcq_questions ──
            modelBuilder.Entity<McqQuestion>(entity =>
            {
                entity.ToTable("mcq_questions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.QuestionNumber).HasColumnName("question_number");
                entity.Property(e => e.PartId).HasColumnName("part_id");
                entity.Property(e => e.QuestionText).HasColumnName("question_text").IsRequired();
                entity.Property(e => e.CorrectAnswer).HasColumnName("correct_answer").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Part)
                      .WithMany(p => p.Questions)
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── mcq_options ──
            modelBuilder.Entity<McqOption>(entity =>
            {
                entity.ToTable("mcq_options");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.QuestionId).HasColumnName("question_id");
                entity.Property(e => e.OptionLetter).HasColumnName("option_letter").IsRequired();
                entity.Property(e => e.OptionText).HasColumnName("option_text").IsRequired();
                entity.Property(e => e.IsCorrect).HasColumnName("is_correct").HasDefaultValue(false);

                entity.HasOne(e => e.Question)
                      .WithMany(q => q.Options)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
