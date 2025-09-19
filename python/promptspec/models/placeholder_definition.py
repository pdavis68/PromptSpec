"""
Placeholder definition for validation rules.
"""
from typing import Literal
from pydantic import BaseModel, Field

PlaceholderType = Literal["string", "number", "boolean"]


class PlaceholderDefinition(BaseModel):
    """Defines validation rules for a placeholder in a prompt template."""
    
    type: PlaceholderType = Field(
        "string", 
        description="Expected data type for this placeholder"
    )
    required: bool = Field(
        False, 
        description="Whether this placeholder is required"
    )