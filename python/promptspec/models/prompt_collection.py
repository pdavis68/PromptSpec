"""
Root container for all prompt specifications.
"""
from typing import List
from pydantic import BaseModel, Field
from .prompt_specification import PromptSpecification


class PromptCollection(BaseModel):
    """Root container for all prompt specifications loaded from YAML."""
    
    prompts: List[PromptSpecification] = Field(
        default_factory=list, 
        description="Collection of prompt specifications"
    )