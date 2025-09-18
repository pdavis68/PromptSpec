using PromptSpec.Core;
using PromptSpec.Exceptions;
using PromptSpec.Models;

namespace PromptSpec.Tests;

public class PromptTemplateTests
{
    #region Helper Methods

    /// <summary>
    /// Creates a basic PromptSpecification for testing
    /// </summary>
    private static PromptSpecification CreateBasicSpecification(
        string name = "testPrompt",
        string template = "Hello {name}!")
    {
        return new PromptSpecification
        {
            Name = name,
            Template = template,
            Version = "1.0",
            Description = "Test prompt",
            SystemMessage = "You are a helpful assistant",
            OutputFormat = "text"
        };
    }

    /// <summary>
    /// Creates a PromptSpecification with parameters
    /// </summary>
    private static PromptSpecification CreateSpecificationWithParameters()
    {
        return new PromptSpecification
        {
            Name = "parameterizedPrompt",
            Template = "Generate content",
            Parameters = new LLMParameters
            {
                Temperature = 0.7,
                TopP = 0.9,
                MaxTokens = 100,
                StopSequences = new List<string> { "\n", "END" }
            }
        };
    }

    /// <summary>
    /// Creates a PromptSpecification with model config
    /// </summary>
    private static PromptSpecification CreateSpecificationWithModelConfig()
    {
        return new PromptSpecification
        {
            Name = "configuredPrompt",
            Template = "Test template",
            ModelConfig = new Dictionary<string, object>
            {
                { "model", "gpt-4" },
                { "stream", true },
                { "presence_penalty", 0.1 }
            }
        };
    }

    /// <summary>
    /// Creates a PromptSpecification with placeholder definitions
    /// </summary>
    private static PromptSpecification CreateSpecificationWithPlaceholders()
    {
        return new PromptSpecification
        {
            Name = "placeholderPrompt",
            Template = "Hello {name}, you are {age} years old and it is {active}.",
            Placeholders = new Dictionary<string, PlaceholderDefinition>
            {
                { "name", new PlaceholderDefinition { Type = "string", Required = true } },
                { "age", new PlaceholderDefinition { Type = "number", Required = true } },
                { "active", new PlaceholderDefinition { Type = "boolean", Required = false } }
            }
        };
    }

    /// <summary>
    /// Creates a PromptSpecification with complex template
    /// </summary>
    private static PromptSpecification CreateComplexSpecification()
    {
        return new PromptSpecification
        {
            Name = "complexPrompt",
            Template = @"
System: {system_instruction}
User: {user_input}
Context: {context}
Requirements: {requirements}
Output format: {format}
Additional notes: {notes}
",
            Placeholders = new Dictionary<string, PlaceholderDefinition>
            {
                { "system_instruction", new PlaceholderDefinition { Type = "string", Required = true } },
                { "user_input", new PlaceholderDefinition { Type = "string", Required = true } },
                { "context", new PlaceholderDefinition { Type = "string", Required = false } },
                { "requirements", new PlaceholderDefinition { Type = "string", Required = false } },
                { "format", new PlaceholderDefinition { Type = "string", Required = false } },
                { "notes", new PlaceholderDefinition { Type = "string", Required = false } }
            }
        };
    }

    #endregion

    #region Constructor and Property Tests

    [Fact]
    public void Constructor_WithValidSpecification_ShouldInitializeCorrectly()
    {
        // Arrange
        var specification = CreateBasicSpecification();

        // Act
        var template = new PromptTemplate(specification);

        // Assert
        template.Name.Should().Be("testPrompt");
        template.Version.Should().Be("1.0");
        template.Description.Should().Be("Test prompt");
        template.SystemMessage.Should().Be("You are a helpful assistant");
        template.OutputFormat.Should().Be("text");
    }

    [Fact]
    public void Constructor_WithNullSpecification_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Action act = () => new PromptTemplate(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("specification");
    }

    [Fact]
    public void Properties_WithNullValues_ShouldReturnNull()
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "minimal",
            Template = "test"
            // All other properties are null
        };

        // Act
        var template = new PromptTemplate(specification);

        // Assert
        template.Name.Should().Be("minimal");
        template.Version.Should().BeNull();
        template.Description.Should().BeNull();
        template.SystemMessage.Should().BeNull();
        template.OutputFormat.Should().BeNull();
    }

    [Fact]
    public void Properties_WithEmptyStrings_ShouldReturnEmptyStrings()
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "",
            Template = "",
            Version = "",
            Description = "",
            SystemMessage = "",
            OutputFormat = ""
        };

        // Act
        var template = new PromptTemplate(specification);

        // Assert
        template.Name.Should().Be("");
        template.Version.Should().Be("");
        template.Description.Should().Be("");
        template.SystemMessage.Should().Be("");
        template.OutputFormat.Should().Be("");
    }

    #endregion

    #region GeneratePrompt Tests

    [Fact]
    public void GeneratePrompt_WithValidReplacements_ShouldReplaceCorrectly()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());
        var replacements = new Dictionary<string, object>
        {
            { "name", "Alice" }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Alice!");
    }

    [Fact]
    public void GeneratePrompt_WithMultiplePlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "Hello {name}, welcome to {place} on {day}!");
        var template = new PromptTemplate(specification);
        var replacements = new Dictionary<string, object>
        {
            { "name", "Bob" },
            { "place", "Paris" },
            { "day", "Monday" }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Bob, welcome to Paris on Monday!");
    }

    [Fact]
    public void GeneratePrompt_WithNullReplacements_ShouldThrowArgumentNullException()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());

        // Act & Assert
        template.Invoking(t => t.GeneratePrompt(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GeneratePrompt_WithEmptyReplacements_ShouldLeaveUnknownPlaceholdersEmpty()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());
        var replacements = new Dictionary<string, object>();

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello !");
    }

    [Fact]
    public void GeneratePrompt_WithMissingRequiredPlaceholder_ShouldThrowMissingPlaceholderException()
    {
        // Arrange
        var template = new PromptTemplate(CreateSpecificationWithPlaceholders());
        var replacements = new Dictionary<string, object>
        {
            { "age", 25 } // Missing required "name"
        };

        // Act & Assert
        template.Invoking(t => t.GeneratePrompt(replacements))
            .Should().Throw<MissingPlaceholderException>()
            .WithMessage("Required placeholder 'name' is missing.")
            .Which.PlaceholderName.Should().Be("name");
    }

    [Fact]
    public void GeneratePrompt_WithWrongPlaceholderType_ShouldThrowPlaceholderTypeException()
    {
        // Arrange
        var template = new PromptTemplate(CreateSpecificationWithPlaceholders());
        var replacements = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "age", "twenty-five" } // Should be number, not string
        };

        // Act & Assert
        template.Invoking(t => t.GeneratePrompt(replacements))
            .Should().Throw<PlaceholderTypeException>()
            .WithMessage("Placeholder 'age' expected type 'number' but received 'string'.")
            .Which.PlaceholderName.Should().Be("age");
    }

    [Fact]
    public void GeneratePrompt_WithCorrectTypes_ShouldSucceed()
    {
        // Arrange
        var template = new PromptTemplate(CreateSpecificationWithPlaceholders());
        var replacements = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "age", 25 },
            { "active", true }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Alice, you are 25 years old and it is True.");
    }

    [Fact]
    public void GeneratePrompt_WithOptionalPlaceholderMissing_ShouldUseEmptyString()
    {
        // Arrange
        var template = new PromptTemplate(CreateSpecificationWithPlaceholders());
        var replacements = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "age", 25 }
            // Missing optional "active" placeholder
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Alice, you are 25 years old and it is .");
    }

    [Fact]
    public void GeneratePrompt_WithNullValue_ShouldUseEmptyString()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());
        var replacements = new Dictionary<string, object>
        {
            { "name", null! }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello !");
    }

    [Fact]
    public void GeneratePrompt_WithNoPlaceholders_ShouldReturnOriginalTemplate()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "This is a simple template without placeholders.");
        var template = new PromptTemplate(specification);
        var replacements = new Dictionary<string, object>();

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("This is a simple template without placeholders.");
    }

    [Theory]
    [InlineData(42)]
    [InlineData(42L)]
    [InlineData(42.5f)]
    [InlineData(42.5d)]
    public void GeneratePrompt_WithDifferentNumericTypes_ShouldBeValidAsNumber(object numericValue)
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "numericTest",
            Template = "The value is {value}",
            Placeholders = new Dictionary<string, PlaceholderDefinition>
            {
                { "value", new PlaceholderDefinition { Type = "number", Required = true } }
            }
        };
        var template = new PromptTemplate(specification);
        var replacements = new Dictionary<string, object>
        {
            { "value", numericValue }
        };

        // Act & Assert
        template.Invoking(t => t.GeneratePrompt(replacements))
            .Should().NotThrow();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GeneratePrompt_WithBooleanValues_ShouldBeValidAsBoolean(bool boolValue)
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "booleanTest",
            Template = "The flag is {flag}",
            Placeholders = new Dictionary<string, PlaceholderDefinition>
            {
                { "flag", new PlaceholderDefinition { Type = "boolean", Required = true } }
            }
        };
        var template = new PromptTemplate(specification);
        var replacements = new Dictionary<string, object>
        {
            { "flag", boolValue }
        };

        // Act & Assert
        template.Invoking(t => t.GeneratePrompt(replacements))
            .Should().NotThrow();
    }

    #endregion

    #region GetParameters Tests

    [Fact]
    public void GetParameters_WithNullParameters_ShouldReturnEmptyLLMParameters()
    {
        // Arrange
        var specification = CreateBasicSpecification();
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetParameters();

        // Assert
        result.Should().NotBeNull();
        result.Temperature.Should().BeNull();
        result.TopP.Should().BeNull();
        result.MaxTokens.Should().BeNull();
        result.StopSequences.Should().BeNull();
    }

    [Fact]
    public void GetParameters_WithPopulatedParameters_ShouldReturnCorrectValues()
    {
        // Arrange
        var template = new PromptTemplate(CreateSpecificationWithParameters());

        // Act
        var result = template.GetParameters();

        // Assert
        result.Should().NotBeNull();
        result.Temperature.Should().Be(0.7);
        result.TopP.Should().Be(0.9);
        result.MaxTokens.Should().Be(100);
        result.StopSequences.Should().Equal("\n", "END");
    }

    [Fact]
    public void GetParameters_WithPartialParameters_ShouldReturnPartialValues()
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "partialParams",
            Template = "test",
            Parameters = new LLMParameters
            {
                Temperature = 0.5
                // Other parameters are null
            }
        };
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetParameters();

        // Assert
        result.Temperature.Should().Be(0.5);
        result.TopP.Should().BeNull();
        result.MaxTokens.Should().BeNull();
        result.StopSequences.Should().BeNull();
    }

    #endregion

    #region GetModelConfig Tests

    [Fact]
    public void GetModelConfig_WithNullConfig_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());

        // Act
        var result = template.GetModelConfig();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetModelConfig_WithPopulatedConfig_ShouldReturnCorrectValues()
    {
        // Arrange
        var template = new PromptTemplate(CreateSpecificationWithModelConfig());

        // Act
        var result = template.GetModelConfig();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result["model"].Should().Be("gpt-4");
        result["stream"].Should().Be(true);
        result["presence_penalty"].Should().Be(0.1);
    }

    [Fact]
    public void GetModelConfig_WithEmptyConfig_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "emptyConfig",
            Template = "test",
            ModelConfig = new Dictionary<string, object>()
        };
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetModelConfig();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetTemplate Tests

    [Fact]
    public void GetTemplate_ShouldReturnRawTemplate()
    {
        // Arrange
        var rawTemplate = "Hello {name}, welcome to {place}!";
        var specification = CreateBasicSpecification(template: rawTemplate);
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetTemplate();

        // Assert
        result.Should().Be(rawTemplate);
    }

    [Fact]
    public void GetTemplate_WithEmptyTemplate_ShouldReturnEmptyString()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "");
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetTemplate();

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void GetTemplate_WithComplexTemplate_ShouldReturnExactString()
    {
        // Arrange
        var complexTemplate = @"
System: {system}
User: {user}
{special_chars}: !@#$%^&*()
{nested}: {something}
";
        var specification = CreateBasicSpecification(template: complexTemplate);
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetTemplate();

        // Assert
        result.Should().Be(complexTemplate);
    }

    #endregion

    #region GetPlaceholderNames Tests

    [Fact]
    public void GetPlaceholderNames_WithNoPlaceholders_ShouldReturnEmptyList()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "This template has no placeholders.");
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPlaceholderNames_WithSinglePlaceholder_ShouldReturnSingleItem()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain("name");
    }

    [Fact]
    public void GetPlaceholderNames_WithMultiplePlaceholders_ShouldReturnAllUnique()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "Hello {name}, welcome to {place} on {day}!");
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(new[] { "name", "place", "day" });
    }

    [Fact]
    public void GetPlaceholderNames_WithDuplicatePlaceholders_ShouldReturnUniqueOnly()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "Hello {name}, goodbye {name}!");
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain("name");
    }

    [Fact]
    public void GetPlaceholderNames_WithComplexTemplate_ShouldFindAllPlaceholders()
    {
        // Arrange
        var template = new PromptTemplate(CreateComplexSpecification());

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().HaveCount(6);
        result.Should().Contain(new[] { "system_instruction", "user_input", "context", "requirements", "format", "notes" });
    }

    [Fact]
    public void GetPlaceholderNames_WithMalformedBraces_ShouldExtractWhatItCan()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "Hello {name}, welcome to {incomplete and {another}!");
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(new[] { "name", "incomplete and {another" });
    }

    [Fact]
    public void GetPlaceholderNames_WithNestedBraces_ShouldHandleCorrectly()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "Value: {outer} and {complex}");
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(new[] { "outer", "complex" });
    }

    [Fact]
    public void GetPlaceholderNames_WithEmptyPlaceholder_ShouldIgnoreEmpty()
    {
        // Arrange - The regex requires at least one character, so {} is ignored
        var specification = CreateBasicSpecification(template: "Hello {name} and {}!");
        var template = new PromptTemplate(specification);

        // Act
        var result = template.GetPlaceholderNames();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain("name");
    }

    #endregion

    #region Edge Cases and Boundary Conditions

    [Fact]
    public void GeneratePrompt_WithSpecialCharactersInPlaceholderValue_ShouldHandleCorrectly()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());
        var replacements = new Dictionary<string, object>
        {
            { "name", "Alice & Bob (2024) - 100% Success!" }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Alice & Bob (2024) - 100% Success!!");
    }

    [Fact]
    public void GeneratePrompt_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());
        var replacements = new Dictionary<string, object>
        {
            { "name", "José 🌟 Müller 中文" }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello José 🌟 Müller 中文!");
    }

    [Fact]
    public void GeneratePrompt_WithLargeString_ShouldHandleCorrectly()
    {
        // Arrange
        var largeString = new string('A', 10000);
        var template = new PromptTemplate(CreateBasicSpecification());
        var replacements = new Dictionary<string, object>
        {
            { "name", largeString }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be($"Hello {largeString}!");
    }

    [Fact]
    public void GeneratePrompt_WithPlaceholderNameContainingSpecialChars_ShouldMatch()
    {
        // Arrange
        var specification = CreateBasicSpecification(template: "Hello {user_name} and {user-id}!");
        var template = new PromptTemplate(specification);
        var replacements = new Dictionary<string, object>
        {
            { "user_name", "Alice" },
            { "user-id", "123" }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Alice and 123!");
    }

    [Fact]
    public void GeneratePrompt_WithObjectType_ShouldUseToStringMethod()
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "objectTest",
            Template = "Object: {obj}",
            Placeholders = new Dictionary<string, PlaceholderDefinition>
            {
                { "obj", new PlaceholderDefinition { Type = "string", Required = true } }
            }
        };
        var template = new PromptTemplate(specification);
        var testObject = new { Name = "Test", Value = 42 };
        var replacements = new Dictionary<string, object>
        {
            { "obj", testObject }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be($"Object: {testObject}");
    }

    [Fact]
    public void GeneratePrompt_WithZeroValues_ShouldHandleCorrectly()
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "zeroTest",
            Template = "Integer: {int}, Double: {double}, Boolean: {bool}",
            Placeholders = new Dictionary<string, PlaceholderDefinition>
            {
                { "int", new PlaceholderDefinition { Type = "number", Required = true } },
                { "double", new PlaceholderDefinition { Type = "number", Required = true } },
                { "bool", new PlaceholderDefinition { Type = "boolean", Required = true } }
            }
        };
        var template = new PromptTemplate(specification);
        var replacements = new Dictionary<string, object>
        {
            { "int", 0 },
            { "double", 0.0 },
            { "bool", false }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Integer: 0, Double: 0, Boolean: False");
    }

    [Fact]
    public void GeneratePrompt_WithCaseInsensitivePlaceholderNames_ShouldBeExactMatch()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification(template: "Hello {Name}!"));
        var replacements = new Dictionary<string, object>
        {
            { "name", "Alice" }, // lowercase 'name' vs uppercase 'Name' in template
            { "Name", "Bob" }    // exact match
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Bob!"); // Should use exact match
    }

    [Fact]
    public void GeneratePrompt_WithExtraReplacements_ShouldIgnoreExtra()
    {
        // Arrange
        var template = new PromptTemplate(CreateBasicSpecification());
        var replacements = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "extra", "ignored" },
            { "another", "also ignored" }
        };

        // Act
        var result = template.GeneratePrompt(replacements);

        // Assert
        result.Should().Be("Hello Alice!");
    }

    [Fact]
    public void ValidationMethods_WithMixedCaseTypes_ShouldBeCaseInsensitive()
    {
        // Arrange
        var specification = new PromptSpecification
        {
            Name = "mixedCaseTest",
            Template = "Value: {value}",
            Placeholders = new Dictionary<string, PlaceholderDefinition>
            {
                { "value", new PlaceholderDefinition { Type = "STRING", Required = true } } // Uppercase type
            }
        };
        var template = new PromptTemplate(specification);
        var replacements = new Dictionary<string, object>
        {
            { "value", "test string" }
        };

        // Act & Assert
        template.Invoking(t => t.GeneratePrompt(replacements))
            .Should().NotThrow();
    }

    #endregion
}