using Newtonsoft.Json;

namespace Api.Client.Core
{
    /// <summary>
    /// Represents the base response from a Web API call.
    /// </summary>
    public class WebApiResponse
    {
        /// <summary>
        /// Gets a value indicating whether the API call was successful.
        /// </summary>
        public bool Success { get; protected set; }

        /// <summary>
        /// Gets the Web API error, if any occurred during the API call.
        /// </summary>
        public WebApiException? WebApiError { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiResponse"/> class, indicating a successful response.
        /// </summary>
        public WebApiResponse()
        {
            Success = true;
            WebApiError = default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiResponse"/> class with a specified error, indicating an unsuccessful response.
        /// </summary>
        /// <param name="webApiException">The Web API exception that occurred.</param>
        public WebApiResponse(WebApiException webApiException)
        {
            Success = false;
            WebApiError = webApiException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiResponse"/> class with a specified success value and error.
        /// </summary>
        /// <param name="success">Indicates whether the API call was successful.</param>
        /// <param name="webApiException">The Web API exception that occurred, if any.</param>
        public WebApiResponse(bool success, WebApiException webApiException)
        {
            Success = success;
            WebApiError = webApiException;
        }
    }

    /// <summary>
    /// Represents the response from a Web API call with a specific type of data.
    /// </summary>
    /// <typeparam name="T">The type of the data returned by the API call.</typeparam>
    public class WebApiResponse<T> : WebApiResponse
    {
        /// <summary>
        /// Gets the data records returned by the API call.
        /// </summary>
        public T? Records { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiResponse{T}"/> class with the specified records, indicating a successful response.
        /// </summary>
        /// <param name="records">The data records returned by the API call.</param>
        public WebApiResponse(T records) : base()
        {
            Records = records;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiResponse{T}"/> class with a specified error, indicating an unsuccessful response.
        /// </summary>
        /// <param name="webApiException">The Web API exception that occurred.</param>
        public WebApiResponse(WebApiException webApiException) : base(webApiException)
        {
            Records = default;
        }

        public bool SaveJsonFile(string path)
        {
            try
            {
                string json = JsonConvert.SerializeObject(Records, Formatting.Indented);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
