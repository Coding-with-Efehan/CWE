namespace CWE.Data.Context
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Implementation of <see cref="IDesignTimeDbContextFactory{TContext}"/> for <see cref="CWEDbContext"/>.
    /// </summary>
    public class CWEDbContextFactory : IDesignTimeDbContextFactory<CWEDbContext>
    {
        /// <inheritdoc/>
        public CWEDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseMySql(
                    configuration["Database"],
                    new MySqlServerVersion(new Version(8, 0, 21)));

            return new CWEDbContext(optionsBuilder.Options);
        }
    }
}
