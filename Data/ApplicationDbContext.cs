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
        }
    }
}
