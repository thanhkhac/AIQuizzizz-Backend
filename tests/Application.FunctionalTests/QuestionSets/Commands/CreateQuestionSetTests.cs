using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;

using static Testing;

public class CreateQuestionSetTests : BaseTestFixture
{
    [Test]
    [TestCaseSource(nameof(InvalidQuestions))]
    public async Task ShouldRejectInvalidQuestion(CreateUpdateQuestionDto invalidQuestion)
    {
        await RunAsDefaultUserAsync();

        var command = new CreateQuestionSetCommand
        {
            Name = "Tên bộ đề",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                invalidQuestion
            }
        };

        Console.WriteLine("=== Invalid Input ===");
        Console.WriteLine(JsonSerializer.Serialize(invalidQuestion, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }));

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    public static IEnumerable<TestCaseData> InvalidQuestions()
    {
        // QuestionText validation
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Missing QuestionText");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = new string('c', 5001),
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("QuestionText too long");

        // Score validation
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = -1,
            ShortAnswer = "Đáp án"
        }).SetName("Negative score");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1001,
            ShortAnswer = "Đáp án"
        }).SetName("Score too high");

        // ShortText validation
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = null
        }).SetName("ShortText missing answer");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = new string('x', 1001)
        }).SetName("ShortText answer too long");

        // MultipleChoice validations
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi trắc nghiệm",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>
            {
                new()
                {
                    Text = "",
                    IsAnswer = false
                },
                new()
                {
                    Text = "",
                    IsAnswer = false
                }
            }
        }).SetName("MultipleChoice no correct answer and empty choices");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>
            {
                new()
                {
                    Text = "A",
                    IsAnswer = true
                }
            }
        }).SetName("MultipleChoice only one choice");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = Enumerable.Range(1, 11).Select(i => new CreateMultipleChoiceDto
            {
                Text = $"Lựa chọn {i}",
                IsAnswer = i == 1
            }).ToList()
        }).SetName("MultipleChoice too many choices");

        // Matching validation
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>
            {
                new()
                {
                    LeftItem = "",
                    RightItem = "Bên phải"
                },
                new()
                {
                    LeftItem = "Bên trái",
                    RightItem = ""
                }
            }
        }).SetName("Matching empty items");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>
            {
                new()
                {
                    LeftItem = "1",
                    RightItem = "2"
                }
            }
        }).SetName("Matching less than 2 pairs");

        // Ordering validation
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>
            {
                new()
                {
                    Text = "Thứ nhất",
                    CorrectOrder = 0
                },
                new()
                {
                    Text = "Thứ hai",
                    CorrectOrder = 0
                }
            }
        }).SetName("Ordering duplicate correct order");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>
            {
                new()
                {
                    Text = "Thứ nhất",
                    CorrectOrder = 0
                }
            }
        }).SetName("Ordering less than 2 items");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>
            {
                new()
                {
                    Text = "A",
                    CorrectOrder = 0
                },
                new()
                {
                    Text = "B",
                    CorrectOrder = 5
                }
            }
        }).SetName("Ordering item out of range");
    }
    
    // --- VALID CASES ---
    [Test]
    public async Task ShouldCreateQuestionSetWithOneQuestion()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateQuestionSetCommand
        {
            Name = "Bộ câu hỏi 1",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "MultipleChoice",
                    QuestionText = "Câu hỏi 1",
                    Score = 1,
                    MultipleChoices = new List<CreateMultipleChoiceDto>
                    {
                        new CreateMultipleChoiceDto
                        {
                            Text = "A",
                            IsAnswer = true
                        },
                        new CreateMultipleChoiceDto
                        {
                            Text = "B",
                            IsAnswer = false
                        }
                    }
                }
            }
        };
        var id = await SendAsync(command);
        var set = await FindAsync<QuestionSet>(id);
        set.Should().NotBeNull();
        set!.Questions.Should().HaveCount(1);
    }

    [Test]
    public async Task ShouldCreateQuestionSetWithMaxQuestions()
    {
        await RunAsDefaultUserAsync();
        var questions = Enumerable.Range(1, 500).Select(i => new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = $"Câu hỏi {i}",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).ToList();
        var command = new CreateQuestionSetCommand
        {
            Name = "Bộ câu hỏi max",
            Description = "Mô tả",
            Questions = questions
        };
        var id = await SendAsync(command);
        var set = await FindAsync<QuestionSet>(id);
        set.Should().NotBeNull();
        set!.Questions.Should().HaveCount(500);
    }

    // --- VALIDATION: General ---
    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowNameExceedMaxLength()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = new string('a', 201),
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireDescription()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowDescriptionExceedMaxLength()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = new string('b', 501),
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireAtLeastOneQuestion()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowMoreThan500Questions()
    {
        await RunAsDefaultUserAsync();
        var questions = Enumerable.Range(1, 501).Select(i => ValidShortText()).ToList();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = questions
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    // --- VALIDATION: QuestionDto chung ---
    [Test]
    public async Task ShouldRequireQuestionText()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "ShortText",
                    Score = 1,
                    ShortAnswer = "A"
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowQuestionTextExceedMaxLength()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "ShortText",
                    QuestionText = new string('c', 5001),
                    Score = 1,
                    ShortAnswer = "A"
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowScoreOutOfRange()
    {
        await RunAsDefaultUserAsync();
        var command1 = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "ShortText",
                    QuestionText = "Q",
                    Score = -1,
                    ShortAnswer = "A"
                }
            }
        };
        var command2 = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "ShortText",
                    QuestionText = "Q",
                    Score = 1001,
                    ShortAnswer = "A"
                }
            }
        };
        var ex1 = await FluentActions.Invoking(() => SendAsync(command1)).Should().ThrowAsync<ErrorCodeException>();
        ex1.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
        var ex2 = await FluentActions.Invoking(() => SendAsync(command2)).Should().ThrowAsync<ErrorCodeException>();
        ex2.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    // --- MULTIPLE CHOICE ---
    [Test]
    public async Task ShouldNotAllowMultipleChoiceWithLessThan2Options()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "MultipleChoice",
                    QuestionText = "Q",
                    Score = 1,
                    MultipleChoices = new List<CreateMultipleChoiceDto>
                    {
                        new CreateMultipleChoiceDto
                        {
                            Text = "A",
                            IsAnswer = true
                        }
                    }
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowMultipleChoiceWithNoCorrectAnswer()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "MultipleChoice",
                    QuestionText = "Q",
                    Score = 1,
                    MultipleChoices = new List<CreateMultipleChoiceDto>
                    {
                        new CreateMultipleChoiceDto
                        {
                            Text = "A",
                            IsAnswer = false
                        },
                        new CreateMultipleChoiceDto
                        {
                            Text = "B",
                            IsAnswer = false
                        }
                    }
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowMultipleChoiceWithMoreThan10Options()
    {
        await RunAsDefaultUserAsync();
        var options = Enumerable.Range(1, 11).Select(i => new CreateMultipleChoiceDto
        {
            Text = $"A{i}",
            IsAnswer = i == 1
        }).ToList();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "MultipleChoice",
                    QuestionText = "Q",
                    Score = 1,
                    MultipleChoices = options
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    // --- MATCHING ---
    [Test]
    public async Task ShouldNotAllowMatchingWithLessThan2Pairs()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "Matching",
                    QuestionText = "Q",
                    Score = 1,
                    MatchingPairs = new List<CreateMatchingPairDto>
                    {
                        new CreateMatchingPairDto
                        {
                            LeftItem = "A",
                            RightItem = "B"
                        }
                    }
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowMatchingWithEmptyLeftOrRight()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "Matching",
                    QuestionText = "Q",
                    Score = 1,
                    MatchingPairs = new List<CreateMatchingPairDto>
                    {
                        new CreateMatchingPairDto
                        {
                            LeftItem = "",
                            RightItem = "B"
                        },
                        new CreateMatchingPairDto
                        {
                            LeftItem = "A",
                            RightItem = null
                        }
                    }
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    // --- ORDERING ---
    [Test]
    public async Task ShouldNotAllowOrderingWithLessThan2Items()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "Ordering",
                    QuestionText = "Q",
                    Score = 1,
                    OrderingItems = new List<CreateOrderingItemDto>
                    {
                        new CreateOrderingItemDto
                        {
                            Text = "A",
                            CorrectOrder = 0
                        }
                    }
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldNotAllowOrderingWithDuplicateOrder()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "Ordering",
                    QuestionText = "Q",
                    Score = 1,
                    OrderingItems = new List<CreateOrderingItemDto>
                    {
                        new CreateOrderingItemDto
                        {
                            Text = "A",
                            CorrectOrder = 0
                        },
                        new CreateOrderingItemDto
                        {
                            Text = "B",
                            CorrectOrder = 0
                        }
                    }
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    // --- SHORT TEXT ---
    [Test]
    public async Task ShouldNotAllowShortTextWithEmptyAnswer()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "ShortText",
                    QuestionText = "Q",
                    Score = 1,
                    ShortAnswer = ""
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        // Không gọi RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                new CreateUpdateQuestionDto
                {
                    Type = "ShortText",
                    QuestionText = "Q",
                    Score = 1,
                    ShortAnswer = "A"
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    // --- Helper ---
    private CreateUpdateQuestionDto ValidShortText() => new CreateUpdateQuestionDto
    {
        Type = "ShortText",
        QuestionText = "Câu hỏi hợp lệ",
        Score = 1,
        ShortAnswer = "Đáp án"
    };
}
