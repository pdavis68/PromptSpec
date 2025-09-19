"""
Manages loading and retrieving prompt templates from YAML configuration.
"""
import yaml
from pathlib import Path
from typing import Dict, Optional, List, Union
from ..models.prompt_collection import PromptCollection
from ..models.prompt_specification import PromptSpecification
from .prompt_template import PromptTemplate
from ..exceptions.prompt_exceptions import (
    PromptValidationException, 
    PromptNotFoundException
)


class PromptManager:
    """Manages loading and retrieving prompt templates from YAML configuration."""
    
    def __init__(self):
        self._templates: Dict[str, PromptTemplate] = {}
    
    @property
    def template_count(self) -> int:
        """Gets the count of loaded templates."""
        return len(self._templates)
    
    @property
    def template_names(self) -> List[str]:
        """Gets the names of all loaded templates."""
        return list(self._templates.keys())
    
    async def load_templates_async(self, yaml_content: str) -> None:
        """
        Parses YAML content and loads all prompts into memory.
        
        Args:
            yaml_content: The YAML content containing prompt definitions
            
        Raises:
            ValueError: When yaml_content is None or empty
            PromptValidationException: When YAML parsing or validation fails
        """
        if not yaml_content or not yaml_content.strip():
            raise ValueError("yaml_content cannot be None or empty")
        
        try:
            # Parse YAML content
            yaml_data = yaml.safe_load(yaml_content)
            
            if not yaml_data or 'prompts' not in yaml_data:
                raise PromptValidationException(
                    "Invalid YAML: No 'prompts' section found or it is null."
                )
            
            # Validate and create prompt collection using Pydantic
            prompt_collection = PromptCollection.model_validate(yaml_data)
            
            self._templates.clear()
            
            for specification in prompt_collection.prompts:
                self._validate_specification(specification)
                
                if specification.name in self._templates:
                    raise PromptValidationException(
                        f"Duplicate prompt name '{specification.name}' found."
                    )
                
                self._templates[specification.name] = PromptTemplate(specification)
                
        except yaml.YAMLError as e:
            raise PromptValidationException("Failed to parse YAML content.") from e
        except Exception as e:
            if isinstance(e, PromptValidationException):
                raise
            raise PromptValidationException("An error occurred while loading templates.") from e
    
    def load_templates(self, yaml_content: str) -> None:
        """Synchronous version of template loading."""
        if not yaml_content or not yaml_content.strip():
            raise ValueError("yaml_content cannot be None or empty")
        
        try:
            # Parse YAML content
            yaml_data = yaml.safe_load(yaml_content)
            
            if not yaml_data or 'prompts' not in yaml_data:
                raise PromptValidationException(
                    "Invalid YAML: No 'prompts' section found or it is null."
                )
            
            # Validate and create prompt collection using Pydantic
            prompt_collection = PromptCollection.model_validate(yaml_data)
            
            self._templates.clear()
            
            for specification in prompt_collection.prompts:
                self._validate_specification(specification)
                
                if specification.name in self._templates:
                    raise PromptValidationException(
                        f"Duplicate prompt name '{specification.name}' found."
                    )
                
                self._templates[specification.name] = PromptTemplate(specification)
                
        except yaml.YAMLError as e:
            raise PromptValidationException("Failed to parse YAML content.") from e
        except Exception as e:
            if isinstance(e, PromptValidationException):
                raise
            raise PromptValidationException("An error occurred while loading templates.") from e
    
    async def load_templates_from_file_async(self, file_path: Union[str, Path]) -> None:
        """
        Loads templates from a YAML file.
        
        Args:
            file_path: Path to the YAML file
            
        Raises:
            FileNotFoundError: When the specified file is not found
            PromptValidationException: When YAML parsing or validation fails
        """
        path = Path(file_path)
        
        if not path.exists():
            raise FileNotFoundError(f"Template file not found: {file_path}")
        
        yaml_content = path.read_text(encoding='utf-8')
        await self.load_templates_async(yaml_content)
    
    def load_templates_from_file(self, file_path: Union[str, Path]) -> None:
        """Synchronous version of template loading from file."""
        path = Path(file_path)
        
        if not path.exists():
            raise FileNotFoundError(f"Template file not found: {file_path}")
        
        yaml_content = path.read_text(encoding='utf-8')
        self.load_templates(yaml_content)
    
    def get_prompt(self, prompt_name: str) -> Optional[PromptTemplate]:
        """
        Retrieves a specific prompt template by its name.
        
        Args:
            prompt_name: The name of the prompt to retrieve
            
        Returns:
            The prompt template, or None if not found
            
        Raises:
            ValueError: When prompt_name is None or empty
        """
        if not prompt_name or not prompt_name.strip():
            raise ValueError("prompt_name cannot be None or empty")
        
        return self._templates.get(prompt_name)
    
    def get_required_prompt(self, prompt_name: str) -> PromptTemplate:
        """
        Retrieves a specific prompt template by name, raising exception if not found.
        
        Args:
            prompt_name: The name of the prompt to retrieve
            
        Returns:
            The prompt template
            
        Raises:
            ValueError: When prompt_name is None or empty
            PromptNotFoundException: When the prompt is not found
        """
        template = self.get_prompt(prompt_name)
        if template is None:
            raise PromptNotFoundException(prompt_name)
        return template
    
    def has_prompt(self, prompt_name: str) -> bool:
        """
        Checks if a prompt template with the specified name exists.
        
        Args:
            prompt_name: The name of the prompt to check
            
        Returns:
            True if the prompt exists, false otherwise
        """
        return bool(prompt_name and prompt_name.strip() and prompt_name in self._templates)
    
    def clear(self) -> None:
        """Clears all loaded templates."""
        self._templates.clear()
    
    def get_all_prompts(self) -> List[PromptTemplate]:
        """Gets all loaded prompt templates."""
        return list(self._templates.values())
    
    def _validate_specification(self, specification: PromptSpecification) -> None:
        """
        Validates a prompt specification for required fields and consistency.
        
        Args:
            specification: The specification to validate
            
        Raises:
            PromptValidationException: When validation fails
        """
        # Pydantic already validates basic structure, but add custom business logic
        if not specification.name.strip():
            raise PromptValidationException("Prompt name is required and cannot be empty.")
        
        if not specification.template.strip():
            raise PromptValidationException(
                f"Template is required for prompt '{specification.name}' and cannot be empty."
            )
        
        # Additional validation for placeholder definitions
        if specification.placeholders:
            for placeholder_name, definition in specification.placeholders.items():
                if not placeholder_name.strip():
                    raise PromptValidationException(
                        f"Placeholder name cannot be empty in prompt '{specification.name}'."
                    )