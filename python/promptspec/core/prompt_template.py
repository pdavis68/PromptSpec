"""
Represents a loaded prompt template with methods to generate final prompt string.
"""
import re
from typing import Dict, Any, List, Optional
from ..models.prompt_specification import PromptSpecification
from ..models.llm_parameters import LLMParameters
from ..exceptions.prompt_exceptions import (
    MissingPlaceholderException, 
    PlaceholderTypeException
)


class PromptTemplate:
    """Represents a loaded prompt template with methods to generate final prompt string."""
    
    _PLACEHOLDER_REGEX = re.compile(r'\{([^}]+)\}')
    
    def __init__(self, specification: PromptSpecification):
        if specification is None:
            raise ValueError("specification cannot be None")
        self._specification = specification
    
    @property
    def name(self) -> str:
        """Gets the name of the prompt template."""
        return self._specification.name
    
    @property
    def version(self) -> Optional[str]:
        """Gets the version of the prompt template."""
        return self._specification.version
    
    @property
    def description(self) -> Optional[str]:
        """Gets the description of the prompt template."""
        return self._specification.description
    
    @property
    def system_message(self) -> Optional[str]:
        """Gets the system message for the prompt template."""
        return self._specification.system_message
    
    @property
    def output_format(self) -> Optional[str]:
        """Gets the output format for the prompt template."""
        return self._specification.output_format
    
    def generate_prompt(self, replacements: Dict[str, Any]) -> str:
        """
        Fills template with provided replacement values and performs validation.
        
        Args:
            replacements: Dictionary containing placeholder values
            
        Returns:
            Generated prompt string with placeholders replaced
            
        Raises:
            MissingPlaceholderException: When a required placeholder is missing
            PlaceholderTypeException: When a placeholder value has incorrect type
        """
        if replacements is None:
            raise ValueError("replacements cannot be None")
        
        # Validate placeholders if definitions exist
        if self._specification.placeholders:
            self._validate_placeholders(replacements)
        
        # Replace placeholders in the template
        def replace_placeholder(match) -> str:
            placeholder_name = match.group(1)
            
            if placeholder_name in replacements:
                value = replacements[placeholder_name]
                return str(value) if value is not None else ""
            
            # Check if placeholder is required
            if (self._specification.placeholders and 
                placeholder_name in self._specification.placeholders):
                definition = self._specification.placeholders[placeholder_name]
                if definition.required:
                    raise MissingPlaceholderException(placeholder_name)
            
            return ""
        
        return self._PLACEHOLDER_REGEX.sub(replace_placeholder, self._specification.template)
    
    def get_parameters(self) -> LLMParameters:
        """Returns the LLM parameters for this prompt template."""
        return self._specification.parameters or LLMParameters()
    
    def get_model_config(self) -> Dict[str, Any]:
        """Returns the model-specific configuration parameters."""
        return self._specification.llm_config or {}
    
    def get_template(self) -> str:
        """Gets the raw template string."""
        return self._specification.template
    
    def get_placeholder_names(self) -> List[str]:
        """Gets all placeholder names found in the template."""
        matches = self._PLACEHOLDER_REGEX.findall(self._specification.template)
        return list(set(matches))  # Remove duplicates
    
    def _validate_placeholders(self, replacements: Dict[str, Any]) -> None:
        """Validates that provided replacements match placeholder definitions."""
        if not self._specification.placeholders:
            return
        
        for placeholder_name, definition in self._specification.placeholders.items():
            # Check if required placeholder is missing
            if definition.required and placeholder_name not in replacements:
                raise MissingPlaceholderException(placeholder_name)
            
            # Validate type if placeholder is provided
            if placeholder_name in replacements:
                value = replacements[placeholder_name]
                if value is not None:
                    self._validate_type(placeholder_name, value, definition.type)
    
    def _validate_type(self, placeholder_name: str, value: Any, expected_type: str) -> None:
        """Validates that a value matches the expected type."""
        actual_type = self._get_value_type(value)
        
        if expected_type.lower() != actual_type.lower():
            raise PlaceholderTypeException(placeholder_name, expected_type, actual_type)
    
    def _get_value_type(self, value: Any) -> str:
        """Determines the type name of a value for validation purposes."""
        if isinstance(value, str):
            return "string"
        elif isinstance(value, (int, float)):
            return "number"
        elif isinstance(value, bool):
            return "boolean"
        else:
            return "string"  # Default to string for other types