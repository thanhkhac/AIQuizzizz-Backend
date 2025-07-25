using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;

using static Testing;

public class CreateQuestionSetTests : BaseTestFixture
{
    // --- VALID CASES ---
    [Test]
    public async Task ShouldCreateQuestionSetWithOneQuestion()
    {
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
        var command = new CreateQuestionSetCommand
        {
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowNameExceedMaxLength()
    {
        var command = new CreateQuestionSetCommand
        {
            Name = new string('a', 201),
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldRequireDescription()
    {
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowDescriptionExceedMaxLength()
    {
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = new string('b', 501),
            Questions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldRequireAtLeastOneQuestion()
    {
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>()
        };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowMoreThan500Questions()
    {
        var questions = Enumerable.Range(1, 501).Select(i => ValidShortText()).ToList();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = questions
        };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    // --- VALIDATION: QuestionDto chung ---
    [Test]
    public async Task ShouldRequireQuestionText()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowQuestionTextExceedMaxLength()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowScoreOutOfRange()
    {
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
        await FluentActions.Invoking(() => SendAsync(command1)).Should().ThrowAsync<FluentValidation.ValidationException>();
        await FluentActions.Invoking(() => SendAsync(command2)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    // --- MULTIPLE CHOICE ---
    [Test]
    public async Task ShouldNotAllowMultipleChoiceWithLessThan2Options()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowMultipleChoiceWithNoCorrectAnswer()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowMultipleChoiceWithMoreThan10Options()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    // --- MATCHING ---
    [Test]
    public async Task ShouldNotAllowMatchingWithLessThan2Pairs()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowMatchingWithEmptyLeftOrRight()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    // --- ORDERING ---
    [Test]
    public async Task ShouldNotAllowOrderingWithLessThan2Items()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task ShouldNotAllowOrderingWithDuplicateOrder()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    // --- SHORT TEXT ---
    [Test]
    public async Task ShouldNotAllowShortTextWithEmptyAnswer()
    {
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
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<FluentValidation.ValidationException>();
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
