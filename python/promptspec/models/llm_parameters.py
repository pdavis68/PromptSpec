"""
Common LLM parameters that can be used across different models.
"""
from typing import Optional, List
from pydantic import BaseModel, Field, ConfigDict


class LLMParameters(BaseModel):
    """Common LLM parameters that can be used across different models."""
    
    model_config = ConfigDict(
        populate_by_name=True,
        alias_generator=lambda field_name: {
            'top_p': 'topP',
            'max_tokens': 'maxTokens',
            'stop_sequences': 'stopSequences'
        }.get(field_name, field_name)
    )
    
    temperature: Optional[float] = Field(
        None, 
        ge=0.0, 
        le=2.0, 
        description="Controls randomness of output (0-2.0)"
    )
    top_p: Optional[float] = Field(
        None, 
        ge=0.0, 
        le=1.0, 
        description="Nucleus sampling parameter (0-1.0)"
    )
    max_tokens: Optional[int] = Field(
        None, 
        gt=0, 
        description="Maximum number of tokens to generate"
    )
    stop_sequences: Optional[List[str]] = Field(
        None, 
        description="List of strings that stop generation when encountered"
    )