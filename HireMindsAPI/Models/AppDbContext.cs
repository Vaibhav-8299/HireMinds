using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HireMindsAPI.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Candidateprofile> Candidateprofiles { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured in Program.cs via DI
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("answers");

            entity.HasIndex(e => e.ApplicationId, "ApplicationId");

            entity.HasIndex(e => e.QuestionId, "QuestionId");

            entity.HasIndex(e => e.SelectedOptionId, "SelectedOptionId");

            entity.Property(e => e.IsCorrect).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.Application).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("answers_ibfk_1");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("answers_ibfk_2");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.Answers)
                .HasForeignKey(d => d.SelectedOptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("answers_ibfk_3");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("applications");

            entity.HasIndex(e => e.JobId, "JobId");

            entity.HasIndex(e => new { e.CandidateId, e.JobId }, "unique_application").IsUnique();

            entity.Property(e => e.Aifeedback)
                .HasColumnType("text")
                .HasColumnName("AIFeedback");
            entity.Property(e => e.SelectionMessage)
                .HasColumnType("text")
                .HasColumnName("SelectionMessage");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Applied'")
                .HasColumnType("enum('Applied','TestPending','TestTaken','Selected','Rejected')");
            entity.Property(e => e.TestTakenAt).HasColumnType("datetime");

            entity.HasOne(d => d.Candidate).WithMany(p => p.Applications)
                .HasForeignKey(d => d.CandidateId)
                .HasConstraintName("applications_ibfk_1");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("applications_ibfk_2");
        });

        modelBuilder.Entity<Candidateprofile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("candidateprofiles");

            entity.HasIndex(e => e.UserId, "UserId").IsUnique();

            entity.Property(e => e.Branch).HasMaxLength(50);
            entity.Property(e => e.Degree).HasMaxLength(50);
            entity.Property(e => e.Skills).HasMaxLength(500);

            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.SGPA).HasColumnType("decimal(4,2)");
            entity.Property(e => e.TenthSchoolName).HasMaxLength(200);
            entity.Property(e => e.TenthScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TwelfthSchoolName).HasMaxLength(200);
            entity.Property(e => e.TwelfthScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ResumeUrl).HasMaxLength(500);
            entity.Property(e => e.Certifications).HasColumnType("text");
            entity.Property(e => e.Projects).HasColumnType("text");

            entity.HasOne(d => d.User).WithOne(p => p.Candidateprofile)
                .HasForeignKey<Candidateprofile>(d => d.UserId)
                .HasConstraintName("candidateprofiles_ibfk_1");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("jobs");

            entity.HasIndex(e => e.RecruiterId, "RecruiterId");

            entity.HasIndex(e => e.TestId, "TestId");

            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.PostedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.RequiredBranch).HasMaxLength(50);
            entity.Property(e => e.RequiredDegree).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.Property(e => e.MinBTechScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Min10thScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Min12thScore).HasColumnType("decimal(5,2)");

            entity.HasOne(d => d.Recruiter).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.RecruiterId)
                .HasConstraintName("jobs_ibfk_1");

            entity.HasOne(d => d.Test).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("jobs_ibfk_2");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("options");

            entity.HasIndex(e => e.QuestionId, "QuestionId");

            entity.Property(e => e.Text).HasMaxLength(500);

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("options_ibfk_1");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("questions");

            entity.HasIndex(e => e.TestId, "TestId");

            entity.Property(e => e.Difficulty)
                .HasDefaultValueSql("'Medium'")
                .HasColumnType("enum('Easy','Medium','Hard')");
            entity.Property(e => e.Text).HasMaxLength(1000);
            entity.Property(e => e.Topic).HasMaxLength(100);

            entity.HasOne(d => d.Test).WithMany(p => p.Questions)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("questions_ibfk_1");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tests");

            entity.HasIndex(e => e.RecruiterId, "RecruiterId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.TimeLimitMinutes).HasDefaultValueSql("'30'");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Recruiter).WithMany(p => p.Tests)
                .HasForeignKey(d => d.RecruiterId)
                .HasConstraintName("tests_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Role).HasColumnType("enum('Candidate','Recruiter')");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
