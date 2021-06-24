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
        private readonly CWEDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessLayer"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="CWEDbContext"/> to be injected.</param>
        public DataAccessLayer(CWEDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Get all campaigns.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Campaign>> GetCampaigns()
        {
            return await this.dbContext.Campaigns
                .ToListAsync();
        }

        /// <summary>
        /// Create a new campaign.
        /// </summary>
        /// <param name="campaign">The campaign that should be created.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateCampaign(Campaign campaign)
        {
            this.dbContext.Add(campaign);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a campaign.
        /// </summary>
        /// <param name="id">The ID of the campaign to be removed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task DeleteCampaign(ulong id)
        {
            var campaign = await this.dbContext.Campaigns
                .FirstOrDefaultAsync(x => x.User == id);

            if (campaign == null)
            {
                return;
            }

            this.dbContext.Remove(campaign);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Create a new request.
        /// </summary>
        /// <param name="request">The request that should be created.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task CreateRequest(Request request)
        {
            this.dbContext.Add(request);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Update the state of an existing request.
        /// </summary>
        /// <param name="messageId">The ID of the message of the request to be updated.</param>
        /// <param name="state">The new <see cref="RequestState"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateRequest(ulong messageId, RequestState state)
        {
            var request = await this.dbContext.Requests
                .FirstOrDefaultAsync(x => x.MessageId == messageId);

            if (request == null)
            {
                return;
            }

            request.State = state;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a request.
        /// </summary>
        /// <param name="messageId">The ID of the message of the request to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task DeleteRequest(ulong messageId)
        {
            var request = await this.dbContext.Requests
                .FirstOrDefaultAsync(x => x.MessageId == messageId);

            if (request == null)
            {
                return;
            }

            this.dbContext.Remove(request);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get a request based on its message ID.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<Request> GetRequest(ulong messageId)
        {
            return await this.dbContext.Requests
                .FirstOrDefaultAsync(x => x.MessageId == messageId);
        }

        /// <summary>
        /// Get all requests.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Request>> GetRequests()
        {
            return await this.dbContext.Requests
                .ToListAsync();
        }

        /// <summary>
        /// Get a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to get.</param>
        /// <returns>A <see cref="Tag"/> depending on if the tag exists in the database.</returns>
        public async Task<Tag> GetTag(string tagName)
        {
            return await this.dbContext.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);
        }

        /// <summary>
        /// Get all tags.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Tag>> GetTags()
        {
            return await this.dbContext.Tags
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
            var tag = await this.dbContext.Tags
                .FirstOrDefaultAsync(x => x.Name == name);

            if (tag != null)
            {
                return;
            }

            this.dbContext.Add(new Tag
            {
                Name = name,
                OwnerId = ownerId,
                Content = content,
            });

            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteTag(string tagName)
        {
            var tag = await this.dbContext.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);

            if (tag == null)
            {
                return;
            }

            this.dbContext.Remove(tag);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Modify the content of a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="content">The new content.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EditTagContent(string tagName, string content)
        {
            var tag = await this.dbContext.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);

            if (tag == null)
            {
                return;
            }

            tag.Content = content;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Modify the owner of a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to transfer.</param>
        /// <param name="ownerId">The ID of the new owner.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EditTagOwner(string tagName, ulong ownerId)
        {
            var tag = await this.dbContext.Tags
                .FirstOrDefaultAsync(x => x.Name == tagName);

            if (tag == null)
            {
                return;
            }

            tag.OwnerId = ownerId;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get a suggestion based upon its ID.
        /// </summary>
        /// <param name="id">The ID of the suggestion.</param>
        /// <returns>A <see cref="Suggestion"/> depending on if the tag exists in the database.</returns>
        public async Task<Suggestion> GetSuggestion(int id)
        {
            return await this.dbContext.Suggestions
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
            var entityEntry = this.dbContext.Add(new Suggestion
            {
                Initiator = initiator,
                MessageId = messageId,
            });

            await this.dbContext.SaveChangesAsync();
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
            var suggestion = await this.dbContext.Suggestions
                .FindAsync(id);

            if (suggestion == null)
            {
                return;
            }

            suggestion.State = state;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a suggestion.
        /// </summary>
        /// <param name="id">The ID of the suggestion.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteSuggestion(int id)
        {
            var suggestion = await this.dbContext.Suggestions
                .FindAsync(id);

            if (suggestion == null)
            {
                return;
            }

            this.dbContext.Remove(suggestion);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Create a rank.
        /// </summary>
        /// <param name="id">The ID of the rank.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateRank(ulong id)
        {
            var rank = await this.dbContext.Ranks
                .FindAsync(id);

            if (rank != null)
            {
                return;
            }

            this.dbContext.Add(new Rank { Id = id });
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get all ranks.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<Rank>> GetRanks()
        {
            return await this.dbContext.Ranks
                .ToListAsync();
        }

        /// <summary>
        /// Delete a rank.
        /// </summary>
        /// <param name="id">The ID of the rank.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteRank(ulong id)
        {
            var rank = await this.dbContext.Ranks
                .FindAsync(id);

            if (rank == null)
            {
                return;
            }

            this.dbContext.Remove(rank);
            await this.dbContext.SaveChangesAsync();
        }
    }
}
