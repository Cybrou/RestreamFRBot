using Microsoft.EntityFrameworkCore;

namespace RestreamFRBot.DAL.Models
{
    public partial class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
        public virtual DbSet<RestreamModule> RestreamModules { get; set; }

        public virtual DbSet<RestreamNotif> RestreamNotifs { get; set; }

        public virtual DbSet<Version> Versions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RestreamModule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("restream_module");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<RestreamNotif>(entity =>
            {
                entity.HasKey(e => new { e.RestreamModuleId, e.Guid });

                entity.ToTable("restream_notif");

                entity.Property(e => e.RestreamModuleId).HasColumnName("restream_module_id");
                entity.Property(e => e.Guid).HasColumnName("guid");
                entity.Property(e => e.InternalSentDate).HasColumnName("sent_date");

                entity.HasOne(e => e.RestreamModule)
                      .WithMany(e => e.RestreamNotifs)
                      .HasForeignKey(e => e.RestreamModuleId);

                entity.Ignore(e => e.SentDate);
            });

            modelBuilder.Entity<Version>(entity =>
            {
                entity.HasKey(e => e.Current);

                entity.ToTable("version");

                entity.Property(e => e.Current).HasColumnName("current");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
