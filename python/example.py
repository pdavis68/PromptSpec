#!/usr/bin/env python3
"""
Example usage of the PromptSpec library.
Run this script to see the library in action.
"""

import asyncio
from pathlib import Path
from promptspec import (
    PromptManager,
    PromptNotFoundException,
    MissingPlaceholderException,
    PlaceholderTypeException
)


def sync_example():
    """Demonstrate synchronous usage of PromptSpec."""
    print("=== Synchronous Example ===")
    
    # Create manager and load templates
    manager = PromptManager()
    manager.load_templates_from_file("example_prompts.yaml")
    
    print(f"Loaded {manager.template_count} templates:")
    for name in manager.template_names:
        print(f"  - {name}")
    print()
    
    # Use greeting template
    template = manager.get_required_prompt("greeting")
    print(f"Template: {template.name}")
    print(f"Description: {template.description}")
    
    result = template.generate_prompt({
        "name": "Alice",
        "platform": "PromptSpec"
    })
    print(f"Generated: {result}")
    
    # Show parameters
    params = template.get_parameters()
    print(f"Temperature: {params.temperature}")
    print(f"Max Tokens: {params.max_tokens}")
    print()


async def async_example():
    """Demonstrate asynchronous usage of PromptSpec."""
    print("=== Asynchronous Example ===")
    
    manager = PromptManager()
    await manager.load_templates_from_file_async("example_prompts.yaml")
    
    template = manager.get_required_prompt("code-review")
    print(f"Template: {template.name}")
    print(f"System Message: {template.system_message}")
    print(f"Output Format: {template.output_format}")
    
    result = template.generate_prompt({
        "language": "Python",
        "code": "def hello():\n    print('world')",
        "focus_areas": "performance, readability, best practices"
    })
    print(f"Generated prompt:\n{result}")
    print()


def error_handling_example():
    """Demonstrate error handling."""
    print("=== Error Handling Example ===")
    
    manager = PromptManager()
    manager.load_templates_from_file("example_prompts.yaml")
    
    # Test PromptNotFoundException
    try:
        template = manager.get_required_prompt("nonexistent")
    except PromptNotFoundException as e:
        print(f"✓ Caught expected error: {e}")
    
    # Test MissingPlaceholderException
    try:
        template = manager.get_required_prompt("greeting")
        result = template.generate_prompt({"platform": "PromptSpec"})  # missing required 'name'
    except MissingPlaceholderException as e:
        print(f"✓ Caught expected error: {e}")
    
    # Test PlaceholderTypeException
    try:
        template = manager.get_required_prompt("greeting")
        result = template.generate_prompt({"name": 123, "platform": "PromptSpec"})  # name should be string
    except PlaceholderTypeException as e:
        print(f"✓ Caught expected error: {e}")
    print()


def advanced_example():
    """Demonstrate advanced features."""
    print("=== Advanced Example ===")
    
    manager = PromptManager()
    manager.load_templates_from_file("example_prompts.yaml")
    
    template = manager.get_required_prompt("creative-writing")
    
    # Show all placeholder names
    placeholders = template.get_placeholder_names()
    print(f"Placeholders in '{template.name}': {placeholders}")
    
    # Generate with optional placeholders
    result = template.generate_prompt({
        "genre": "sci-fi",
        "topic": "time travel",
        "length": "short",
        "tone": "mysterious"
    })
    print(f"Generated story prompt:\n{result}")
    
    # Show model config (if any)
    model_config = template.get_model_config()
    print(f"Model config: {model_config}")
    
    # Show parameters
    params = template.get_parameters()
    print(f"Parameters: temperature={params.temperature}, top_p={params.top_p}, max_tokens={params.max_tokens}")
    print()


async def main():
    """Run all examples."""
    print("PromptSpec Python Library - Example Usage")
    print("=" * 50)
    
    # Check if example file exists
    if not Path("example_prompts.yaml").exists():
        print("Error: example_prompts.yaml not found!")
        print("Make sure you're running this script from the correct directory.")
        return
    
    sync_example()
    await async_example()
    error_handling_example()
    advanced_example()
    
    print("✓ All examples completed successfully!")


if __name__ == "__main__":
    asyncio.run(main())