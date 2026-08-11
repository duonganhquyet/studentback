using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Domain.Entities;
using System;

namespace StudentAcademicManagement.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<School> Schools { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<StudentContact> StudentContacts { get; set; }
        public DbSet<StudentDocument> StudentDocuments { get; set; }
        public DbSet<StudentIdentity> StudentIdentities { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<StudentEditRequest> StudentEditRequests { get; set; }
        public DbSet<SpecialCategory> SpecialCategories { get; set; }
        public DbSet<StudentSpecialCategory> StudentSpecialCategories { get; set; }
        public DbSet<StudentFamilyMember> FamilyMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // School Configuration
            modelBuilder.Entity<School>().HasIndex(s => s.SchoolCode).IsUnique();

            // User Configuration
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>()
                .HasOne(u => u.School)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student Configuration
            // Unique MSSV trong phạm vi 1 trường học
            modelBuilder.Entity<Student>()
                .HasIndex(s => new { s.SchoolId, s.StudentCode })
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.School)
                .WithMany()
                .HasForeignKey(s => s.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Profile Configuration
            modelBuilder.Entity<StudentProfile>()
                .HasOne(sp => sp.Student)
                .WithOne(s => s.Profile)
                .HasForeignKey<StudentProfile>(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Student thì xóa luôn Profile

            // Contact Configuration
            modelBuilder.Entity<StudentContact>()
                .HasOne(sc => sc.Student)
                .WithOne(s => s.Contact)
                .HasForeignKey<StudentContact>(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Student thì xóa luôn Contact

            // Document Configuration
            modelBuilder.Entity<StudentDocument>()
                .HasOne(sd => sd.Student)
                .WithMany()
                .HasForeignKey(sd => sd.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Identity Configuration
            modelBuilder.Entity<StudentIdentity>()
                .HasOne(si => si.Student)
                .WithOne(s => s.Identity)
                .HasForeignKey<StudentIdentity>(si => si.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Edit Request Configuration
            modelBuilder.Entity<StudentEditRequest>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Special Category Configuration
            modelBuilder.Entity<SpecialCategory>()
                .HasOne(sc => sc.School)
                .WithMany()
                .HasForeignKey(sc => sc.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentSpecialCategory>()
                .HasOne(ssc => ssc.Student)
                .WithMany()
                .HasForeignKey(ssc => ssc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentSpecialCategory>()
                .HasOne(ssc => ssc.SpecialCategory)
                .WithMany()
                .HasForeignKey(ssc => ssc.SpecialCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Category nếu đang có Sinh viên đăng ký

            // ================= SEED DATA ================= //
            // Cố định mốc thời gian để tránh lỗi PendingModelChangesWarning khi Migration
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "SuperAdmin", CreatedAt = seedDate },
                new Role { Id = 2, Name = "SchoolAdmin", CreatedAt = seedDate },
                new Role { Id = 3, Name = "Student", CreatedAt = seedDate }
            );

            var superAdminHash = "$2a$12$LeZohjv5cBvtODrGfDy7YOW0/vNNeOHoY251SLJSfDRdUQw8JpHzO";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "superadmin@system.com",
                    PasswordHash = superAdminHash,
                    RoleId = 1,
                    SchoolId = null,
                    IsFirstLogin = false,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );
        }
    }
}