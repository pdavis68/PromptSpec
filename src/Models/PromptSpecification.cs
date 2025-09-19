using YamlDotNet.Serialization;

namespace PromptSpec.Models
{

    /// <summary>
    /// Represents a complete prompt specification as defined in YAML.
    /// </summary>
    public class PromptSpecification
    {
        /// <summary>
        /// A unique identifier for the prompt.
        /// </summary>
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The version of the prompt template.
        /// </summary>
        [YamlMember(Alias = "version")]
        public string Version { get; set; }

        /// <summary>
        /// A human-readable description of the prompt's purpose.
        /// </summary>
        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The system-level instructions for the model.
        /// </summary>
        [YamlMember(Alias = "systemMessage")]
        public string SystemMessage { get; set; }

        /// <summary>
        /// The main prompt content with placeholders for dynamic content.
        /// </summary>
        [YamlMember(Alias = "template")]
        public string Template { get; set; } = string.Empty;

        /// <summary>
        /// Common LLM parameters.
        /// </summary>
        [YamlMember(Alias = "parameters")]
        public LLMParameters Parameters { get; set; }

        /// <summary>
        /// Model-specific parameters as a flexible key-value map.
        /// </summary>
        [YamlMember(Alias = "modelConfig")]
        public Dictionary<string, object> ModelConfig { get; set; }

        /// <summary>
        /// Placeholder definitions for validation.
        /// </summary>
        [YamlMember(Alias = "placeholders")]
        public Dictionary<string, PlaceholderDefinition> Placeholders { get; set; }

        /// <summary>
        /// The desired output format (text, json, xml, etc.).
        /// </summary>
        [YamlMember(Alias = "outputFormat")]
        public string OutputFormat { get; set; }
    }
}
