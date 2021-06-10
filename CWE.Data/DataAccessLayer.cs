namespace CWE.Data
{
    using System.Collections.Generic;
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
    }
}
