# PromptSpec

A C# library for managing prompt templates with YAML-based configuration, parameter validation, and dynamic content replacement for LLM (Large Language Model) applications.

## Features

- 📝 **YAML-based prompt definitions** - Define and organize your prompts in human-readable YAML files
- 🔧 **Dynamic placeholder replacement** - Replace placeholders with runtime values using simple `{key}` syntax
- ✅ **Type validation** - Validate placeholder values against expected types (string, number, boolean)
- 🎯 **Required field validation** - Ensure critical placeholders are always provided
- ⚙️ **LLM parameter management** - Built-in support for common parameters like temperature, topP, maxTokens
- 🔌 **Model-specific configuration** - Flexible key-value store for model-specific parameters
- 🚀 **Async loading** - Asynchronous template loading for better performance
- 📋 **Multiple output formats** - Support for text, JSON, XML, and custom output formats

## Installation

Install the PromptSpec NuGet package:

```bash
dotnet add package PromptSpec
```

## Quick Start

### 1. Create a YAML Template File

Create a `templates.yaml` file:

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

## Comprehensive Example

Here's a more complex example showing advanced features:

### YAML Configuration

```yaml
prompts:
  - name: "product_review_summary"
    version: "1.0"
    description: "Summarizes a product review for a marketing team."
    systemMessage: "You are a marketing analyst. Summarize the following product review into a single, concise paragraph."
    template: |
      Review ID: {reviewId}
      Customer: {customerName}
      Rating: {rating}/5
      
      Review Text:
      {reviewText}
      
      ---
      Summary:
    parameters:
      temperature: 0.2
      maxTokens: 150
      topP: 0.9
      stopSequences: ["---END---"]
    modelConfig:
      seed: 1234
      repetitionPenalty: 1.1
    outputFormat: "text"
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

  - name: "json_recipe_generator"
    version: "2.0"
    description: "Generates a recipe in JSON format."
    systemMessage: "You are a culinary expert. Generate a detailed recipe in JSON format."
    template: |
      Generate a JSON recipe with these details:
      - Name: {recipeName}
      - Difficulty: {difficulty}
      - Prep Time: {prepTime}
      - Ingredients: {ingredients}
    parameters:
      temperature: 0.8
    modelConfig:
      seed: 1234
      responseFormat: "json_object"
    outputFormat: "json"
    placeholders:
      recipeName:
        type: "string"
        required: true
      difficulty:
        type: "string"
        required: true
      prepTime:
        type: "string"
        required: false
      ingredients:
        type: "string"
        required: false
```

### C# Implementation

```csharp
using PromptSpec.Core;
using PromptSpec.Exceptions;

class Program
{
    static async Task Main(string[] args)
    {
        var manager = new PromptManager();
        
        try
        {
            // Load templates
            await manager.LoadTemplatesFromFileAsync("templates.yaml");
            
            // Example 1: Product Review Summary
            await ProcessProductReview(manager);
            
            // Example 2: Recipe Generator
            await ProcessRecipe(manager);
        }
        catch (PromptValidationException ex)
        {
            Console.WriteLine($"Validation Error: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File Error: {ex.Message}");
        }
    }
    
    static async Task ProcessProductReview(PromptManager manager)
    {
        var template = manager.GetPrompt("product_review_summary");
        
        var reviewData = new Dictionary<string, object>
        {
            { "reviewId", "REV-12345" },
            { "customerName", "John Doe" },
            { "rating", 5 },
            { "reviewText", "This product exceeded my expectations! The quality is outstanding and delivery was fast." }
        };
        
        // Generate the prompt
        var prompt = template.GeneratePrompt(reviewData);
        Console.WriteLine("Generated Prompt:");
        Console.WriteLine(prompt);
        
        // Get system message
        Console.WriteLine($"\nSystem Message: {template.SystemMessage}");
        
        // Get parameters for LLM
        var parameters = template.GetParameters();
        Console.WriteLine($"Temperature: {parameters.Temperature}");
        Console.WriteLine($"Max Tokens: {parameters.MaxTokens}");
        Console.WriteLine($"Top P: {parameters.TopP}");
        
        // Get model-specific config
        var modelConfig = template.GetModelConfig();
        foreach (var kvp in modelConfig)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
    
    static async Task ProcessRecipe(PromptManager manager)
    {
        var template = manager.GetPrompt("json_recipe_generator");
        
        var recipeData = new Dictionary<string, object>
        {
            { "recipeName", "Chocolate Chip Cookies" },
            { "difficulty", "Easy" },
            { "prepTime", "30 minutes" },
            { "ingredients", "flour, sugar, butter, chocolate chips, eggs" }
        };
        
        try
        {
            var prompt = template.GeneratePrompt(recipeData);
            Console.WriteLine("\nRecipe Prompt:");
            Console.WriteLine(prompt);
            Console.WriteLine($"Output Format: {template.OutputFormat}");
        }
        catch (MissingPlaceholderException ex)
        {
            Console.WriteLine($"Missing required field: {ex.PlaceholderName}");
        }
        catch (PlaceholderTypeException ex)
        {
            Console.WriteLine($"Type mismatch for {ex.PlaceholderName}: expected {ex.ExpectedType}, got {ex.ActualType}");
        }
    }
}
```

## Error Handling

PromptSpec provides specific exceptions for different error scenarios:

### `PromptValidationException`
Thrown when YAML parsing fails or template validation errors occur.

### `MissingPlaceholderException`
Thrown when a required placeholder is not provided.

```csharp
try
{
    var prompt = template.GeneratePrompt(incompleteData);
}
catch (MissingPlaceholderException ex)
{
    Console.WriteLine($"Missing required placeholder: {ex.PlaceholderName}");
}
```

### `PlaceholderTypeException`
Thrown when a placeholder value doesn't match the expected type.

```csharp
try
{
    var data = new Dictionary<string, object>
    {
        { "age", "not_a_number" } // Should be number
    };
    var prompt = template.GeneratePrompt(data);
}
catch (PlaceholderTypeException ex)
{
    Console.WriteLine($"Type error for {ex.PlaceholderName}: expected {ex.ExpectedType}, got {ex.ActualType}");
}
```

### `PromptNotFoundException`
Thrown when trying to get a non-existent prompt template.

```csharp
try
{
    var template = manager.GetRequiredPrompt("non_existent_prompt");
}
catch (PromptNotFoundException ex)
{
    Console.WriteLine($"Prompt not found: {ex.PromptName}");
}
```

## API Reference

### PromptManager

#### Methods

- `LoadTemplatesAsync(string yamlContent)` - Load templates from YAML string
- `LoadTemplatesFromFileAsync(string filePath)` - Load templates from YAML file
- `GetPrompt(string promptName)` - Get template by name (returns null if not found)
- `GetRequiredPrompt(string promptName)` - Get template by name (throws exception if not found)
- `HasPrompt(string promptName)` - Check if template exists
- `Clear()` - Clear all loaded templates
- `GetAllPrompts()` - Get all loaded templates

#### Properties

- `TemplateCount` - Number of loaded templates
- `TemplateNames` - Names of all loaded templates

### PromptTemplate

#### Methods

- `GeneratePrompt(Dictionary<string, object> replacements)` - Generate prompt with replacements
- `GetParameters()` - Get LLM parameters
- `GetModelConfig()` - Get model-specific configuration
- `GetTemplate()` - Get raw template string
- `GetPlaceholderNames()` - Get all placeholder names in template

#### Properties

- `Name` - Template name
- `Version` - Template version
- `Description` - Template description
- `SystemMessage` - System message for LLM
- `OutputFormat` - Expected output format

### LLMParameters

#### Properties

- `Temperature` - Controls randomness (0.0 - 2.0)
- `TopP` - Nucleus sampling (0.0 - 1.0)
- `MaxTokens` - Maximum tokens to generate
- `StopSequences` - List of stop sequences

## Best Practices

### 1. Template Organization
- Use descriptive names for your prompts
- Include version numbers for template evolution
- Add descriptions to document prompt purposes
- Group related prompts in the same YAML file

### 2. Placeholder Design
- Use clear, descriptive placeholder names
- Always specify `required: true` for critical data
- Use appropriate types for validation
- Provide default values in your application for optional fields

### 3. Parameter Management
- Set conservative temperature values for consistent outputs
- Use appropriate maxTokens to control response length
- Leverage stop sequences to control generation boundaries

### 4. Error Handling
- Always wrap prompt generation in try-catch blocks
- Provide meaningful error messages to users
- Log validation errors for debugging

### 5. Performance
- Load templates once at application startup
- Cache PromptManager instances
- Use async methods for file operations

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For questions, issues, or feature requests, please open an issue on GitHub.