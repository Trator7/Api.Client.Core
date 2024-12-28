namespace Api.Client.Core
{
    /// <summary>
    /// Defines a contract for Web API argument classes, ensuring they can validate themselves.
    /// </summary>
    public interface IWebApiArguments
    {
        /// <summary>
        /// Validates the arguments of a Web API call. Implementations should throw exceptions if validation fails.
        /// </summary>
        void CheckArgumentsValidation();
    }

    /// <summary>
    /// Provides a base implementation for Web API argument classes, including basic validation logic.
    /// </summary>
    /// <typeparam name="T">The type of the API call argument.</typeparam>
    public abstract class WebApiArguments<T> : IWebApiArguments
    {
        /// <summary>
        /// Represents the type of API call being made. This should be defined in derived classes.
        /// </summary>
        public T? ApiCallType;

        /// <summary>
        /// Performs basic validation on the API call arguments. Ensures that the ApiCallType is not null.
        /// Derived classes should override this method to implement more specific validation logic.
        /// </summary>
        public virtual void CheckArgumentsValidation()
        {
            if (ApiCallType == null)
            {
                throw new NotImplementedException("ApiCallType must be defined for current API framework.");
            }
        }
    }
}
