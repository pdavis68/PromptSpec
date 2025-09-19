using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace PromptSpec.Models
{

    /// <summary>
    /// Represents common LLM parameters that can be used across different models.
    /// </summary>
    public class LLMParameters
    {
        /// <summary>
        /// Controls the randomness of the output. Value between 0 and 2.0.
        /// </summary>
        [YamlMember(Alias = "temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// Nucleus sampling parameter. Value between 0 and 1.0.
        /// </summary>
        [YamlMember(Alias = "topP")]
        public double? TopP { get; set; }

        /// <summary>
        /// The maximum number of tokens to generate.
        /// </summary>
        [YamlMember(Alias = "maxTokens")]
        public int? MaxTokens { get; set; }

        /// <summary>
        /// A list of strings that will stop the generation when encountered.
        /// </summary>
        [YamlMember(Alias = "stopSequences")]
        public List<string> StopSequences { get; set; }
    }
}
