using YamlDotNet.Serialization;

namespace PromptSpec.Models
{

    /// <summary>
    /// Defines the validation rules for a placeholder in a prompt template.
    /// </summary>
    public class PlaceholderDefinition
    {
        /// <summary>
        /// The expected data type for this placeholder (string, number, boolean).
        /// </summary>
        [YamlMember(Alias = "type")]
        public string Type { get; set; } = "string";

        /// <summary>
        /// Indicates whether this placeholder is required and must be provided.
        /// </summary>
        [YamlMember(Alias = "required")]
        public bool Required { get; set; } = false;
    }
}
