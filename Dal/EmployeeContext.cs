using Microsoft.EntityFrameworkCore;

using Template.Dal.Dto;

namespace Template.Dal
{
    public class EmployeeContext : DbContext
    {

        public EmployeeContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeIdentifier>().HasKey(nameof(EmployeeIdentifier.EmployeeKey), nameof(EmployeeIdentifier.IdentifierKey));
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmployeeIdentifier> EmployeeIdentifiers { get; set; }

        public DbSet<Identifier> Identifiers { get; set; }

        public DbSet<IdentifierType> IdentifierTypes { get; set; }
    }
}
