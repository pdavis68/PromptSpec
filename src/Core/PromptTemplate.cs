using System.Text.RegularExpressions;
using PromptSpec.Models;
using PromptSpec.Exceptions;

namespace PromptSpec.Core;

/// <summary>
/// Represents a loaded prompt template with methods to generate the final prompt string.
/// </summary>
public class PromptTemplate
{
    private readonly PromptSpecification _specification;
    private static readonly Regex PlaceholderRegex = new(@"\{([^}]+)\}", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the PromptTemplate class.
    /// </summary>
    /// <param name="specification">The prompt specification.</param>
    public PromptTemplate(PromptSpecification specification)
    {
        _specification = specification ?? throw new ArgumentNullException(nameof(specification));
    }

    /// <summary>
    /// Gets the name of the prompt template.
    /// </summary>
    public string Name => _specification.Name;

    /// <summary>
    /// Gets the version of the prompt template.
    /// </summary>
    public string? Version => _specification.Version;

    /// <summary>
    /// Gets the description of the prompt template.
    /// </summary>
    public string? Description => _specification.Description;

    /// <summary>
    /// Gets the system message for the prompt template.
    /// </summary>
    public string? SystemMessage => _specification.SystemMessage;

    /// <summary>
    /// Gets the output format for the prompt template.
    /// </summary>
    public string? OutputFormat => _specification.OutputFormat;

    /// <summary>
    /// Fills the template with the provided replacement values and performs validation.
    /// </summary>
    /// <param name="replacements">Dictionary containing placeholder values.</param>
    /// <returns>The generated prompt string with placeholders replaced.</returns>
    /// <exception cref="MissingPlaceholderException">Thrown when a required placeholder is missing.</exception>
    /// <exception cref="PlaceholderTypeException">Thrown when a placeholder value has an incorrect type.</exception>
    public string GeneratePrompt(Dictionary<string, object> replacements)
    {
        ArgumentNullException.ThrowIfNull(replacements);

        // Validate placeholders if definitions exist
        if (_specification.Placeholders != null)
        {
            ValidatePlaceholders(replacements);
        }

        // Replace placeholders in the template
        return PlaceholderRegex.Replace(_specification.Template, match =>
        {
            var placeholderName = match.Groups[1].Value;
            
            if (replacements.TryGetValue(placeholderName, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }

            // If placeholder is not found and not required, return empty string
            if (_specification.Placeholders?.TryGetValue(placeholderName, out var definition) == true)
            {
                if (definition.Required)
                {
                    throw new MissingPlaceholderException(placeholderName);
                }
            }

            return string.Empty;
        });
    }

    /// <summary>
    /// Returns the LLM parameters for this prompt template.
    /// </summary>
    /// <returns>LLMParameters object containing temperature, topP, etc.</returns>
    public LLMParameters GetParameters()
    {
        return _specification.Parameters ?? new LLMParameters();
    }

    /// <summary>
    /// Returns the model-specific configuration parameters.
    /// </summary>
    /// <returns>Dictionary of model-specific parameters.</returns>
    public Dictionary<string, object> GetModelConfig()
    {
        return _specification.ModelConfig ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets the raw template string.
    /// </summary>
    /// <returns>The template string with placeholders.</returns>
    public string GetTemplate()
    {
        return _specification.Template;
    }

    /// <summary>
    /// Gets all placeholder names found in the template.
    /// </summary>
    /// <returns>List of placeholder names.</returns>
    public List<string> GetPlaceholderNames()
    {
        var matches = PlaceholderRegex.Matches(_specification.Template);
        return matches.Cast<Match>()
                     .Select(m => m.Groups[1].Value)
                     .Distinct()
                     .ToList();
    }

    /// <summary>
    /// Validates that the provided replacements match the placeholder definitions.
    /// </summary>
    /// <param name="replacements">The replacement values to validate.</param>
    private void ValidatePlaceholders(Dictionary<string, object> replacements)
    {
        if (_specification.Placeholders == null) return;

        foreach (var (placeholderName, definition) in _specification.Placeholders)
        {
            // Check if required placeholder is missing
            if (definition.Required && !replacements.ContainsKey(placeholderName))
            {
                throw new MissingPlaceholderException(placeholderName);
            }

            // Validate type if placeholder is provided
            if (replacements.TryGetValue(placeholderName, out var value) && value != null)
            {
                ValidateType(placeholderName, value, definition.Type);
            }
        }
    }

    /// <summary>
    /// Validates that a value matches the expected type.
    /// </summary>
    /// <param name="placeholderName">Name of the placeholder being validated.</param>
    /// <param name="value">The value to validate.</param>
    /// <param name="expectedType">The expected type (string, number, boolean).</param>
    private static void ValidateType(string placeholderName, object value, string expectedType)
    {
        var actualType = GetValueType(value);

        if (!string.Equals(expectedType, actualType, StringComparison.OrdinalIgnoreCase))
        {
            throw new PlaceholderTypeException(placeholderName, expectedType, actualType);
        }
    }

    /// <summary>
    /// Determines the type name of a value for validation purposes.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>The type name (string, number, boolean).</returns>
    private static string GetValueType(object value)
    {
        return value switch
        {
            string => "string",
            int or long or float or double or decimal => "number",
            bool => "boolean",
            _ => "string" // Default to string for other types
        };
    }
}