using Newtonsoft.Json;
using Api.Client.Core.Entity;
using System.Net.Http.Headers;

namespace Api.Client.Core
{
    /// <summary>
    /// Provides a base class for Web API interactions, including handling of HTTP requests with retries, 
    /// and processing of API responses.
    /// </summary>
    /// <typeparam name="T">The type of the arguments used for Web API calls, must implement IWebApiArguments.</typeparam>
    public abstract class WebApiBase<T> : IDisposable
        where T : IWebApiArguments
    {
        public HttpClientWithRetries HttpClientWithRetries { get; set; }
        public string WebApiUrl { get; set; }

        /// <summary>
        /// Indicates whether to retry the request if rate limited.
        /// </summary>
        public bool RetryIfRateLimited => HttpClientWithRetries.RetryIfRateLimited;

        /// <summary>
        /// The delay in milliseconds to wait before retrying if rate limited.
        /// </summary>
        public int DelayInMsIfRateLimit => HttpClientWithRetries.DelayInMsIfRateLimit;

        /// <summary>
        /// Initializes a new instance of the WebApiBase class with the specified URL.
        /// </summary>
        /// <param name="url">The base URL of the Web API.</param>
        public WebApiBase(string url)
        {
            WebApiUrl = url;
            HttpClientWithRetries = new HttpClientWithRetries();
        }

        /// <summary>
        /// Initializes a new instance of the WebApiBase class with the specified URL and API key.
        /// </summary>
        /// <param name="url">The base URL of the Web API.</param>
        /// <param name="bearerToken">The API key for authentication.</param>
        public WebApiBase(string url, string bearerToken)
        {
            WebApiUrl = url;
            HttpClientWithRetries = new HttpClientWithRetries(bearerToken);
        }

        /// <summary>
        /// Initializes a new instance of the WebApiBase class with the specified URL, user, and password.
        /// </summary>
        /// <param name="url">The base URL of the Web API.</param>
        /// <param name="user">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        public WebApiBase(string url, string user, string password)
        {
            WebApiUrl = url;
            HttpClientWithRetries = new HttpClientWithRetries(user, password);
        }

        public async Task<string> GetRawJsonResponse(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            HttpResponseMessage response = await HttpClientWithRetries.SendAsync(request).ConfigureAwait(false);
            WebApiException? error = await CheckForWebApiException(response).ConfigureAwait(false);

            if (error != null)
            {
                return error.ToString();
            }

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseBody;
        }

        protected abstract Uri BuildUriForListRecords(T webApiArguments);

        public static void AddParameterToQuery(ref UriBuilder uriBuilder, string queryToAppend)
        {
            if (string.IsNullOrEmpty(uriBuilder.Query))
            {
                uriBuilder.Query = queryToAppend;
            }
            else
            {
                uriBuilder.Query = $"{uriBuilder.Query[1..]}&{queryToAppend}";
            }
        }

        internal static async Task<string?> ReadResponseErrorMessage(HttpResponseMessage httpResponseMessage)
        {
            string? content = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            InvalidRequestError? json = JsonConvert.DeserializeObject<InvalidRequestError>(content);
            return json?.MessagePart?.ErrorType != null ? json.MessagePart.Message : null;
        }

        public async Task<HttpResponseMessage> SendWebApiRequest(HttpMethod httpMethod, T webApiArguments, object? content = null)
        {
            webApiArguments.CheckArgumentsValidation();
            Uri uri = BuildUriForListRecords(webApiArguments);
            HttpRequestMessage request = new HttpRequestMessage
            { 
                Method = httpMethod,
                RequestUri = uri,
            };

            if (content != null)
            {
                string? serializedObject = JsonConvert.SerializeObject(content).ToLower();
                if (serializedObject != null)
                {
                    Dictionary<string, string>? contentDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedObject);
                    if (contentDictionary != null)
                    {
                        request.Content = new FormUrlEncodedContent(contentDictionary);
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    }
                }
            }

            return await HttpClientWithRetries.SendAsync(request).ConfigureAwait(false);
        }

        public static async Task<WebApiException?> CheckForWebApiException(HttpResponseMessage httpResponseMessage)
        {
            switch (httpResponseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return null;
                case System.Net.HttpStatusCode.NoContent:
                    return null;
                case System.Net.HttpStatusCode.BadRequest:
                    return new WebApiBadRequestException();
                case System.Net.HttpStatusCode.Forbidden:
                    return new WebApiForbiddenException();
                case System.Net.HttpStatusCode.NotFound:
                    return new WebApiNotFoundException();
                case System.Net.HttpStatusCode.PaymentRequired:
                    return new WebApiPaymentRequiredException();
                case System.Net.HttpStatusCode.Unauthorized:
                    return new WebApiUnauthorizedException();
                case System.Net.HttpStatusCode.RequestEntityTooLarge:
                    return new WebApiRequestEntityTooLargeException();
                case (System.Net.HttpStatusCode)422:
                    string? detailedErrorMessage = await ReadResponseErrorMessage(httpResponseMessage).ConfigureAwait(false);
                    return new WebApiInvalidRequestException(detailedErrorMessage);
                case (System.Net.HttpStatusCode)429:
                    return new WebApiTooManyRequestsException();
                case System.Net.HttpStatusCode.GatewayTimeout:
                    return new WebApiGatewayTimeoutException();
                default:
                    throw new WebApiUnrecognizedException(httpResponseMessage.StatusCode);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            HttpClientWithRetries.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
