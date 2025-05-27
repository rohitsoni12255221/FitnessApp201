using Microsoft.EntityFrameworkCore;
using UserAPI.Models;
using WorkoutService.Models.DTO;

namespace UserAPI.DAL
{
    public partial class UserDbContext : DbContext
    {
        public UserDbContext()
        {
        }

        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public DbSet<GetAllUserDetailsDto> GetAllUserDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Users__3213E83FED14FA11");

                entity.HasIndex(e => e.UserName, "UQ__Users__66DCF95CB0DEDCFF").IsUnique();

                entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164BEADDFE8").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("email");
                entity.Property(e => e.FitnessGoal)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("fitnessGoal");
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("passwordHash");
                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValue("User")
                    .HasColumnName("role");
                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("userName");
            });

            modelBuilder.Entity<GetAllUserDetailsDto>().HasNoKey().ToView(null); 
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
