using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OWebHooks
{
    /// <summary>
    /// Provides an <see cref="HttpMessageHandler"/> where the response can be modified in flight by providing
    /// an appropriate <see cref="M:Handler"/>.
    /// </summary>
    public class HttpMessageHandlerMock : HttpMessageHandler
    {
        /// <summary>
        /// The handler which will process the <see cref="HttpRequestMessage"/> and return an <see cref="HttpResponseMessage"/>.
        /// </summary>
        public Func<HttpRequestMessage, Task<HttpResponseMessage>> Handler { get; set; }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Handler(request);
        }
    }
}