using Newtonsoft.Json;

namespace Api.Client.Core.Entity
{
    /// <summary>
    /// Represents a part of a message detailing an error from the Web API.
    /// </summary>
    internal class MessagePart
    {
        /// <summary>
        /// Gets or sets the type of error.
        /// </summary>
        [JsonProperty("type")]
        [JsonRequired]
        public string ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the detailed message describing the error.
        /// </summary>
        [JsonProperty("message")]
        [JsonRequired]
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePart"/> class.
        /// </summary>
        /// <param name="errorType">The type of error.</param>
        /// <param name="message">The detailed error message.</param>
        public MessagePart(string errorType, string message)
        {
            ErrorType = errorType;
            Message = message;
        }
    }
}