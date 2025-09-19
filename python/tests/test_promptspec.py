"""
Basic tests for the PromptSpec library.
"""

import pytest
from promptspec import (
    PromptManager,
    PromptTemplate,
    LLMParameters,
    PlaceholderDefinition,
    PromptSpecification,
    PromptCollection,
    PromptValidationException,
    MissingPlaceholderException,
    PlaceholderTypeException,
    PromptNotFoundException
)


def test_llm_parameters():
    """Test LLMParameters model."""
    params = LLMParameters(
        temperature=0.7,
        top_p=0.9,
        max_tokens=100,
        stop_sequences=["END", "STOP"]
    )
    
    assert params.temperature == 0.7
    assert params.top_p == 0.9
    assert params.max_tokens == 100
    assert params.stop_sequences == ["END", "STOP"]


def test_placeholder_definition():
    """Test PlaceholderDefinition model."""
    placeholder = PlaceholderDefinition(type="string", required=True)
    
    assert placeholder.type == "string"
    assert placeholder.required is True


def test_prompt_specification():
    """Test PromptSpecification model."""
    spec = PromptSpecification(
        name="test",
        template="Hello {name}!",
        parameters=LLMParameters(temperature=0.5),
        placeholders={"name": PlaceholderDefinition(type="string", required=True)}
    )
    
    assert spec.name == "test"
    assert spec.template == "Hello {name}!"
    assert spec.parameters.temperature == 0.5
    assert spec.placeholders["name"].required is True


def test_prompt_template():
    """Test PromptTemplate functionality."""
    spec = PromptSpecification(
        name="greeting",
        template="Hello {name}! Welcome to {platform}.",
        placeholders={
            "name": PlaceholderDefinition(type="string", required=True),
            "platform": PlaceholderDefinition(type="string", required=False)
        }
    )
    
    template = PromptTemplate(spec)
    
    assert template.name == "greeting"
    assert template.get_template() == "Hello {name}! Welcome to {platform}."
    
    # Test successful generation
    result = template.generate_prompt({"name": "Alice", "platform": "PromptSpec"})
    assert result == "Hello Alice! Welcome to PromptSpec."
    
    # Test with missing optional placeholder
    result = template.generate_prompt({"name": "Bob"})
    assert result == "Hello Bob! Welcome to ."
    
    # Test placeholder names extraction
    placeholders = template.get_placeholder_names()
    assert set(placeholders) == {"name", "platform"}


def test_prompt_template_validation_errors():
    """Test PromptTemplate validation errors."""
    spec = PromptSpecification(
        name="test",
        template="Hello {name}!",
        placeholders={"name": PlaceholderDefinition(type="string", required=True)}
    )
    
    template = PromptTemplate(spec)
    
    # Test missing required placeholder
    with pytest.raises(MissingPlaceholderException) as exc_info:
        template.generate_prompt({})
    assert exc_info.value.placeholder_name == "name"
    
    # Test type validation
    with pytest.raises(PlaceholderTypeException) as exc_info:
        template.generate_prompt({"name": 123})
    assert exc_info.value.placeholder_name == "name"
    assert exc_info.value.expected_type == "string"
    assert exc_info.value.actual_type == "number"


def test_prompt_manager():
    """Test PromptManager functionality."""
    yaml_content = """
prompts:
  - name: "test"
    template: "Hello {name}!"
    placeholders:
      name:
        type: "string"
        required: true
    parameters:
      temperature: 0.7
"""
    
    manager = PromptManager()
    manager.load_templates(yaml_content)
    
    assert manager.template_count == 1
    assert "test" in manager.template_names
    assert manager.has_prompt("test")
    assert not manager.has_prompt("nonexistent")
    
    template = manager.get_prompt("test")
    assert template is not None
    assert template.name == "test"
    
    # Test get_required_prompt
    template = manager.get_required_prompt("test")
    assert template.name == "test"
    
    # Test exception for missing prompt
    with pytest.raises(PromptNotFoundException) as exc_info:
        manager.get_required_prompt("nonexistent")
    assert exc_info.value.prompt_name == "nonexistent"


def test_prompt_manager_validation_errors():
    """Test PromptManager validation errors."""
    manager = PromptManager()
    
    # Test empty YAML
    with pytest.raises(ValueError):
        manager.load_templates("")
    
    # Test invalid YAML structure
    with pytest.raises(PromptValidationException):
        manager.load_templates("invalid: yaml: content:")
    
    # Test missing prompts section
    with pytest.raises(PromptValidationException):
        manager.load_templates("other: content")
    
    # Test duplicate prompt names
    yaml_content = """
prompts:
  - name: "duplicate"
    template: "First template"
  - name: "duplicate"
    template: "Second template"
"""
    with pytest.raises(PromptValidationException):
        manager.load_templates(yaml_content)


if __name__ == "__main__":
    pytest.main([__file__])