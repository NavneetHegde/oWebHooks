using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace WebHookSenderHub.Controllers
{
    /// <summary>
    /// The <see cref="RegistrationsController"/> allows the caller to create, modify, and manage WebHooks registration
    /// </summary>
    /// <remarks>TODO Model validation on PUT and POST</remarks>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RegistrationsController : ApiController
    {
        private IWebHookStore _store;
        private ILogger _logger;

        // TODO : Work on getting authenticated user
        private string _userId = "testuser";

        /// <summary>
        /// Gets all registered WebHooks for a given user.
        /// </summary>
        /// <returns>A collection containing the registered <see cref="WebHook"/> instances for a given user.</returns>
        public async Task<IEnumerable<WebHook>> Get()
        {
            string userId = _userId;
            IEnumerable<WebHook> webHooks = await _store.GetAllWebHooksAsync(userId);
            return webHooks;
        }

        /// <summary>
        /// Gets all registered WebHooks for a given user.
        /// </summary>
        /// <returns>A collection containing the registered <see cref="WebHook"/> instances for a given user.</returns>
        public async Task<WebHook> Get(string id)
        {
            string userId = _userId;
            WebHook webHooks = await _store.LookupWebHookAsync(userId, id);
            return webHooks;
        }

        /// <summary>
        /// Registers a new WebHook for a given user.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to create.</param>
        [ResponseType(typeof(WebHook))]
        public async Task<IHttpActionResult> Post(WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }

            string userId = _userId;
            await VerifyFilters(webHook);

            try
            {
                // Ensure we have a normalized ID for the WebHook
                webHook.Id = null;

                // Add WebHook for this user.
                StoreResult result = await _store.InsertWebHookAsync(userId, webHook);
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ex.Message);
                _logger.Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }


        /// <summary>
        /// Updates an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        /// <param name="webHook">The new <see cref="WebHook"/> to use.</param>
        [HttpPut]
        public async Task<IHttpActionResult> Put(string id, WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }
            if (!string.Equals(id, webHook.Id, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            string userId = _userId;
            await VerifyFilters(webHook);

            try
            {
                StoreResult result = await _store.UpdateWebHookAsync(userId, webHook);
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ex.Message);
                _logger.Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }


        /// <summary>
        /// Deletes an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        public async Task<IHttpActionResult> Delete(string id)
        {
            string userId = _userId;

            try
            {
                StoreResult result = await _store.DeleteWebHookAsync(userId, id);
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ex.Message);
                _logger.Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }

        /// <summary>
        /// Deletes all existing WebHook registrations.
        /// </summary>
        public async Task<IHttpActionResult> DeleteAll()
        {
            string userId = _userId;

            try
            {
                await _store.DeleteAllWebHooksAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ex.Message);
                _logger.Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            _logger = Configuration.DependencyResolver.GetLogger();
            _store = Configuration.DependencyResolver.GetStore();
        }

        /// <summary>
        /// Creates an <see cref="IHttpActionResult"/> based on the provided <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The result to use when creating the <see cref="IHttpActionResult"/>.</param>
        /// <returns>An initialized <see cref="IHttpActionResult"/>.</returns>
        protected IHttpActionResult CreateHttpResult(StoreResult result)
        {
            switch (result)
            {
                case StoreResult.Success:
                    return Ok();

                case StoreResult.Conflict:
                    return Conflict();

                case StoreResult.NotFound:
                    return NotFound();

                case StoreResult.OperationError:
                    return BadRequest();

                default:
                    return InternalServerError();
            }
        }

        /// <summary>
        /// Ensure that the provided <paramref name="webHook"/> only has registered filters.
        /// </summary>
        protected virtual async Task VerifyFilters(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            // If there are no filters then add our wildcard filter.
            if (webHook.Filters.Count == 0)
            {
                webHook.Filters.Add(WildcardWebHookFilterProvider.Name);
                return;
            }

            IWebHookFilterManager filterManager = Configuration.DependencyResolver.GetFilterManager();
            IDictionary<string, WebHookFilter> filters = await filterManager.GetAllWebHookFiltersAsync();
            HashSet<string> normalizedFilters = new HashSet<string>();
            List<string> invalidFilters = new List<string>();
            foreach (string filter in webHook.Filters)
            {
                WebHookFilter hookFilter;
                if (filters.TryGetValue(filter, out hookFilter))
                {
                    normalizedFilters.Add(hookFilter.Name);
                }
                else
                {
                    invalidFilters.Add(filter);
                }
            }

            if (invalidFilters.Count > 0)
            {
                string invalidFiltersMsg = string.Join(", ", invalidFilters);
                string msg = string.Format(CultureInfo.CurrentCulture, invalidFiltersMsg);
                _logger.Info(msg);

                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(response);
            }
            else
            {
                webHook.Filters.Clear();
                foreach (string filter in normalizedFilters)
                {
                    webHook.Filters.Add(filter);
                }
            }
        }
    }
}
