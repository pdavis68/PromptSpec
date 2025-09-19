"""
Complete prompt specification as defined in YAML.
"""
from typing import Optional, Dict, Any
from pydantic import BaseModel, Field, ConfigDict
from .llm_parameters import LLMParameters
from .placeholder_definition import PlaceholderDefinition


class PromptSpecification(BaseModel):
    """Complete prompt specification as defined in YAML."""
    
    model_config = ConfigDict(
        populate_by_name=True,
        alias_generator=lambda field_name: {
            'system_message': 'systemMessage',
            'llm_config': 'modelConfig',
            'output_format': 'outputFormat',
        }.get(field_name, field_name)
    )
    
    name: str = Field(..., description="Unique identifier for the prompt")
    version: Optional[str] = Field(None, description="Version of the prompt template")
    description: Optional[str] = Field(None, description="Human-readable description")
    system_message: Optional[str] = Field(None, description="System-level instructions")
    template: str = Field(..., description="Main prompt content with placeholders")
    parameters: Optional[LLMParameters] = Field(None, description="Common LLM parameters")
    llm_config: Optional[Dict[str, Any]] = Field(
        None, 
        description="Model-specific parameters as key-value map"
    )
    placeholders: Optional[Dict[str, PlaceholderDefinition]] = Field(
        None, 
        description="Placeholder definitions for validation"
    )
    output_format: Optional[str] = Field(
        None, 
        description="Desired output format (text, json, xml, etc.)"
    )