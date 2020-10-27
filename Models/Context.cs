using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using mini_message.Common;
using mini_message.Common.Settings;

namespace mini_message.Models
{
    public class Context : DbContext
    {
        public static DatabaseSettings DatabaseConfiguration;
        private bool useInMemoryDatabase;
        private bool useConfiguration = true;
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<UploadFile> UploadFiles { get; set; }
        public virtual DbSet<Participant> Participants { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Complaint> Complaints { get; set; }
        public virtual DbSet<ChatRoom> ChatRooms { get; set; }
        public virtual DbSet<BlockedUser> BlockedUsers { get; set; }

        public Context()
        {

        }
        public Context(bool useInMemoryDatabase)
        {
            this.useInMemoryDatabase = useInMemoryDatabase;
            useConfiguration = true;
        }

        public Context(DatabaseSettings databaseConfiguration)
        {
            if (databaseConfiguration != null)
                DatabaseConfiguration = databaseConfiguration;
        }
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (useInMemoryDatabase)
                optionsBuilder.UseInMemoryDatabase("mimicrymessanger");
            if (useConfiguration) {
                if (!optionsBuilder.IsConfigured) {
                    optionsBuilder.UseMySql(DatabaseConnection());
                }
            }
        }
        public static string DatabaseConnection()
        {
            if (DatabaseConfiguration == null)
            {
                var configuration = ServerConfiguration.Get();
                DatabaseConfiguration = configuration
                    .GetSection("DatabaseSettings")
                    .Get<DatabaseSettings>();
            }
            return "Server=" + DatabaseConfiguration.Server +
               ";Database=" + DatabaseConfiguration.Database + 
               ";User=" + DatabaseConfiguration.User + 
               ";Pwd=" + DatabaseConfiguration.Password + 
               ";Charset=utf8;";
            
        }
        public static string DatabaseConnectionForSqlConnection()
        {
            if (DatabaseConfiguration == null)
            {
                var configuration = ServerConfiguration.Get(); 
                DatabaseConfiguration = configuration
                    .GetSection("DatabaseSettings")
                    .Get<DatabaseSettings>();
            }
            return "server=" + DatabaseConfiguration.Server +
                ";database=" + DatabaseConfiguration.Database + 
                ";uid=" + DatabaseConfiguration.User + 
                ";pwd=" + DatabaseConfiguration.Password + 
                ";";
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}