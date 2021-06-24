namespace CWE.Data.Context
{
    using CWE.Data.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Implementation of <see cref="DbContext"/> for CWE.
    /// </summary>
    public class CWEDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CWEDbContext"/> class.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions"/> to be injected.</param>
        public CWEDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> containing all campaigns.
        /// </summary>
        public DbSet<Campaign> Campaigns { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> containing all requests.
        /// </summary>
        public DbSet<Request> Requests { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> containing all suggestions.
        /// </summary>
        public DbSet<Suggestion> Suggestions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> containing all tags.
        /// </summary>
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> containing all infractions.
        /// </summary>
        public DbSet<Infraction> Infractions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> containing all the mutes.
        /// </summary>
        public DbSet<Mute> Mutes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> containing all ranks.
        /// </summary>
        public DbSet<Rank> Ranks { get; set; }
    }
}
