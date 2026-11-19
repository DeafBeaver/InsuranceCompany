using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceSystem.BLL.Persistence
{
    public class InsuranceDbContext : DbContext
    {
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Agent> Agents => Set<Agent>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Request> Requests => Set<Request>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=insurance.db");
        }
    }
}
