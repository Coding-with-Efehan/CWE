namespace CWE.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Data.Context;
    using CWE.Data.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The data access layer used by CWE.
    /// </summary>
    public class DataAccessLayer
    {
        private readonly IDbContextFactory<CWEDbContext> _contextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessLayer"/> class.
        /// </summary>
        /// <param name="contextFactory">The <see cref="IDbContextFactory{T}"/> to be injected.</param>
        public DataAccessLayer(IDbContextFactory<CWEDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Create a new request.
        /// </summary>
        /// <param name="request">The request that should be created.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task CreateRequest(Request request)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Add(request);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the state of an existing request.
        /// </summary>
        /// <param name="messageId">The ID of the message of the request to be updated.</param>
        /// <param name="state">The new <see cref="RequestState"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateRequest(ulong messageId, RequestState state)
        {
            using var context = _contextFactory.CreateDbContext();
            var request = await context.Requests
                .FirstOrDefaultAsync(x => x.MessageId == messageId);

            if (request == null)
            {
                return;
            }

            request.State = state;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a request.
        /// </summary>
        /// <param name="messageId">The ID of the message of the request to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task DeleteRequest(ulong messageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var request = await context.Requests
                .FirstOrDefaultAsync(x => x.MessageId == messageId);

            if (request == null)
            {
                return;
            }

            context.Remove(request);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Get a request based on its message ID.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<Request> GetRequest(ulong messageId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Requests
                .FirstOrDefaultAsync(x => x.MessageId == messageId);
        }

        /// <summary>
        /// Get all requests.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Request>> GetRequests()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Requests
                .ToListAsync();
        }

        /// <summary>
        /// Get a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to get.</param>
        /// <returns>A <see cref="Tag"/> depending on if the tag exists in the database.</returns>
        public async Task<Tag> GetTag(string tagName)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);
        }

        /// <summary>
        /// Get all tags.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Tag>> GetTags()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tags
                .ToListAsync();
        }

        /// <summary>
        /// Create a new tag.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <param name="ownerId">The ID of the author.</param>
        /// <param name="content">The content of the tag.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateTag(string name, ulong ownerId, string content)
        {
            using var context = _contextFactory.CreateDbContext();
            var tag = await context.Tags
                .FirstOrDefaultAsync(x => x.Name == name);

            if (tag != null)
            {
                return;
            }

            context.Add(new Tag
            {
                Name = name,
                OwnerId = ownerId,
                Content = content,
            });

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteTag(string tagName)
        {
            using var context = _contextFactory.CreateDbContext();
            var tag = await context.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);

            if (tag == null)
            {
                return;
            }

            context.Remove(tag);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Modify the content of a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="content">The new content.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EditTagContent(string tagName, string content)
        {
            using var context = _contextFactory.CreateDbContext();
            var tag = await context.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);

            if (tag == null)
            {
                return;
            }

            tag.Content = content;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Modify the owner of a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to transfer.</param>
        /// <param name="ownerId">The ID of the new owner.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EditTagOwner(string tagName, ulong ownerId)
        {
            using var context = _contextFactory.CreateDbContext();
            var tag = await context.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);

            if (tag == null)
            {
                return;
            }

            tag.OwnerId = ownerId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Get a suggestion based upon its ID.
        /// </summary>
        /// <param name="id">The ID of the suggestion.</param>
        /// <returns>A <see cref="Suggestion"/> depending on if the tag exists in the database.</returns>
        public async Task<Suggestion> GetSuggestion(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Suggestions
                .FindAsync(id);
        }

        /// <summary>
        /// Create a new suggestion.
        /// </summary>
        /// <param name="initiator">The ID of the initiator.</param>
        /// <param name="messageId">The ID of the suggestion its message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<int> CreateSuggestion(ulong initiator, ulong messageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var entityEntry = context.Add(new Suggestion
            {
                Initiator = initiator,
                MessageId = messageId,
            });

            await context.SaveChangesAsync();
            return entityEntry.Entity.Id;
        }

        /// <summary>
        /// Update the state of an existing suggestion.
        /// </summary>
        /// <param name="id">The ID of the suggestion.</param>
        /// <param name="state">The new <see cref="SuggestionState"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateSuggestion(int id, SuggestionState state)
        {
            using var context = _contextFactory.CreateDbContext();
            var suggestion = await context.Suggestions
                .FindAsync(id);

            if (suggestion == null)
            {
                return;
            }

            suggestion.State = state;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a suggestion.
        /// </summary>
        /// <param name="id">The ID of the suggestion.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteSuggestion(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var suggestion = await context.Suggestions
                .FindAsync(id);

            if (suggestion == null)
            {
                return;
            }

            context.Remove(suggestion);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Create a rank.
        /// </summary>
        /// <param name="id">The ID of the rank.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateRank(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();
            var rank = await context.Ranks
                .FindAsync(id);

            if (rank != null)
            {
                return;
            }

            context.Add(new Rank { Id = id });
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Get all ranks.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<Rank>> GetRanks()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Ranks
                .ToListAsync();
        }

        /// <summary>
        /// Delete a rank.
        /// </summary>
        /// <param name="id">The ID of the rank.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteRank(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();
            var rank = await context.Ranks
                .FindAsync(id);

            if (rank == null)
            {
                return;
            }

            context.Remove(rank);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Create an auto-role.
        /// </summary>
        /// <param name="id">The ID of the auto-role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateAutoRole(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();
            var autoRole = await context.AutoRoles
                .FindAsync(id);

            if (autoRole != null)
            {
                return;
            }

            context.Add(new AutoRole { Id = id });
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Get all auto-roles.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<AutoRole>> GetAutoRoles()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.AutoRoles
                .ToListAsync();
        }

        /// <summary>
        /// Delete an auto-role.
        /// </summary>
        /// <param name="id">The ID of the auto-role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteAutoRole(ulong id)
        {
            using var context = _contextFactory.CreateDbContext();
            var autoRole = await context.AutoRoles
                .FindAsync(id);

            if (autoRole == null)
            {
                return;
            }

            context.Remove(autoRole);
            await context.SaveChangesAsync();
        }
    }
}
