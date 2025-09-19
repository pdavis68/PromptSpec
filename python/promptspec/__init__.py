"""
PromptSpec - A Python library for managing prompt templates with YAML configuration.
"""

from .core.prompt_manager import PromptManager
from .core.prompt_template import PromptTemplate
from .models.llm_parameters import LLMParameters
from .models.placeholder_definition import PlaceholderDefinition, PlaceholderType
from .models.prompt_specification import PromptSpecification
from .models.prompt_collection import PromptCollection
from .exceptions.prompt_exceptions import (
    PromptValidationException,
    MissingPlaceholderException,
    PlaceholderTypeException,
    PromptNotFoundException
)

__version__ = "1.0.0"
__all__ = [
    "PromptManager",
    "PromptTemplate", 
    "LLMParameters",
    "PlaceholderDefinition",
    "PlaceholderType",
    "PromptSpecification",
    "PromptCollection",
    "PromptValidationException",
    "MissingPlaceholderException", 
    "PlaceholderTypeException",
    "PromptNotFoundException"
]