using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.ComponentModel;

namespace Api.Client.Core
{
    /// <summary>
    /// Provides a HttpClient with automatic retries on rate limit errors (HTTP 429).
    /// </summary>
    public class HttpClientWithRetries : IDisposable
    {
        public const int MAX_RETRIES = 3;
        public const int DEFAULT_RETRY_DELAY = 2000;

        /// <summary>
        /// Gets or sets a value indicating whether to retry if a rate limit error (HTTP 429) is encountered.
        /// </summary>
        public bool RetryIfRateLimited { get; set; }

        private int delayInMsIfRateLimit;

        /// <summary>
        /// Gets or sets the delay in milliseconds to wait before retrying after a rate limit error.
        /// Throws a WarningException if set to less than the default retry delay.
        /// </summary>
        public int DelayInMsIfRateLimit
        {
            get { return delayInMsIfRateLimit; }

            set
            {
                if (value < DEFAULT_RETRY_DELAY)
                {
                    throw new WarningException($"Retry Delay shouldn't be less than {DEFAULT_RETRY_DELAY} ms.", "DelayInMsIfRateLimit");
                }

                delayInMsIfRateLimit = value;
            }
        }

        private readonly HttpClient client;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientWithRetries"/> class.
        /// </summary>
        public HttpClientWithRetries()
        {
            RetryIfRateLimited = true;
            DelayInMsIfRateLimit = DEFAULT_RETRY_DELAY;

            client = new HttpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientWithRetries"/> class with an API key for authorization.
        /// </summary>
        /// <param name="bearerToken">The API key for authorization.</param>
        public HttpClientWithRetries(string bearerToken) : this()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientWithRetries"/> class with a username and password for basic authentication.
        /// </summary>
        /// <param name="user">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        public HttpClientWithRetries(string user, string password) : this()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}")));
        }

        #endregion

        /// <summary>
        /// Adds a header to the HttpClient.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void AddHeader(string name, string value)
        {
            client.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Sends an HTTP request asynchronously, with retries on rate limit errors.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <returns>The HTTP response message.</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            string? content = null;

            if (request.Content != null)
            {
                content = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

            int dueTimeDelay = DelayInMsIfRateLimit;
            int retries = 0;
            while (
                RetryIfRateLimited &&
                request.RequestUri != null &&
                response.StatusCode == (HttpStatusCode)429 &&
                retries < MAX_RETRIES)
            {
                await Task.Delay(dueTimeDelay).ConfigureAwait(false);
                HttpRequestMessage requestRegenerated = RegenerateRequest(request.Method, request.RequestUri, content);
                response = await client.SendAsync(requestRegenerated).ConfigureAwait(false);
                retries++;
                dueTimeDelay *= 2;
            }

            return response;
        }

        /// <summary>
        /// Regenerates an HTTP request message with the same content, method, and request URI.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="content">The request content as a string.</param>
        /// <returns>A new HttpRequestMessage.</returns>
        private static HttpRequestMessage RegenerateRequest(HttpMethod method, Uri requestUri, string? content)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            if (content != null)
            {
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }
            return request;
        }

        /// <summary>
        /// Releases the unmanaged resources and disposes of the managed resources used by the HttpClient.
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
        }
    }
}