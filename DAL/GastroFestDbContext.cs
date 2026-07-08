using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL;

public class GastroFestDbContext : DbContext
{
    public GastroFestDbContext(DbContextOptions<GastroFestDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Festival> Festivals => Set<Festival>();
    public DbSet<FestivalImage> FestivalImages => Set<FestivalImage>();
    public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // всё лежит в схеме gf
        b.HasDefaultSchema("gf");

        // users
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.PwdHash).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
        });

        // festivals
        b.Entity<Festival>(e =>
        {
            e.ToTable("festivals", t =>
            {
                // чек был в SQL — продублируем для миграций EF
                t.HasCheckConstraint("CK_festivals_dates", "end_date >= start_date");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired();
            e.Property(x => x.StartDate).HasColumnName("start_date");
            e.Property(x => x.EndDate).HasColumnName("end_date");
            e.Property(x => x.Latitude).HasPrecision(9, 6);
            e.Property(x => x.Longitude).HasPrecision(9, 6);

            e.HasMany(x => x.Images)
             .WithOne(i => i.Festival!)
             .HasForeignKey(i => i.FestivalId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // festival_images
        b.Entity<FestivalImage>(e =>
        {
            e.ToTable("festival_images");
            e.HasKey(x => x.Id);
            e.Property(x => x.Url).IsRequired();
            e.Property(x => x.SortOrder).HasColumnName("sort_order");
            e.HasIndex(x => new { x.FestivalId, x.SortOrder });
        });

        // user_favorites (M2M с полезным полем created_at)
        b.Entity<UserFavorite>(e =>
        {
            e.ToTable("user_favorites");
            e.HasKey(x => new { x.UserId, x.FestivalId });

            e.HasOne(x => x.User)
             .WithMany(u => u.Favorites)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Festival)
             .WithMany(f => f.Favorites)
             .HasForeignKey(x => x.FestivalId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.FestivalId);
        });

        // contact_messages
        b.Entity<ContactMessage>(e =>
        {
            e.ToTable("contact_messages");
            e.HasKey(x => x.Id);
            e.Property(x => x.IsRead).HasColumnName("is_read");
        });
    }
}
public class GastroFestDbContextFactory : IDesignTimeDbContextFactory<GastroFestDbContext>
{
    public GastroFestDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GastroFestDbContext>()
            .UseNpgsql("Host=ep-holy-star-agws4xak-pooler.c-2.eu-central-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_7u8EAtwhICKm;Ssl Mode=Require;Trust Server Certificate=true")
            .Options;

        return new GastroFestDbContext(options);
    }
}