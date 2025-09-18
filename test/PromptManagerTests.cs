using PromptSpec.Core;
using PromptSpec.Exceptions;
using PromptSpec.Models;
using Moq;

namespace PromptSpec.Tests;

public class PromptManagerTests
{
    private PromptManager _promptManager;

    public PromptManagerTests()
    {
        _promptManager = new PromptManager();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a valid YAML content for testing
    /// </summary>
    private static string CreateValidYaml(string promptName = "testPrompt", string template = "Hello {{name}}")
    {
        return $@"
prompts:
  - name: {promptName}
    template: ""{template}""
    description: ""Test prompt""
    version: ""1.0""
    parameters:
      temperature: 0.7
      maxTokens: 100
    placeholders:
      name:
        type: string
        description: ""User name""
        required: true";
    }

    /// <summary>
    /// Creates YAML with multiple prompts
    /// </summary>
    private static string CreateMultiPromptYaml()
    {
        return @"
prompts:
  - name: prompt1
    template: ""Hello {{name}}""
    description: ""First prompt""
  - name: prompt2
    template: ""Goodbye {{name}}""
    description: ""Second prompt""
  - name: prompt3
    template: ""Welcome {{user}}""
    description: ""Third prompt""";
    }

    /// <summary>
    /// Creates YAML with invalid temperature
    /// </summary>
    private static string CreateInvalidTemperatureYaml()
    {
        return @"
prompts:
  - name: testPrompt
    template: ""Hello world""
    parameters:
      temperature: 3.0";
    }

    /// <summary>
    /// Creates YAML with invalid topP
    /// </summary>
    private static string CreateInvalidTopPYaml()
    {
        return @"
prompts:
  - name: testPrompt
    template: ""Hello world""
    parameters:
      topP: 1.5";
    }

    /// <summary>
    /// Creates YAML with invalid maxTokens
    /// </summary>
    private static string CreateInvalidMaxTokensYaml()
    {
        return @"
prompts:
  - name: testPrompt
    template: ""Hello world""
    parameters:
      maxTokens: -1";
    }

    /// <summary>
    /// Creates YAML with invalid placeholder type
    /// </summary>
    private static string CreateInvalidPlaceholderTypeYaml()
    {
        return @"
prompts:
  - name: testPrompt
    template: ""Hello {{name}}""
    placeholders:
      name:
        type: invalidType
        description: ""Invalid type""";
    }

    /// <summary>
    /// Creates YAML with duplicate prompt names
    /// </summary>
    private static string CreateDuplicatePromptYaml()
    {
        return @"
prompts:
  - name: duplicate
    template: ""First template""
  - name: duplicate
    template: ""Second template""";
    }

    /// <summary>
    /// Creates a temporary file with the given content
    /// </summary>
    private static string CreateTempFile(string content)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, content);
        return tempFile;
    }

    #endregion

    #region Constructor and Properties Tests

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyTemplates()
    {
        // Arrange & Act
        var manager = new PromptManager();

        // Assert
        manager.TemplateCount.Should().Be(0);
        manager.TemplateNames.Should().BeEmpty();
    }

    [Fact]
    public void TemplateCount_WhenEmpty_ShouldReturnZero()
    {
        // Arrange & Act & Assert
        _promptManager.TemplateCount.Should().Be(0);
    }

    [Fact]
    public void TemplateNames_WhenEmpty_ShouldReturnEmptyCollection()
    {
        // Arrange & Act & Assert
        _promptManager.TemplateNames.Should().BeEmpty();
    }

    #endregion

    #region LoadTemplatesAsync Tests

    [Fact]
    public async Task LoadTemplatesAsync_WithValidYaml_ShouldLoadTemplate()
    {
        // Arrange
        var yamlContent = CreateValidYaml();

        // Act
        await _promptManager.LoadTemplatesAsync(yamlContent);

        // Assert
        _promptManager.TemplateCount.Should().Be(1);
        _promptManager.TemplateNames.Should().Contain("testPrompt");
        _promptManager.HasPrompt("testPrompt").Should().BeTrue();
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithMultiplePrompts_ShouldLoadAllTemplates()
    {
        // Arrange
        var yamlContent = CreateMultiPromptYaml();

        // Act
        await _promptManager.LoadTemplatesAsync(yamlContent);

        // Assert
        _promptManager.TemplateCount.Should().Be(3);
        _promptManager.TemplateNames.Should().Contain(new[] { "prompt1", "prompt2", "prompt3" });
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithNullContent_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(null!))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithWhitespaceContent_ShouldThrowPromptValidationException()
    {
        // Arrange & Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync("   "))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Invalid YAML: No 'prompts' section found or it is null.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithInvalidYaml_ShouldThrowPromptValidationException()
    {
        // Arrange
        var invalidYaml = "invalid: yaml: content: [";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(invalidYaml))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Failed to parse YAML content.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithNoPromptsSection_ShouldLoadSuccessfully()
    {
        // Arrange
        var yamlWithoutPrompts = "other: data";

        // Act & Assert - This should succeed because the deserializer creates an empty PromptCollection
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlWithoutPrompts))
            .Should().NotThrowAsync();
        
        _promptManager.TemplateCount.Should().Be(0);
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithNullPromptsSection_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlWithNullPrompts = "prompts: null";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlWithNullPrompts))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Invalid YAML: No 'prompts' section found or it is null.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithDuplicatePromptNames_ShouldThrowPromptValidationException()
    {
        // Arrange
        var duplicateYaml = CreateDuplicatePromptYaml();

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(duplicateYaml))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Duplicate prompt name 'duplicate' found.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithEmptyPromptName_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlWithEmptyName = @"
prompts:
  - name: """"
    template: ""Hello world""";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlWithEmptyName))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Prompt name is required and cannot be empty.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithEmptyTemplate_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlWithEmptyTemplate = @"
prompts:
  - name: testPrompt
    template: """"";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlWithEmptyTemplate))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Template is required for prompt 'testPrompt' and cannot be empty.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithInvalidTemperature_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlContent = CreateInvalidTemperatureYaml();

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Temperature for prompt 'testPrompt' must be between 0 and 2.0.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithInvalidTopP_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlContent = CreateInvalidTopPYaml();

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("TopP for prompt 'testPrompt' must be between 0 and 1.0.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithInvalidMaxTokens_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlContent = CreateInvalidMaxTokensYaml();

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("MaxTokens for prompt 'testPrompt' must be greater than 0.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithInvalidPlaceholderType_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlContent = CreateInvalidPlaceholderTypeYaml();

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Invalid placeholder type 'invalidType' for placeholder 'name' in prompt 'testPrompt'. Valid types are: string, number, boolean.");
    }

    [Fact]
    public async Task LoadTemplatesAsync_CalledTwice_ShouldClearPreviousTemplates()
    {
        // Arrange
        var firstYaml = CreateValidYaml("firstPrompt");
        var secondYaml = CreateValidYaml("secondPrompt");

        // Act
        await _promptManager.LoadTemplatesAsync(firstYaml);
        await _promptManager.LoadTemplatesAsync(secondYaml);

        // Assert
        _promptManager.TemplateCount.Should().Be(1);
        _promptManager.HasPrompt("firstPrompt").Should().BeFalse();
        _promptManager.HasPrompt("secondPrompt").Should().BeTrue();
    }

    #endregion

    #region LoadTemplatesFromFileAsync Tests

    [Fact]
    public async Task LoadTemplatesFromFileAsync_WithValidFile_ShouldLoadTemplates()
    {
        // Arrange
        var yamlContent = CreateValidYaml();
        var tempFile = CreateTempFile(yamlContent);

        try
        {
            // Act
            await _promptManager.LoadTemplatesFromFileAsync(tempFile);

            // Assert
            _promptManager.TemplateCount.Should().Be(1);
            _promptManager.HasPrompt("testPrompt").Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadTemplatesFromFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = "/path/to/nonexistent/file.yaml";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesFromFileAsync(nonExistentFile))
            .Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"Template file not found: {nonExistentFile}");
    }

    [Fact]
    public async Task LoadTemplatesFromFileAsync_WithInvalidFileContent_ShouldThrowPromptValidationException()
    {
        // Arrange
        var invalidContent = "invalid: yaml: content: [";
        var tempFile = CreateTempFile(invalidContent);

        try
        {
            // Act & Assert
            await _promptManager.Invoking(pm => pm.LoadTemplatesFromFileAsync(tempFile))
                .Should().ThrowAsync<PromptValidationException>();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region GetPrompt Tests

    [Fact]
    public async Task GetPrompt_WithExistingPrompt_ShouldReturnTemplate()
    {
        // Arrange
        var yamlContent = CreateValidYaml();
        await _promptManager.LoadTemplatesAsync(yamlContent);

        // Act
        var result = _promptManager.GetPrompt("testPrompt");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("testPrompt");
    }

    [Fact]
    public void GetPrompt_WithNonExistentPrompt_ShouldReturnNull()
    {
        // Arrange & Act
        var result = _promptManager.GetPrompt("nonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetPrompt_WithNullPromptName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        _promptManager.Invoking(pm => pm.GetPrompt(null!))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetPrompt_WithEmptyPromptName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        _promptManager.Invoking(pm => pm.GetPrompt(string.Empty))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetPrompt_WithWhitespacePromptName_ShouldReturnNull()
    {
        // Arrange & Act
        var result = _promptManager.GetPrompt("   ");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetRequiredPrompt Tests

    [Fact]
    public async Task GetRequiredPrompt_WithExistingPrompt_ShouldReturnTemplate()
    {
        // Arrange
        var yamlContent = CreateValidYaml();
        await _promptManager.LoadTemplatesAsync(yamlContent);

        // Act
        var result = _promptManager.GetRequiredPrompt("testPrompt");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("testPrompt");
    }

    [Fact]
    public void GetRequiredPrompt_WithNonExistentPrompt_ShouldThrowPromptNotFoundException()
    {
        // Arrange & Act & Assert
        _promptManager.Invoking(pm => pm.GetRequiredPrompt("nonExistent"))
            .Should().Throw<PromptNotFoundException>()
            .WithMessage("Prompt template 'nonExistent' not found.");
    }

    [Fact]
    public void GetRequiredPrompt_WithNullPromptName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        _promptManager.Invoking(pm => pm.GetRequiredPrompt(null!))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetRequiredPrompt_WithEmptyPromptName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        _promptManager.Invoking(pm => pm.GetRequiredPrompt(string.Empty))
            .Should().Throw<ArgumentException>();
    }

    #endregion

    #region HasPrompt Tests

    [Fact]
    public async Task HasPrompt_WithExistingPrompt_ShouldReturnTrue()
    {
        // Arrange
        var yamlContent = CreateValidYaml();
        await _promptManager.LoadTemplatesAsync(yamlContent);

        // Act & Assert
        _promptManager.HasPrompt("testPrompt").Should().BeTrue();
    }

    [Fact]
    public void HasPrompt_WithNonExistentPrompt_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        _promptManager.HasPrompt("nonExistent").Should().BeFalse();
    }

    [Fact]
    public void HasPrompt_WithNullPromptName_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        _promptManager.HasPrompt(null!).Should().BeFalse();
    }

    [Fact]
    public void HasPrompt_WithEmptyPromptName_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        _promptManager.HasPrompt(string.Empty).Should().BeFalse();
    }

    [Fact]
    public void HasPrompt_WithWhitespacePromptName_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        _promptManager.HasPrompt("   ").Should().BeFalse();
    }

    #endregion

    #region Clear and GetAllPrompts Tests

    [Fact]
    public async Task Clear_WithLoadedTemplates_ShouldRemoveAllTemplates()
    {
        // Arrange
        var yamlContent = CreateMultiPromptYaml();
        await _promptManager.LoadTemplatesAsync(yamlContent);

        // Act
        _promptManager.Clear();

        // Assert
        _promptManager.TemplateCount.Should().Be(0);
        _promptManager.TemplateNames.Should().BeEmpty();
        _promptManager.GetAllPrompts().Should().BeEmpty();
    }

    [Fact]
    public void Clear_WithEmptyTemplates_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        _promptManager.Invoking(pm => pm.Clear()).Should().NotThrow();
        _promptManager.TemplateCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllPrompts_WithLoadedTemplates_ShouldReturnAllTemplates()
    {
        // Arrange
        var yamlContent = CreateMultiPromptYaml();
        await _promptManager.LoadTemplatesAsync(yamlContent);

        // Act
        var result = _promptManager.GetAllPrompts().ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Select(t => t.Name).Should().Contain(new[] { "prompt1", "prompt2", "prompt3" });
    }

    [Fact]
    public void GetAllPrompts_WithEmptyTemplates_ShouldReturnEmptyCollection()
    {
        // Arrange & Act
        var result = _promptManager.GetAllPrompts();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task LoadTemplatesAsync_WithValidTemperatureValues_ShouldSucceed(double temperature)
    {
        // Arrange
        var yamlContent = $@"
prompts:
  - name: testPrompt
    template: ""Hello world""
    parameters:
      temperature: {temperature}";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(2.1)]
    public async Task LoadTemplatesAsync_WithInvalidTemperatureValues_ShouldThrowException(double temperature)
    {
        // Arrange
        var yamlContent = $@"
prompts:
  - name: testPrompt
    template: ""Hello world""
    parameters:
      temperature: {temperature}";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public async Task LoadTemplatesAsync_WithValidTopPValues_ShouldSucceed(double topP)
    {
        // Arrange
        var yamlContent = $@"
prompts:
  - name: testPrompt
    template: ""Hello world""
    parameters:
      topP: {topP}";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public async Task LoadTemplatesAsync_WithInvalidTopPValues_ShouldThrowException(double topP)
    {
        // Arrange
        var yamlContent = $@"
prompts:
  - name: testPrompt
    template: ""Hello world""
    parameters:
      topP: {topP}";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>();
    }

    [Theory]
    [InlineData("string")]
    [InlineData("number")]
    [InlineData("boolean")]
    [InlineData("STRING")]
    [InlineData("Number")]
    [InlineData("Boolean")]
    public async Task LoadTemplatesAsync_WithValidPlaceholderTypes_ShouldSucceed(string type)
    {
        // Arrange
        var yamlContent = $@"
prompts:
  - name: testPrompt
    template: ""Hello {{{{name}}}}""
    placeholders:
      name:
        type: {type}
        description: ""Test placeholder""";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("integer")]
    [InlineData("float")]
    [InlineData("object")]
    [InlineData("array")]
    [InlineData("invalid")]
    public async Task LoadTemplatesAsync_WithInvalidPlaceholderTypes_ShouldThrowException(string type)
    {
        // Arrange
        var yamlContent = $@"
prompts:
  - name: testPrompt
    template: ""Hello {{{{name}}}}""
    placeholders:
      name:
        type: {type}
        description: ""Test placeholder""";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>();
    }

    [Fact]
    public async Task LoadTemplatesAsync_WithEmptyPlaceholderName_ShouldThrowPromptValidationException()
    {
        // Arrange
        var yamlContent = @"
prompts:
  - name: testPrompt
    template: ""Hello world""
    placeholders:
      """":
        type: string
        description: ""Empty name""";

        // Act & Assert
        await _promptManager.Invoking(pm => pm.LoadTemplatesAsync(yamlContent))
            .Should().ThrowAsync<PromptValidationException>()
            .WithMessage("Placeholder name cannot be empty in prompt 'testPrompt'.");
    }

    #endregion
}