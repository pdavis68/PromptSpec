"""
Custom exceptions for the PromptSpec library.
"""


class PromptValidationException(Exception):
    """Exception thrown when prompt validation fails."""
    pass


class MissingPlaceholderException(PromptValidationException):
    """Exception thrown when a required placeholder is missing."""
    
    def __init__(self, placeholder_name: str):
        self.placeholder_name = placeholder_name
        super().__init__(f"Required placeholder '{placeholder_name}' is missing.")


class PlaceholderTypeException(PromptValidationException):
    """Exception thrown when a placeholder value has incorrect type."""
    
    def __init__(self, placeholder_name: str, expected_type: str, actual_type: str):
        self.placeholder_name = placeholder_name
        self.expected_type = expected_type
        self.actual_type = actual_type
        super().__init__(
            f"Placeholder '{placeholder_name}' expected type '{expected_type}' "
            f"but received '{actual_type}'."
        )


class PromptNotFoundException(Exception):
    """Exception thrown when a prompt template is not found."""
    
    def __init__(self, prompt_name: str):
        self.prompt_name = prompt_name
        super().__init__(f"Prompt template '{prompt_name}' not found.")