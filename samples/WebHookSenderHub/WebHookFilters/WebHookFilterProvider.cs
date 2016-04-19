using Microsoft.AspNet.WebHooks;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WebHookSenderHub.WebHookFilters
{
    /// <summary>
    /// The <see cref="WebHookFilterProvider"/> adds custom filter besides the default *
    /// </summary>
    public class WebHookFilterProvider : IWebHookFilterProvider
    {
        /// <summary>
        /// Use a IWebHookFilterProvider implementation to describe the events that users can 
        /// subscribe to. A wildcard is always registered meaning that users can register for 
        /// "all events". It is possible to have 0, 1, or more IWebHookFilterProvider 
        /// implementations.
        /// </summary>
        private readonly Collection<WebHookFilter> filters = new Collection<WebHookFilter>
        {
            new WebHookFilter { Name = "Insert", Description = "New entity added"},
            new WebHookFilter { Name = "Update", Description = "Entity updated"},
            new WebHookFilter { Name = "Delete", Description = "Entity deleted"}
        };

        /// <summary>
        ///  This sends the all available filters
        /// </summary>
        /// <returns>Collection of WebHookFilter</returns>
        public Task<Collection<WebHookFilter>> GetFiltersAsync()
        {
            return Task.FromResult(this.filters);
        }
    }
}