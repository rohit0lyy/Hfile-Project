using System.Collections.Generic;
using HFile.Models;
using Microsoft.EntityFrameworkCore;


namespace HFile.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<MedicalFileUploadModel> MedicalFiles { get; set; }

        public DbSet<UploadFileModels> DocumentDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users table: ensure Imageprofile uses existing column
            modelBuilder.Entity<UserModel>()
                        .Property(u => u.Imageprofile)
                        .HasColumnName("Imageprofile");

            // MedicalFiles table: ignore IFormFile for EF Core
            modelBuilder.Entity<MedicalFileUploadModel>()
                        .Ignore(m => m.FileUpload);
        }
    }
}

