using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Commands;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        Console.WriteLine($"\n=== TEST CASE: {TestContext.CurrentContext.Test.Name}");
        PrintJson(command.Questions);

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    public static IEnumerable<TestCaseData> InvalidQuestions()
    {
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = null,
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Type null");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Essay",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Type invalid");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionId = Guid.Empty,
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("QuestionId empty");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = null
        }).SetName("MultipleChoice null choices");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>()
        }).SetName("MultipleChoice empty choices");

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
                    IsAnswer = false
                },
                new()
                {
                    Text = "B",
                    IsAnswer = false
                }
            }
        }).SetName("MultipleChoice no correct answer");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>
            {
                new()
                {
                    Text = null,
                    IsAnswer = true
                },
                new()
                {
                    Text = "B",
                    IsAnswer = false
                }
            }
        }).SetName("MultipleChoice null text");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>
            {
                new()
                {
                    Text = "",
                    IsAnswer = true
                },
                new()
                {
                    Text = "B",
                    IsAnswer = false
                }
            }
        }).SetName("MultipleChoice empty text");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>
            {
                new()
                {
                    Text = new string('a', 1001),
                    IsAnswer = true
                },
                new()
                {
                    Text = "B",
                    IsAnswer = false
                }
            }
        }).SetName("MultipleChoice text too long");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = null
        }).SetName("Matching null pairs");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>()
        }).SetName("Matching empty pairs");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>
            {
                new()
                {
                    LeftItem = "A",
                    RightItem = "B"
                }
            }
        }).SetName("Matching only one pair");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>
            {
                new()
                {
                    LeftItem = null,
                    RightItem = "B"
                },
                new()
                {
                    LeftItem = "A",
                    RightItem = "B"
                }
            }
        }).SetName("Matching null left");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>
            {
                new()
                {
                    LeftItem = "A",
                    RightItem = null
                },
                new()
                {
                    LeftItem = "B",
                    RightItem = "C"
                }
            }
        }).SetName("Matching null right");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>
            {
                new()
                {
                    LeftItem = new string('a', 1001),
                    RightItem = "B"
                },
                new()
                {
                    LeftItem = "A",
                    RightItem = "B"
                }
            }
        }).SetName("Matching left too long");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>
            {
                new()
                {
                    LeftItem = "A",
                    RightItem = new string('b', 1001)
                },
                new()
                {
                    LeftItem = "B",
                    RightItem = "C"
                }
            }
        }).SetName("Matching right too long");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = null
        }).SetName("Ordering null items");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>()
        }).SetName("Ordering empty items");

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
                }
            }
        }).SetName("Ordering only one item");

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
                    CorrectOrder = 0
                }
            }
        }).SetName("Ordering duplicate order");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>
            {
                new()
                {
                    Text = null,
                    CorrectOrder = 0
                },
                new()
                {
                    Text = "B",
                    CorrectOrder = 1
                }
            }
        }).SetName("Ordering null text");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>
            {
                new()
                {
                    Text = "",
                    CorrectOrder = 0
                },
                new()
                {
                    Text = "B",
                    CorrectOrder = 1
                }
            }
        }).SetName("Ordering empty text");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>
            {
                new()
                {
                    Text = new string('a', 1001),
                    CorrectOrder = 0
                },
                new()
                {
                    Text = "B",
                    CorrectOrder = 1
                }
            }
        }).SetName("Ordering text too long");

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
                    CorrectOrder = -1
                },
                new()
                {
                    Text = "B",
                    CorrectOrder = 1
                }
            }
        }).SetName("Ordering negative order");

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
        }).SetName("Ordering order out of range");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = ""
        }).SetName("ShortText empty answer");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = ""
        }).SetName("ShortText null answer");

        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = new string('x', 1001)
        }).SetName("ShortText answer too long");

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
    }

    [Test]
    public async Task ShouldCreateValidAllQuestionTypes()
    {
        await RunAsDefaultUserAsync();
        var questions = new List<CreateUpdateQuestionDto>
        {
            new CreateUpdateQuestionDto
            {
                Type = "MultipleChoice",
                QuestionText = "Câu hỏi trắc nghiệm",
                Score = 2,
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
            },
            new CreateUpdateQuestionDto
            {
                Type = "Matching",
                QuestionText = "Ghép cặp",
                Score = 3,
                MatchingPairs = new List<CreateMatchingPairDto>
                {
                    new CreateMatchingPairDto
                    {
                        LeftItem = "Trái 1",
                        RightItem = "Phải 1"
                    },
                    new CreateMatchingPairDto
                    {
                        LeftItem = "Trái 2",
                        RightItem = "Phải 2"
                    }
                }
            },
            new CreateUpdateQuestionDto
            {
                Type = "Ordering",
                QuestionText = "Sắp xếp thứ tự",
                Score = 4,
                OrderingItems = new List<CreateOrderingItemDto>
                {
                    new CreateOrderingItemDto
                    {
                        Text = "Bước 1",
                        CorrectOrder = 0
                    },
                    new CreateOrderingItemDto
                    {
                        Text = "Bước 2",
                        CorrectOrder = 1
                    }
                }
            },
            new CreateUpdateQuestionDto
            {
                Type = "ShortText",
                QuestionText = "Điền đáp án ngắn",
                Score = 1,
                ShortAnswer = "Đáp án đúng"
            }
        };
        var command = new CreateQuestionSetCommand
        {
            Name = "Bộ câu hỏi tổng hợp",
            Description = "desc",
            Questions = questions
        };

        PrintJson(command.Questions);

        var id = await SendAsync(command);
        var set = (await QueryListAsync<QuestionSet>(x => x.Include(y => y.Questions).Where(y => y.Id == id))).First();
        set.Should().NotBeNull();
        set!.Questions.Should().HaveCount(4);
        set.Questions.Any(q => q.QuestionText == "Câu hỏi trắc nghiệm").Should().BeTrue();
        set.Questions.Any(q => q.QuestionText == "Ghép cặp").Should().BeTrue();
        set.Questions.Any(q => q.QuestionText == "Sắp xếp thứ tự").Should().BeTrue();
        set.Questions.Any(q => q.QuestionText == "Điền đáp án ngắn").Should().BeTrue();
    }
    
    [Test]
    public async Task ShouldCreate400MatchingQuestions()
    {
        await RunAsDefaultUserAsync();

        var questions = new List<CreateUpdateQuestionDto>();

        for (int i = 1; i <= 500; i++)
        {
            questions.Add(new CreateUpdateQuestionDto
            {
                Type = "Matching",
                QuestionText = $"Ghép cặp số {i}",
                Score = 1,
                MatchingPairs = new List<CreateMatchingPairDto>
                {
                    new CreateMatchingPairDto
                    {
                        LeftItem = $"Trái {i}-1",
                        RightItem = $"Phải {i}-1"
                    },
                    new CreateMatchingPairDto
                    {
                        LeftItem = $"Trái {i}-2",
                        RightItem = $"Phải {i}-2"
                    }
                }
            });
        }

        var command = new CreateQuestionSetCommand
        {
            Name = "Bộ 400 câu hỏi Matching",
            Description = "Test hiệu năng với 400 câu hỏi Matching",
            Questions = questions
        };

        var id = await SendAsync(command);
        var set = (await QueryListAsync<QuestionSet>(
            x => x.Include(y => y.Questions).Where(y => y.Id == id)
        )).First();

        set.Should().NotBeNull();
        set.Questions.Should().HaveCount(500);
    }

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
    public async Task ShouldRequireAtLeastOneQuestion()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateQuestionSetCommand
        {
            Name = "Tên",
            Description = "Mô tả",
            Questions = new List<CreateUpdateQuestionDto>()
        };
        PrintJson(command);
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }


    [Test]
    [TestCaseSource(nameof(InvalidCreateQuestionSetCommandData))]
    public async Task ShouldRejectInvalidCreateQuestionSetCommand(CreateQuestionSetCommand invalidCommand, string expectedErrorCode)
    {
        await RunAsDefaultUserAsync();

        Console.WriteLine("=== Invalid Command Input ===");
        Console.WriteLine($"\n=== TEST CASE: {TestContext.CurrentContext.Test.Name}");
        PrintJson(invalidCommand);

        var ex = await FluentActions.Invoking(() => SendAsync(invalidCommand))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(expectedErrorCode);
    }

    public static IEnumerable<TestCaseData> InvalidCreateQuestionSetCommandData()
    {
        yield return new TestCaseData(
            new CreateQuestionSetCommand
            {
                Name = "",
                Description = "Mô tả",
                Questions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Name empty");

        yield return new TestCaseData(
            new CreateQuestionSetCommand
            {
                Name = new string('a', 201),
                Description = "Mô tả",
                Questions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Name too long");

        yield return new TestCaseData(
            new CreateQuestionSetCommand
            {
                Name = "Tên",
                Description = new string('b', 501),
                Questions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Description too long");

        yield return new TestCaseData(
            new CreateQuestionSetCommand
            {
                Name = "Tên",
                Description = "Mô tả",
                Questions = new List<CreateUpdateQuestionDto>()
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Questions empty");

        yield return new TestCaseData(
            new CreateQuestionSetCommand
            {
                Name = "Tên",
                Description = "Mô tả",
                Questions = Enumerable.Range(1, 501).Select(i => ValidShortText()).ToList()
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Questions too many");

        yield return new TestCaseData(
            new CreateQuestionSetCommand
            {
                Name = "Tên",
                Description = "Mô tả",
                Questions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                },
                Tags = Enumerable.Range(1, 11).Select(i => $"tag{i}").ToList()
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Tags too many");

        yield return new TestCaseData(
            new CreateQuestionSetCommand
            {
                Name = "Tên",
                Description = "Mô tả",
                Questions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                },
                Tags = new List<string>
                {
                    "validtag",
                    new string('t', 51)
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Tag too long");
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
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
                    ShortAnswer = "A"
                }
            }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    private static CreateUpdateQuestionDto ValidShortText() => new CreateUpdateQuestionDto
    {
        Type = "ShortText",
        QuestionText = "Câu hỏi hợp lệ",
        Score = 1,
        ShortAnswer = "Đáp án"
    };
}
