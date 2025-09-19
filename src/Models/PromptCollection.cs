using YamlDotNet.Serialization;

namespace PromptSpec.Models
{

    /// <summary>
    /// Root container for all prompt specifications loaded from YAML.
    /// </summary>
    public class PromptCollection
    {
        /// <summary>
        /// Collection of prompt specifications.
        /// </summary>
        [YamlMember(Alias = "prompts")]
        public List<PromptSpecification> Prompts { get; set; }
    }
}
