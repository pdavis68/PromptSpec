# PromptSpec YAML Specification

This document provides a comprehensive specification for the PromptSpec YAML format, detailing all available keys, their types, validation rules, and usage examples.

## Table of Contents

- [Root Structure](#root-structure)
- [Prompt Object](#prompt-object)
- [Parameters Object](#parameters-object)
- [ModelConfig Object](#modelconfig-object)
- [Placeholders Object](#placeholders-object)
- [Validation Rules](#validation-rules)
- [Examples](#examples)
- [Migration Guide](#migration-guide)

## Root Structure

The YAML file must contain a root-level `prompts` key that contains an array of prompt objects.

```yaml
prompts:
  - # Prompt object 1
  - # Prompt object 2
  - # ... additional prompts
```

### Root Level Keys

| Key | Type | Required | Description |
|-----|------|----------|-------------|
| `prompts` | Array | ✅ Yes | Array of prompt objects defining all templates |

## Prompt Object

Each prompt object defines a single template with its configuration and validation rules.

### Prompt Object Keys

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `name` | string | ✅ Yes | - | Unique identifier for the prompt template |
| `version` | string | ❌ No | null | Version identifier for the template (e.g., "1.0", "2.1-beta") |
| `description` | string | ❌ No | null | Human-readable description of the prompt's purpose |
| `systemMessage` | string | ❌ No | null | System-level instructions for the LLM |
| `template` | string | ✅ Yes | - | Main prompt content with placeholders |
| `parameters` | object | ❌ No | null | Common LLM parameters (see [Parameters Object](#parameters-object)) |
| `modelConfig` | object | ❌ No | null | Model-specific parameters (see [ModelConfig Object](#modelconfig-object)) |
| `placeholders` | object | ❌ No | null | Placeholder definitions for validation (see [Placeholders Object](#placeholders-object)) |
| `outputFormat` | string | ❌ No | null | Expected output format (e.g., "text", "json", "xml") |

### Field Details

#### `name`
- **Type**: string
- **Required**: Yes
- **Validation**: Must be non-empty and unique across all prompts in the file
- **Example**: `"product_review_summary"`

#### `version`
- **Type**: string
- **Required**: No
- **Format**: Semantic versioning recommended but not enforced
- **Examples**: `"1.0"`, `"2.1-beta"`, `"v1.2.3"`

#### `description`
- **Type**: string
- **Required**: No
- **Purpose**: Documentation for maintainers and users
- **Example**: `"Summarizes a product review for a marketing team"`

#### `systemMessage`
- **Type**: string
- **Required**: No
- **Purpose**: Provides context and instructions to the LLM
- **Example**: `"You are a helpful assistant. Respond in a professional tone."`

#### `template`
- **Type**: string
- **Required**: Yes
- **Format**: Text with placeholders in `{placeholderName}` format
- **Validation**: Must be non-empty
- **Example**: `"Hello {name}, your order {orderId} is {status}."`

#### `outputFormat`
- **Type**: string
- **Required**: No
- **Common Values**: `"text"`, `"json"`, `"xml"`, `"markdown"`, `"html"`
- **Purpose**: Indicates expected output format for downstream processing
- **Example**: `"json"`

## Parameters Object

The `parameters` object contains common LLM parameters that are widely supported across different models.

### Parameters Keys

| Key | Type | Required | Valid Range | Default | Description |
|-----|------|----------|-------------|---------|-------------|
| `temperature` | number | ❌ No | 0.0 - 2.0 | null | Controls randomness of output |
| `topP` | number | ❌ No | 0.0 - 1.0 | null | Nucleus sampling parameter |
| `maxTokens` | integer | ❌ No | > 0 | null | Maximum number of tokens to generate |
| `stopSequences` | array[string] | ❌ No | - | null | Sequences that stop generation |

### Field Details

#### `temperature`
- **Type**: number (double)
- **Range**: 0.0 to 2.0
- **Purpose**: Controls creativity/randomness
  - `0.0` = Deterministic (least creative)
  - `1.0` = Balanced
  - `2.0` = Maximum creativity
- **Example**: `0.7`

#### `topP`
- **Type**: number (double)
- **Range**: 0.0 to 1.0
- **Purpose**: Nucleus sampling for token selection
- **Example**: `0.9`

#### `maxTokens`
- **Type**: integer
- **Range**: Must be greater than 0
- **Purpose**: Limits response length
- **Example**: `150`

#### `stopSequences`
- **Type**: array of strings
- **Purpose**: Strings that will terminate generation when encountered
- **Example**: `["---", "END", "\n\n---\n\n"]`

### Example Parameters Object

```yaml
parameters:
  temperature: 0.7
  topP: 0.9
  maxTokens: 150
  stopSequences: ["---END---", "\n\n"]
```

## ModelConfig Object

The `modelConfig` object is a flexible key-value store for model-specific parameters that don't fit into the common `parameters` object.

### Common ModelConfig Keys

| Key | Type | Example Value | Models | Description |
|-----|------|---------------|--------|-------------|
| `seed` | integer | `1234` | OpenAI, Anthropic | Seed for deterministic output |
| `repetitionPenalty` | number | `1.1` | HuggingFace | Penalty for repeating tokens |
| `presencePenalty` | number | `0.5` | OpenAI | Penalty for token presence |
| `frequencyPenalty` | number | `0.3` | OpenAI | Penalty for token frequency |
| `responseFormat` | string | `"json_object"` | OpenAI GPT-4 | Structured output format |
| `tools` | array | `[]` | OpenAI | Function calling tools |
| `toolChoice` | string | `"auto"` | OpenAI | Tool selection strategy |

### Example ModelConfig Object

```yaml
modelConfig:
  seed: 1234
  repetitionPenalty: 1.1
  presencePenalty: 0.0
  frequencyPenalty: 0.1
  responseFormat: "json_object"
  customParameter: "custom_value"
```

## Placeholders Object

The `placeholders` object defines validation rules for template placeholders. Each key represents a placeholder name found in the template.

### Placeholder Definition Keys

| Key | Type | Required | Valid Values | Default | Description |
|-----|------|----------|--------------|---------|-------------|
| `type` | string | ❌ No | `"string"`, `"number"`, `"boolean"` | `"string"` | Expected data type |
| `required` | boolean | ❌ No | `true`, `false` | `false` | Whether placeholder must be provided |

### Field Details

#### `type`
- **Type**: string
- **Valid Values**: 
  - `"string"` - Text values
  - `"number"` - Numeric values (int, long, float, double, decimal)
  - `"boolean"` - Boolean values (true/false)
- **Default**: `"string"`
- **Purpose**: Validates runtime values match expected type

#### `required`
- **Type**: boolean
- **Default**: `false`
- **Purpose**: Ensures critical placeholders are always provided

### Example Placeholders Object

```yaml
placeholders:
  userId:
    type: "string"
    required: true
  age:
    type: "number"
    required: true
  isActive:
    type: "boolean"
    required: false
  notes:
    type: "string"
    required: false
```

## Validation Rules

### File-Level Validation
- YAML must be valid and parseable
- Root `prompts` key must exist and contain an array
- Array cannot be empty

### Prompt-Level Validation
- `name` must be unique across all prompts
- `name` and `template` cannot be empty strings
- Parameter ranges must be within valid bounds:
  - `temperature`: 0.0 ≤ value ≤ 2.0
  - `topP`: 0.0 ≤ value ≤ 1.0
  - `maxTokens`: value > 0

### Placeholder Validation
- Placeholder names in `placeholders` object can reference any placeholder in template
- Placeholder `type` must be one of: `"string"`, `"number"`, `"boolean"`
- Runtime validation occurs during `GeneratePrompt()`:
  - Required placeholders must be provided
  - Provided values must match declared types

### Type Conversion Rules

| Declared Type | Valid C# Types |
|---------------|----------------|
| `"string"` | `string`, all other types converted to string |
| `"number"` | `int`, `long`, `float`, `double`, `decimal` |
| `"boolean"` | `bool` |

## Examples

### Minimal Example

```yaml
prompts:
  - name: "simple_greeting"
    template: "Hello {name}!"
```

### Basic Example with Parameters

```yaml
prompts:
  - name: "greeting"
    version: "1.0"
    description: "A personalized greeting"
    template: "Hello {name}, welcome to {platform}!"
    parameters:
      temperature: 0.5
      maxTokens: 50
    placeholders:
      name:
        type: "string"
        required: true
      platform:
        type: "string"
        required: true
```

### Advanced Example

```yaml
prompts:
  - name: "customer_support_response"
    version: "2.1"
    description: "Generate customer support responses"
    systemMessage: "You are a helpful customer support agent. Be polite and professional."
    template: |
      Customer: {customerName}
      Issue Type: {issueType}
      Priority: {priority}
      
      Customer Message:
      {customerMessage}
      
      Previous Interactions: {interactionCount}
      
      Please provide a helpful response:
    parameters:
      temperature: 0.3
      topP: 0.9
      maxTokens: 300
      stopSequences: ["---END---"]
    modelConfig:
      seed: 42
      presencePenalty: 0.1
      frequencyPenalty: 0.1
    outputFormat: "text"
    placeholders:
      customerName:
        type: "string"
        required: true
      issueType:
        type: "string"
        required: true
      priority:
        type: "number"
        required: true
      customerMessage:
        type: "string"
        required: true
      interactionCount:
        type: "number"
        required: false
```

### JSON Output Example

```yaml
prompts:
  - name: "structured_data_extraction"
    version: "1.0"
    description: "Extract structured data from text"
    systemMessage: "Extract information and return as valid JSON only."
    template: |
      Extract the following information from this text and return as JSON:
      
      Text: {inputText}
      
      Required fields:
      - name: person's full name
      - email: email address if available
      - phone: phone number if available
      - company: company name if mentioned
      
      JSON:
    parameters:
      temperature: 0.1
      maxTokens: 200
    modelConfig:
      responseFormat: "json_object"
    outputFormat: "json"
    placeholders:
      inputText:
        type: "string"
        required: true
```

## Migration Guide

### From Version 1.0 to 2.0

If you're upgrading an existing YAML schema, consider these changes:

1. **New Optional Fields**: `version`, `description`, `systemMessage`, `outputFormat`
2. **Enhanced Validation**: Stricter parameter range checking
3. **Extended ModelConfig**: More flexibility for model-specific parameters

### Backward Compatibility

PromptSpec maintains backward compatibility with simpler configurations:

```yaml
# This still works
prompts:
  - name: "simple"
    template: "Hello {name}"
```

### Best Practices for Schema Evolution

1. Always include `version` in new templates
2. Use `description` for documentation
3. Migrate complex configurations to use `modelConfig`
4. Add placeholder validation incrementally

## Schema Validation

The library validates YAML against this specification at load time. Common validation errors:

| Error | Cause | Solution |
|-------|-------|----------|
| "Prompt name is required" | Empty or missing `name` | Provide non-empty name |
| "Template is required" | Empty or missing `template` | Provide template content |
| "Temperature must be between 0 and 2.0" | Invalid temperature value | Use value in valid range |
| "Duplicate prompt name found" | Multiple prompts with same name | Use unique names |
| "Invalid placeholder type" | Unsupported type in placeholders | Use: string, number, boolean |

## Reference Implementation

The PromptSpec library implements this specification with the following C# classes:

- `PromptCollection` - Root container
- `PromptSpecification` - Individual prompt
- `LLMParameters` - Common parameters
- `PlaceholderDefinition` - Placeholder validation rules

For implementation details, see the library source code and README.md.