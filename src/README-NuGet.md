# PromptSpec

A C# library for managing prompt templates with YAML-based configuration, parameter validation, and dynamic content replacement for LLM (Large Language Model) applications.

## Installation

Install the PromptSpec NuGet package:

### Package Manager Console
```powershell
Install-Package PromptSpec
```

### .NET CLI
```bash
dotnet add package PromptSpec
```

### Package Reference
```xml
<PackageReference Include="PromptSpec" Version="1.0.1" />
```

## Features

- 📝 **YAML-based prompt definitions** - Define and organize your prompts in human-readable YAML files
- 🔧 **Dynamic placeholder replacement** - Replace placeholders with runtime values using simple `{key}` syntax
- ✅ **Type validation** - Validate placeholder values against expected types (string, number, boolean)
- 🎯 **Required field validation** - Ensure critical placeholders are always provided
- ⚙️ **LLM parameter management** - Built-in support for common parameters like temperature, topP, maxTokens
- 🔌 **Model-specific configuration** - Flexible key-value store for model-specific parameters
- 🚀 **Async loading** - Asynchronous template loading for better performance
- 📋 **Multiple output formats** - Support for text, JSON, XML, and custom output formats

## Quick Start

### 1. Create a YAML Template File

Create a `templates.yaml` file in your project:

```yaml
prompts:
  - name: "greeting"
    version: "1.0"
    description: "A simple greeting prompt"
    template: "Hello {name}, welcome to {platform}!"
    parameters:
      temperature: 0.7
      maxTokens: 50
    placeholders:
      name:
        type: "string"
        required: true
      platform:
        type: "string"
        required: true
```

### 2. Load and Use Templates

```csharp
using PromptSpec.Core;

// Create a prompt manager
var manager = new PromptManager();

// Load templates from YAML
await manager.LoadTemplatesFromFileAsync("templates.yaml");

// Get a specific template
var template = manager.GetPrompt("greeting");

// Generate the prompt with data
var replacements = new Dictionary<string, object>
{
    { "name", "Alice" },
    { "platform", "PromptSpec" }
};

var prompt = template.GeneratePrompt(replacements);
// Output: "Hello Alice, welcome to PromptSpec!"

// Get LLM parameters
var parameters = template.GetParameters();
Console.WriteLine($"Temperature: {parameters.Temperature}"); // 0.7
Console.WriteLine($"Max Tokens: {parameters.MaxTokens}");   // 50
```

## Advanced Example

```yaml
prompts:
  - name: "product_review_summary"
    version: "1.0"
    description: "Summarizes a product review for analysis"
    systemMessage: "You are a marketing analyst. Summarize the following product review."
    template: |
      Review ID: {reviewId}
      Customer: {customerName}
      Rating: {rating}/5
      
      Review Text:
      {reviewText}
      
      Provide a concise summary focusing on key points.
    parameters:
      temperature: 0.2
      maxTokens: 150
      topP: 0.9
    placeholders:
      reviewId:
        type: "string"
        required: true
      customerName:
        type: "string"
        required: false
      rating:
        type: "number"
        required: true
      reviewText:
        type: "string"
        required: true
```

```csharp
using PromptSpec.Core;
using PromptSpec.Exceptions;

var manager = new PromptManager();
await manager.LoadTemplatesFromFileAsync("templates.yaml");

var template = manager.GetPrompt("product_review_summary");

var reviewData = new Dictionary<string, object>
{
    { "reviewId", "REV-12345" },
    { "customerName", "John Doe" },
    { "rating", 5 },
    { "reviewText", "This product exceeded my expectations! Outstanding quality." }
};

try
{
    var prompt = template.GeneratePrompt(reviewData);
    Console.WriteLine("Generated Prompt:");
    Console.WriteLine(prompt);
    
    // Get system message for your LLM client
    Console.WriteLine($"System Message: {template.SystemMessage}");
    
    // Get parameters for your LLM API call
    var parameters = template.GetParameters();
    Console.WriteLine($"Temperature: {parameters.Temperature}");
    Console.WriteLine($"Max Tokens: {parameters.MaxTokens}");
}
catch (MissingPlaceholderException ex)
{
    Console.WriteLine($"Missing required field: {ex.PlaceholderName}");
}
catch (PlaceholderTypeException ex)
{
    Console.WriteLine($"Type error: {ex.PlaceholderName} should be {ex.ExpectedType}");
}
```

## API Overview

### PromptManager
- `LoadTemplatesAsync(string yamlContent)` - Load from YAML string
- `LoadTemplatesFromFileAsync(string filePath)` - Load from file
- `GetPrompt(string name)` - Get template (null if not found)
- `GetRequiredPrompt(string name)` - Get template (throws if not found)
- `HasPrompt(string name)` - Check if template exists

### PromptTemplate
- `GeneratePrompt(Dictionary<string, object> replacements)` - Generate prompt
- `GetParameters()` - Get LLM parameters
- `GetModelConfig()` - Get model-specific config
- `Name`, `Version`, `Description` - Template metadata
- `SystemMessage` - System message for LLM
- `OutputFormat` - Expected output format

### Exception Handling
- `PromptValidationException` - YAML parsing/validation errors
- `MissingPlaceholderException` - Required placeholder missing
- `PlaceholderTypeException` - Type validation failed
- `PromptNotFoundException` - Template not found

## Integration Examples

### With OpenAI Client
```csharp
var template = manager.GetPrompt("my_prompt");
var prompt = template.GeneratePrompt(data);
var parameters = template.GetParameters();

var response = await openAiClient.GetChatCompletionsAsync(
    new ChatCompletionsOptions
    {
        Messages = { new ChatMessage(ChatRole.System, template.SystemMessage),
                    new ChatMessage(ChatRole.User, prompt) },
        Temperature = (float?)parameters.Temperature,
        MaxTokens = parameters.MaxTokens
    });
```

### With Azure OpenAI
```csharp
var template = manager.GetPrompt("analysis_prompt");
var prompt = template.GeneratePrompt(analysisData);
var params = template.GetParameters();

var chatOptions = new ChatCompletionsOptions()
{
    Temperature = (float?)params.Temperature,
    MaxTokens = params.MaxTokens,
    NucleusSamplingFactor = (float?)params.TopP
};

chatOptions.Messages.Add(new ChatMessage(ChatRole.System, template.SystemMessage));
chatOptions.Messages.Add(new ChatMessage(ChatRole.User, prompt));
```

## Best Practices

1. **Load templates once** at application startup
2. **Use descriptive names** for prompts and placeholders
3. **Always handle exceptions** when generating prompts
4. **Set appropriate types** for placeholder validation
5. **Use conservative temperature values** for consistent outputs
6. **Cache PromptManager instances** for better performance

## Requirements

- .NET 9.0 or later
- YamlDotNet (automatically included)

## Documentation

For complete documentation, examples, and advanced usage patterns, visit the [GitHub repository](https://github.com/pdavis68/PromptSpec).

## License

MIT License - see the LICENSE file for details.

## Support

- [GitHub Issues](https://github.com/pdavis68/PromptSpec/issues) - Bug reports and feature requests
- [GitHub Discussions](https://github.com/pdavis68/PromptSpec/discussions) - Questions and community support