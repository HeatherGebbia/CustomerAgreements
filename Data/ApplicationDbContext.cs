using Microsoft.EntityFrameworkCore;
using CustomerAgreements.Models;

namespace CustomerAgreements.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DependentAnswer> DependentAnswers { get; set; }
        public DbSet<DependentQuestionList> DependentQuestionLists { get; set; }
        public DbSet<DependentQuestion> DependentQuestions { get; set; }
        public DbSet<Models.File> Files { get; set; }
        public DbSet<List> Lists { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<QuestionLibrary> QuestionLibrary { get; set; }
        public DbSet<QuestionList> QuestionLists { get; set; }
        public DbSet<Section> Sections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Questionnaire>().ToTable("cqQuestionnaires");
            modelBuilder.Entity<Question>().ToTable("cqQuestions");
            modelBuilder.Entity<Agreement>().ToTable("cqAgreements");
            modelBuilder.Entity<Answer>().ToTable("cqAnswers");
            modelBuilder.Entity<Customer>().ToTable("cqCustomers");
            modelBuilder.Entity<DependentAnswer>().ToTable("cqDependentAnswers");
            modelBuilder.Entity<DependentQuestionList>().ToTable("cqDependentQuestionLists");
            modelBuilder.Entity<DependentQuestion>().ToTable("cqDependentQuestions");
            modelBuilder.Entity<Models.File>().ToTable("cqFiles");
            modelBuilder.Entity<List>().ToTable("cqLists");
            modelBuilder.Entity<Notification>().ToTable("cqNotifications");
            modelBuilder.Entity<QuestionLibrary>().ToTable("cqQuestionLibrary");
            modelBuilder.Entity<QuestionList>().ToTable("cqQuestionLists");
            modelBuilder.Entity<Section>().ToTable("cqSections");

            modelBuilder.Entity<QuestionList>()
            .HasOne(q => q.Question)
            .WithMany(q => q.QuestionLists)
            .HasForeignKey(q => new { q.QuestionID, q.QuestionnaireID }) 
            .HasPrincipalKey(q => new { q.QuestionID, q.QuestionnaireID });

            // Section → Questions (cascade delete)
            //modelBuilder.Entity<Section>()
            //    .HasMany(s => s.Questions)
            //    .WithOne(q => q.Section)
            //    .HasForeignKey(q => q.SectionID)
            //    .OnDelete(DeleteBehavior.Cascade);

            //// Question → QuestionLists (cascade delete)
            //modelBuilder.Entity<Question>()
            //    .HasMany(q => q.QuestionLists)
            //    .WithOne(ql => ql.Question)
            //    .HasForeignKey(ql => ql.QuestionID)
            //    .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
