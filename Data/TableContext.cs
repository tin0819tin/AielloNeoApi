using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aiello_Restful_API.Models;

namespace Aiello_Restful_API.Data
{
    public class TableContext : DbContext
    {

        public TableContext(DbContextOptions<TableContext> options) : base(options)
        {
           
        }


        public DbSet<UuidTable> UuidTables { get; set; }

        public Guid getId()
        {
            var row = this.UuidTables.Where(s => s.RegisteredTime == null).FirstOrDefault();
            try
            {
                row.RegisteredTime = DateTimeOffset.Now;
            }
            catch(Exception ex)
            {
                throw (ArgumentNullException)ex;
            }

            return row.Id;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlServer("Server=aiello-database.database.windows.net;Database=AielloSensenet;Persist Security Info=False;User ID=aiello_rd;Password=HelloVic@#123;Connection Timeout=30;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Max Pool Size=500;Min Pool Size=10");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UuidTable>(entity =>
            {
                entity.ToTable("Aiello_UUID_Test");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                .HasColumnName("Id");

                entity.Property(e => e.RegisteredTime)
                .HasColumnName("RegisteredTime")
                .HasColumnType("datetimeoffset(3)");

            });
        }
    }
}
