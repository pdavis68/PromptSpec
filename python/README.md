# PromptSpec Python Library

A Python library for managing prompt templates with YAML configuration, placeholder validation, and LLM parameter handling.

## Features

- **YAML-based Configuration**: Define prompts with structured YAML files
- **Placeholder Validation**: Type-safe placeholder replacement with validation rules
- **LLM Parameter Management**: Built-in support for common LLM parameters
- **Async/Sync Support**: Both asynchronous and synchronous APIs
- **Type Safety**: Full type hints and Pydantic validation
- **Extensible**: Easy to extend for custom validation rules and parameters

## Installation

```bash
pip install promptspec
```

For development dependencies:

```bash
pip install promptspec[dev]
```

## Quick Start

### 1. Define Your Prompts in YAML

Create a `prompts.yaml` file:

```yaml
prompts:
  - name: "greeting"
    description: "A simple greeting prompt"
    template: "Hello {name}! Welcome to {platform}."
    placeholders:
      name:
        type: "string"
        required: true
      platform:
        type: "string"
        required: false
    parameters:
      temperature: 0.7
      maxTokens: 100

  - name: "code-review"
    description: "Code review assistant"
    systemMessage: "You are a helpful code review assistant."
    template: "Please review this {language} code:\\n\\n{code}\\n\\nFocus on: {focus_areas}"
    placeholders:
      language:
        type: "string"
        required: true
      code:
        type: "string"
        required: true
      focus_areas:
        type: "string"
        required: false
    parameters:
      temperature: 0.3
      maxTokens: 500
      topP: 0.9
    outputFormat: "markdown"
```

### 2. Load and Use Templates

```python
from promptspec import PromptManager

# Create manager and load templates
manager = PromptManager()
manager.load_templates_from_file("prompts.yaml")

# Get a template
template = manager.get_required_prompt("greeting")

# Generate prompt with replacements
result = template.generate_prompt({
    "name": "Alice",
    "platform": "PromptSpec"
})
print(result)  # "Hello Alice! Welcome to PromptSpec."

# Access template properties
print(template.name)  # "greeting"
print(template.description)  # "A simple greeting prompt"
parameters = template.get_parameters()
print(parameters.temperature)  # 0.7
print(parameters.max_tokens)  # 100
```

### 3. Async Usage

```python
import asyncio
from promptspec import PromptManager

async def main():
    manager = PromptManager()
    await manager.load_templates_from_file_async("prompts.yaml")
    
    template = manager.get_required_prompt("code-review")
    result = template.generate_prompt({
        "language": "Python",
        "code": "def hello(): print('world')",
        "focus_areas": "performance, readability"
    })
    print(result)

asyncio.run(main())
```

### 4. Error Handling

```python
from promptspec import (
    PromptManager, 
    PromptNotFoundException, 
    MissingPlaceholderException,
    PlaceholderTypeException
)

manager = PromptManager()
manager.load_templates_from_file("prompts.yaml")

try:
    # This will raise PromptNotFoundException
    template = manager.get_required_prompt("nonexistent")
except PromptNotFoundException as e:
    print(f"Prompt not found: {e.prompt_name}")

try:
    template = manager.get_required_prompt("greeting")
    # This will raise MissingPlaceholderException (name is required)
    result = template.generate_prompt({"platform": "PromptSpec"})
except MissingPlaceholderException as e:
    print(f"Missing required placeholder: {e.placeholder_name}")

try:
    # This will raise PlaceholderTypeException (name should be string)
    result = template.generate_prompt({"name": 123, "platform": "PromptSpec"})
except PlaceholderTypeException as e:
    print(f"Type error for {e.placeholder_name}: expected {e.expected_type}, got {e.actual_type}")
```

## API Reference

### PromptManager

The main class for loading and managing prompt templates.

#### Methods

- `load_templates(yaml_content: str)` - Load templates from YAML string
- `load_templates_async(yaml_content: str)` - Async version of load_templates
- `load_templates_from_file(file_path: Union[str, Path])` - Load from file
- `load_templates_from_file_async(file_path: Union[str, Path])` - Async version
- `get_prompt(name: str) -> Optional[PromptTemplate]` - Get template by name
- `get_required_prompt(name: str) -> PromptTemplate` - Get template or raise exception
- `has_prompt(name: str) -> bool` - Check if template exists
- `clear()` - Clear all loaded templates
- `get_all_prompts() -> List[PromptTemplate]` - Get all templates

#### Properties

- `template_count: int` - Number of loaded templates
- `template_names: List[str]` - Names of all loaded templates

### PromptTemplate

Represents a loaded prompt template.

#### Methods

- `generate_prompt(replacements: Dict[str, Any]) -> str` - Generate final prompt
- `get_parameters() -> LLMParameters` - Get LLM parameters
- `get_model_config() -> Dict[str, Any]` - Get model-specific config
- `get_template() -> str` - Get raw template string
- `get_placeholder_names() -> List[str]` - Get all placeholder names

#### Properties

- `name: str` - Template name
- `version: Optional[str]` - Template version
- `description: Optional[str]` - Template description
- `system_message: Optional[str]` - System message
- `output_format: Optional[str]` - Expected output format

### Models

#### LLMParameters

Common LLM parameters with validation:

- `temperature: Optional[float]` - Controls randomness (0-2.0)
- `top_p: Optional[float]` - Nucleus sampling (0-1.0)
- `max_tokens: Optional[int]` - Maximum tokens to generate
- `stop_sequences: Optional[List[str]]` - Stop sequences

#### PlaceholderDefinition

Defines validation rules for placeholders:

- `type: PlaceholderType` - Expected type ("string", "number", "boolean")
- `required: bool` - Whether placeholder is required

## YAML Schema

### Prompt Specification

```yaml
prompts:
  - name: string              # Required: Unique identifier
    version: string           # Optional: Version string
    description: string       # Optional: Human-readable description
    systemMessage: string     # Optional: System-level instructions
    template: string          # Required: Main prompt with {placeholders}
    parameters:               # Optional: LLM parameters
      temperature: float      # 0.0-2.0
      topP: float            # 0.0-1.0
      maxTokens: integer     # > 0
      stopSequences:         # List of stop strings
        - string
    modelConfig:             # Optional: Model-specific parameters
      key: value
    placeholders:            # Optional: Placeholder definitions
      placeholder_name:
        type: string         # "string", "number", "boolean"
        required: boolean
    outputFormat: string     # Optional: Expected output format
```

## Development

### Setup Development Environment

```bash
git clone https://github.com/yourusername/promptspec.git
cd promptspec/python
pip install -e .[dev]
```

### Running Tests

```bash
pytest
```

### Code Formatting

```bash
black promptspec/
isort promptspec/
```

### Type Checking

```bash
mypy promptspec/
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

MIT License - see LICENSE file for details.

## Changelog

### 1.0.0

- Initial release
- YAML-based prompt management
- Placeholder validation
- Async/sync API support
- Full type hints with Pydantic
- LLM parameter handling