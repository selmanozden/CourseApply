using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using static CourseApply3.Models.CourseProperty;


namespace CourseApply3.Models
{
    public class CourseDbContext:DbContext
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options)
           : base(options)
        {
        }
        public DbSet<CourseProperty.UserProperty> Users { get; set; }
        public DbSet<CourseProperty.ExamProperty> Exams { get; set; }
        public DbSet<CourseProperty.QuestionProperty> Questions { get; set; }
        public DbSet<CourseProperty.UserInformation> UserInformations { get; set; }
        public DbSet<CourseProperty.AdminProperty> Admins { get; set; }
        public DbSet<CourseProperty.AnswerProperty> Answers { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // QuestionProperty ve ExamProperty arasındaki ilişki
            modelBuilder.Entity<CourseProperty.QuestionProperty>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade); // Sınav silindiğinde ilgili soruları da siler

            // UserInformation ve ExamProperty arasındaki ilişki
            modelBuilder.Entity<CourseProperty.UserInformation>()
                .HasOne(ui => ui.Exam)
                .WithMany()
                .HasForeignKey(ui => ui.ExamId)
                .OnDelete(DeleteBehavior.Cascade); // Sınav silindiğinde kullanıcı bilgilerini etkilemez

            // UserInformation ve UserProperty arasındaki ilişki
            modelBuilder.Entity<CourseProperty.UserInformation>()
                .HasOne<UserProperty>(ui => ui.User)
                .WithMany()
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde kullanıcı bilgilerini etkilemez
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=D:\\sqllite\\sqlitecoursedatabase\\Course3\\CourseDb.db")
                              .UseLazyLoadingProxies(); 
            }
        }
    }
}
