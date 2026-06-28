using Microsoft.EntityFrameworkCore;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.Career.Domain.Recruiters;
using CareerApplication = UniHub.Career.Domain.Applications.Application;

namespace UniHub.Career.Infrastructure.Persistence;

public sealed class CareerDbContext : DbContext
{
    public CareerDbContext(DbContextOptions<CareerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<CareerApplication> Applications => Set<CareerApplication>();
    public DbSet<Recruiter> Recruiters => Set<Recruiter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareerDbContext).Assembly);
    }
}
