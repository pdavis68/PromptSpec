using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PromptSpec.Models;
using PromptSpec.Exceptions;

namespace PromptSpec.Core;

/// <summary>
/// Manages loading and retrieving prompt templates from YAML configuration.
/// </summary>
public class PromptManager
{
    private readonly Dictionary<string, PromptTemplate> _templates = new();
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of the PromptManager class.
    /// </summary>
    public PromptManager()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Gets the count of loaded templates.
    /// </summary>
    public int TemplateCount => _templates.Count;

    /// <summary>
    /// Gets the names of all loaded templates.
    /// </summary>
    public IEnumerable<string> TemplateNames => _templates.Keys;

    /// <summary>
    /// Parses the YAML content and loads all prompts into memory.
    /// </summary>
    /// <param name="yamlContent">The YAML content containing prompt definitions.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when yamlContent is null.</exception>
    /// <exception cref="PromptValidationException">Thrown when YAML parsing or validation fails.</exception>
    public async Task LoadTemplatesAsync(string yamlContent)
    {
        ArgumentException.ThrowIfNullOrEmpty(yamlContent);

        try
        {
            await Task.Run(() =>
            {
                var promptCollection = _deserializer.Deserialize<PromptCollection>(yamlContent);
                
                if (promptCollection?.Prompts == null)
                {
                    throw new PromptValidationException("Invalid YAML: No 'prompts' section found or it is null.");
                }

                _templates.Clear();

                foreach (var specification in promptCollection.Prompts)
                {
                    ValidateSpecification(specification);
                    
                    if (_templates.ContainsKey(specification.Name))
                    {
                        throw new PromptValidationException($"Duplicate prompt name '{specification.Name}' found.");
                    }

                    _templates[specification.Name] = new PromptTemplate(specification);
                }
            });
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new PromptValidationException("Failed to parse YAML content.", ex);
        }
        catch (Exception ex) when (ex is not PromptValidationException)
        {
            throw new PromptValidationException("An error occurred while loading templates.", ex);
        }
    }

    /// <summary>
    /// Loads templates from a YAML file.
    /// </summary>
    /// <param name="filePath">Path to the YAML file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file is not found.</exception>
    /// <exception cref="PromptValidationException">Thrown when YAML parsing or validation fails.</exception>
    public async Task LoadTemplatesFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Template file not found: {filePath}");
        }

        var yamlContent = await File.ReadAllTextAsync(filePath);
        await LoadTemplatesAsync(yamlContent);
    }

    /// <summary>
    /// Retrieves a specific prompt template by its name.
    /// </summary>
    /// <param name="promptName">The name of the prompt to retrieve.</param>
    /// <returns>The prompt template, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when promptName is null.</exception>
    public PromptTemplate? GetPrompt(string promptName)
    {
        ArgumentException.ThrowIfNullOrEmpty(promptName);
        
        return _templates.TryGetValue(promptName, out var template) ? template : null;
    }

    /// <summary>
    /// Retrieves a specific prompt template by its name and throws an exception if not found.
    /// </summary>
    /// <param name="promptName">The name of the prompt to retrieve.</param>
    /// <returns>The prompt template.</returns>
    /// <exception cref="ArgumentNullException">Thrown when promptName is null.</exception>
    /// <exception cref="PromptNotFoundException">Thrown when the prompt is not found.</exception>
    public PromptTemplate GetRequiredPrompt(string promptName)
    {
        var template = GetPrompt(promptName);
        return template ?? throw new PromptNotFoundException(promptName);
    }

    /// <summary>
    /// Checks if a prompt template with the specified name exists.
    /// </summary>
    /// <param name="promptName">The name of the prompt to check.</param>
    /// <returns>True if the prompt exists, false otherwise.</returns>
    public bool HasPrompt(string promptName)
    {
        return !string.IsNullOrEmpty(promptName) && _templates.ContainsKey(promptName);
    }

    /// <summary>
    /// Clears all loaded templates.
    /// </summary>
    public void Clear()
    {
        _templates.Clear();
    }

    /// <summary>
    /// Gets all loaded prompt templates.
    /// </summary>
    /// <returns>Collection of all prompt templates.</returns>
    public IEnumerable<PromptTemplate> GetAllPrompts()
    {
        return _templates.Values;
    }

    /// <summary>
    /// Validates a prompt specification for required fields and consistency.
    /// </summary>
    /// <param name="specification">The specification to validate.</param>
    /// <exception cref="PromptValidationException">Thrown when validation fails.</exception>
    private static void ValidateSpecification(PromptSpecification specification)
    {
        if (string.IsNullOrEmpty(specification.Name))
        {
            throw new PromptValidationException("Prompt name is required and cannot be empty.");
        }

        if (string.IsNullOrEmpty(specification.Template))
        {
            throw new PromptValidationException($"Template is required for prompt '{specification.Name}' and cannot be empty.");
        }

        // Validate parameter ranges
        if (specification.Parameters != null)
        {
            if (specification.Parameters.Temperature.HasValue)
            {
                var temp = specification.Parameters.Temperature.Value;
                if (temp < 0 || temp > 2.0)
                {
                    throw new PromptValidationException($"Temperature for prompt '{specification.Name}' must be between 0 and 2.0.");
                }
            }

            if (specification.Parameters.TopP.HasValue)
            {
                var topP = specification.Parameters.TopP.Value;
                if (topP < 0 || topP > 1.0)
                {
                    throw new PromptValidationException($"TopP for prompt '{specification.Name}' must be between 0 and 1.0.");
                }
            }

            if (specification.Parameters.MaxTokens.HasValue && specification.Parameters.MaxTokens.Value <= 0)
            {
                throw new PromptValidationException($"MaxTokens for prompt '{specification.Name}' must be greater than 0.");
            }
        }

        // Validate placeholder definitions
        if (specification.Placeholders != null)
        {
            foreach (var (placeholderName, definition) in specification.Placeholders)
            {
                if (string.IsNullOrEmpty(placeholderName))
                {
                    throw new PromptValidationException($"Placeholder name cannot be empty in prompt '{specification.Name}'.");
                }

                if (!IsValidPlaceholderType(definition.Type))
                {
                    throw new PromptValidationException($"Invalid placeholder type '{definition.Type}' for placeholder '{placeholderName}' in prompt '{specification.Name}'. Valid types are: string, number, boolean.");
                }
            }
        }
    }

    /// <summary>
    /// Validates if a placeholder type is supported.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <returns>True if the type is valid, false otherwise.</returns>
    private static bool IsValidPlaceholderType(string type)
    {
        return type?.ToLowerInvariant() switch
        {
            "string" or "number" or "boolean" => true,
            _ => false
        };
    }
}