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
        /// Checks if the tag name provided exists in the database.
        /// </summary>
        /// <param name="tagName">The name of the tag to fetch..</param>
        /// <returns>A <see cref="Tag"/> depending on if the tag exists in the database.</returns>
        public async Task<Tag> FetchTagAsync(string tagName)
        {
            var tag = await this.dbContext.Tags
                .Where(x => x.Name == tagName)
                .SingleOrDefaultAsync();

            if (tag == null)
            {
                throw new ArgumentException("The name provided does not match an existing tag in the database.");
            }

            return tag;
        }

        /// <summary>
        /// Creates a tag.
        /// </summary>
        /// <param name="name">The name that the tag should have.</param>
        /// <param name="ownerId">The owner's user ID of the tag.</param>
        /// <param name="content">The content that the tag holds.</param>
        /// <returns><see cref="void"/>.</returns>
        public async Task CreateTagAsync(string name, ulong ownerId, string content)
        {
            var tag = await this.dbContext.Tags
                .Where(x => x.Name == name)
                .AnyAsync();
            if (tag)
            {
                throw new Exception("A tag with this name already exists.");
            }

            this.dbContext.Tags.Add(new Tag
            {
                Name = name,
                OwnerId = ownerId,
                Content = content,
            });
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a tag from the database.
        /// </summary>
        /// <param name="tagName">The name of the tag to delete.</param>
        /// <returns><see cref="void"/>.</returns>
        public async Task DeleteTagAsync(string tagName)
        {
            var tag = await this.dbContext.Tags
                .Where(x => x.Name == tagName)
                .SingleOrDefaultAsync();

            if (tag == null)
            {
                throw new ArgumentException("The tag provided was not found in the database.");
            }

            this.dbContext.Tags.Remove(tag);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Edits the content of a tag, and modifies it to the new content.
        /// </summary>
        /// <param name="tagName">The tag to edit.</param>
        /// <param name="executorId">The user who is attempting to modify the tag.</param>
        /// <param name="newContent">The content that the new tag should hold.</param>
        /// <returns><see cref="void"/>.</returns>
        public async Task EditTagContentAsync(string tagName, ulong executorId, string newContent)
        {
            var tag = await this.dbContext.Tags
                .Where(x => x.Name == tagName)
                .SingleOrDefaultAsync();
            if (tag == null)
            {
                throw new ArgumentException("The tag provided does not match any currently in the database.");
            }

            if (tag.OwnerId != executorId)
            {
                throw new InvalidOperationException("You cannot modify a tag that you do not own.");
            }

            tag.Content = newContent;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Transfers the ownership of the current tag to a new specified owner.
        /// </summary>
        /// <param name="tagName">The name of the tag to transfer.</param>
        /// <param name="executorId">The person attempting to transfer ownership of the tag.</param>
        /// <param name="newOwnerId">The new owner of the tag's ID.</param>
        /// <returns><see cref="void"/>.</returns>
        public async Task TransferTagOwnershipAsync(string tagName, ulong executorId, ulong newOwnerId)
        {
            var tag = await this.dbContext.Tags
                .Where(x => x.Name == tagName)
                .SingleOrDefaultAsync();
            if (tag == null)
            {
                throw new ArgumentException("The tag provided does not match any inside the database.");
            }

            if (tag.OwnerId != executorId)
            {
                throw new InvalidOperationException("You do not own this tag, so I cannot transfer ownership.");
            }

            tag.OwnerId = newOwnerId;
            await this.dbContext.SaveChangesAsync();
        }
    }
}
