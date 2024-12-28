using Newtonsoft.Json;

namespace Api.Client.Core.Entity
{
    /// <summary>
    /// Represents an error that occurs due to an invalid request to the Web API.
    /// This class is used to encapsulate the error details received from the API.
    /// </summary>
    internal class InvalidRequestError
    {
        /// <summary>
        /// Gets or sets the message part that contains the error details.
        /// </summary>
        [JsonProperty("error")]
        [JsonRequired]
        public MessagePart MessagePart { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRequestError"/> class with the specified message part.
        /// </summary>
        /// <param name="messagePart">The message part containing the error details.</param>
        public InvalidRequestError(MessagePart messagePart)
        {
            MessagePart = messagePart;
        }
    }
}