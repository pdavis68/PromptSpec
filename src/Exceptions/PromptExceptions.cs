
using System;

namespace PromptSpec.Exceptions
{

    /// <summary>
    /// Exception thrown when prompt validation fails.
    /// </summary>
    public class PromptValidationException : Exception
    {
        public PromptValidationException(string message) : base(message) { }

        public PromptValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// Exception thrown when a required placeholder is missing.
    /// </summary>
    public class MissingPlaceholderException : PromptValidationException
    {
        public string PlaceholderName { get; }

        public MissingPlaceholderException(string placeholderName)
            : base($"Required placeholder '{placeholderName}' is missing.")
        {
            PlaceholderName = placeholderName;
        }
    }


    /// <summary>
    /// Exception thrown when a placeholder value has an incorrect type.
    /// </summary>
    public class PlaceholderTypeException : PromptValidationException
    {
        public string PlaceholderName { get; }
        public string ExpectedType { get; }
        public string ActualType { get; }

        public PlaceholderTypeException(string placeholderName, string expectedType, string actualType)
            : base($"Placeholder '{placeholderName}' expected type '{expectedType}' but received '{actualType}'.")
        {
            PlaceholderName = placeholderName;
            ExpectedType = expectedType;
            ActualType = actualType;
        }
    }

    /// <summary>
    /// Exception thrown when a prompt template is not found.
    /// </summary>
    public class PromptNotFoundException : Exception
    {
        public string PromptName { get; }

        public PromptNotFoundException(string promptName)
            : base($"Prompt template '{promptName}' not found.")
        {
            PromptName = promptName;
        }
    }

}
