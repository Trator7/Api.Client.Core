using System.Net;

namespace Api.Client.Core
{
    /// <summary>
    /// Represents the base class for exceptions thrown by the Web API.
    /// </summary>
    public abstract class WebApiException : Exception
    {
        public readonly HttpStatusCode ErrorCode;
        public readonly string ErrorName;
        public readonly string ErrorMessage;

        /// <summary>
        /// Gets the detailed error message if available.
        /// </summary>
        public string? DetailedErrorMessage { get; protected set; }

        protected WebApiException(HttpStatusCode errorCode, string errorName, string errorMessage) : base($"{errorName} - {errorCode}: {errorMessage}")
        {
            ErrorCode = errorCode;
            ErrorName = errorName;
            ErrorMessage = errorMessage;
            DetailedErrorMessage = null;
        }

        public override string ToString()
        {
            return $"{ErrorName} ({ErrorCode}): {ErrorMessage}";
        }
    }

    /// <summary>
    /// Represents an exception thrown when an unrecognized error occurs.
    /// </summary>
    public class WebApiUnrecognizedException : WebApiException
    {
        public WebApiUnrecognizedException(HttpStatusCode statusCode) : base(statusCode, "Unrecognized Error", $"Web returned HTTP status code {statusCode}")
        {
        }
    }

    /// <summary>
    /// Represents an exception thrown when a bad request is made to the Web API.
    /// </summary>
    public class WebApiBadRequestException : WebApiException
    {
        public WebApiBadRequestException() : base(HttpStatusCode.BadRequest, "Bad Request", "The request encoding is invalid; the request can't be parsed as a valid JSON.")
        {
        }
    }

    /// <summary>
    /// Represents an exception thrown when unauthorized access is attempted.
    /// </summary>
    public class WebApiUnauthorizedException : WebApiException
    {
        public WebApiUnauthorizedException() : base(HttpStatusCode.Unauthorized, "Unauthorized", "Accessing a protected resource without authorization or with invalid credentials.")
        {
        }
    }

    /// <summary>
    /// Represents an exception thrown when payment is required.
    /// </summary>
    public class WebApiPaymentRequiredException : WebApiException
    {
        public WebApiPaymentRequiredException() : base(
            HttpStatusCode.PaymentRequired,
            "Payment Required",
            "The account associated with the API key making requests hits a quota that can be increased by upgrading the Web account plan.")
        {
        }
    }

    /// <summary>
    /// Represents an exception thrown when access to a resource is forbidden.
    /// </summary>
    public class WebApiForbiddenException : WebApiException
    {
        public WebApiForbiddenException() : base(
            HttpStatusCode.Forbidden,
            "Forbidden",
            "Accessing a protected resource with API credentials that don't have access to that resource.")
        {
        }
    }

    /// <summary>
    /// Represents an exception thrown when a requested resource is not found.
    /// </summary>
    public class WebApiNotFoundException : WebApiException
    {
        public WebApiNotFoundException() : base(
            HttpStatusCode.NotFound,
            "Not Found",
            "Route or resource is not found. This error is returned when the request hits an undefined route, or if the resource doesn't exist (e.g. has been deleted).")
        {
        }
    }

    /// <summary>
    /// Represents an exception thrown when the request entity is too large.
    /// </summary>
    public class WebApiRequestEntityTooLargeException : WebApiException
    {
        public WebApiRequestEntityTooLargeException() : base(
            HttpStatusCode.RequestEntityTooLarge,
            "Request Entity Too Large",
            "The request exceeded the maximum allowed payload size. You shouldn't encounter this under normal use.")
        {
        }
    }

    /// <summary>
    /// Represents an exception thrown when a request is invalid.
    /// </summary>
    public class WebApiInvalidRequestException : WebApiException
    {
        public WebApiInvalidRequestException(string? detailedErrorMessage = null) : base(
            (HttpStatusCode)422,
            "Invalid Request",
            "The request data is invalid. This includes most of the base-specific validations.\nThe DetailedErrorMessage property contains the detailed error message string.")
        {
            DetailedErrorMessage = detailedErrorMessage;
        }
    }

    /// <summary>
    /// Represents an exception thrown due to too many requests being sent in a given amount of time.
    /// </summary>
    public class WebApiTooManyRequestsException : WebApiException
    {
        public WebApiTooManyRequestsException() : base(
            (HttpStatusCode)429,
            "Too Many Requests",
            "The user has sent too many requests in a given amount of time (rate limiting).")
        {
        }
    }


    /// <summary>
    /// Represents an exception thrown when a bad request is made to the Web API.
    /// </summary>
    public class WebApiGatewayTimeoutException : WebApiException
    {
        public WebApiGatewayTimeoutException() : base(HttpStatusCode.GatewayTimeout, "Gateway Timeout", "The server, while acting as a gateway or proxy, did not receive a timely response from an upstream server it needed to access in order to complete the request.")
        {
        }
    }
}