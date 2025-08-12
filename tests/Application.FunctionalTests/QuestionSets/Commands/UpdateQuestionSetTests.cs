using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Commands;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;

using static Testing;

public class UpdateQuestionSetTests : BaseTestFixture
{
    private static CreateUpdateQuestionDto ValidShortText() => new CreateUpdateQuestionDto
    {
        Type = "ShortText",
        QuestionText = "Câu hỏi hợp lệ",
        Score = 1,
        ShortAnswer = "Đáp án"
    };

    private static CreateUpdateQuestionDto ValidMultipleChoice() => new CreateUpdateQuestionDto
    {
        Type = "MultipleChoice",
        QuestionText = "Câu hỏi trắc nghiệm hợp lệ",
        Score = 1,
        MultipleChoices = new List<CreateMultipleChoiceDto>
        {
            new()
            {
                Text = "A",
                IsAnswer = true
            },
            new()
            {
                Text = "B",
                IsAnswer = false
            }
        }
    };

    private static CreateUpdateQuestionDto ValidMatching() => new CreateUpdateQuestionDto
    {
        Type = "Matching",
        QuestionText = "Câu hỏi ghép đôi hợp lệ",
        Score = 1,
        MatchingPairs = new List<CreateMatchingPairDto>
        {
            new()
            {
                LeftItem = "Trái 1",
                RightItem = "Phải 1"
            },
            new()
            {
                LeftItem = "Trái 2",
                RightItem = "Phải 2"
            }
        }
    };

    private static CreateUpdateQuestionDto ValidOrdering() => new CreateUpdateQuestionDto
    {
        Type = "Ordering",
        QuestionText = "Câu hỏi sắp xếp hợp lệ",
        Score = 1,
        OrderingItems = new List<CreateOrderingItemDto>
        {
            new()
            {
                Text = "Bước 1",
                CorrectOrder = 0
            },
            new()
            {
                Text = "Bước 2",
                CorrectOrder = 1
            }
        }
    };

    //normal
    [Test]
    public async Task ShouldUpdateQuestionSetSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = userId,
            Questions = new List<Question>
            {
                ValidQuestionEntity()
            }
        };
        await AddAsync(originalQuestionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = originalQuestionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = originalQuestionSet.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };

        var resultId = await SendAsync(command);

        resultId.Should().Be(originalQuestionSet.Id);

        var updatedQuestionSet = await FindAsync<QuestionSet>(originalQuestionSet.Id);
        updatedQuestionSet.Should().NotBeNull();
        updatedQuestionSet!.Name.Should().Be("Updated Name");
        updatedQuestionSet.Description.Should().Be("Updated Description");
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = Guid.NewGuid(),
            Name = "Test",
            Description = "Test",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotEditQuestionSet()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(originalQuestionSet);

        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = originalQuestionSet.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //normal
    [Test]
    public async Task ShouldAllowUpdate_WhenUserHasEditableShareMode()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(originalQuestionSet);

        var editorId = await RunAsUserAsync("editor@local", "Editor1234!", []);
        await AddAsync(new QuestionSetUser
        {
            UserId = editorId,
            QuestionSetId = originalQuestionSet.Id,
            ShareMode = QuestionSetUserShareMode.Editable
        });

        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = originalQuestionSet.Id,
            Name = "Updated Name by Editor",
            Description = "Updated Description by Editor",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };

        var resultId = await SendAsync(command);

        resultId.Should().Be(originalQuestionSet.Id);
        var updatedQuestionSet = await FindAsync<QuestionSet>(originalQuestionSet.Id);
        updatedQuestionSet.Should().NotBeNull();
        updatedQuestionSet!.Name.Should().Be("Updated Name by Editor");
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserHasViewOnlyShareMode()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(originalQuestionSet);

        var viewerId = await RunAsUserAsync("viewer@local", "Viewer1234!", []);
        await AddAsync(new QuestionSetUser
        {
            UserId = viewerId,
            QuestionSetId = originalQuestionSet.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = originalQuestionSet.Id,
            Name = "Updated Name by Viewer",
            Description = "Updated Description by Viewer",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(originalQuestionSet);

        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = originalQuestionSet.Id,
            Name = "Test",
            Description = "Test",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionCountExceedsLimit()
    {
        var userId = await RunAsDefaultUserAsync();
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = userId,
            QuestionCount = 500,
            Questions = Enumerable.Range(0, 501).Select(_ => ValidQuestionEntity()).ToList()
        };
        await AddAsync(originalQuestionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = originalQuestionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = originalQuestionSet.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                ValidShortText()
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.NUMBER_OF_QUESTION_EXCEED_LIMIT);
    }

    //abnormal
    [Test]
    [TestCaseSource(nameof(InvalidUpdateQuestionSetCommandData))]
    public async Task ShouldRejectInvalidUpdateQuestionSetCommand(UpdateQuestionSetCommand invalidCommand, string expectedErrorCode)
    {
        var userId = await RunAsDefaultUserAsync();
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = userId,
            Questions = new List<Question>
            {
                ValidQuestionEntity()
            }
        };
        await AddAsync(originalQuestionSet);

        invalidCommand.QuestionSetId = originalQuestionSet.Id;
        var ex = await FluentActions.Invoking(() => SendAsync(invalidCommand))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(expectedErrorCode);
    }

    public static IEnumerable<TestCaseData> InvalidUpdateQuestionSetCommandData()
    {
        //abnormal
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = "",
                Description = "Desc",
                CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Name empty");

        //boundary
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = new string('a', 201),
                Description = "Desc",
                CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Name too long");

        //boundary
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = "Name",
                Description = new string('b', 501),
                CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Description too long");

        //abnormal
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = "Name",
                Description = "Desc",
                CreateUpdateQuestions = new List<CreateUpdateQuestionDto>()
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("CreateUpdateQuestions empty");
        //boundary
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = "Name",
                Description = "Desc",
                CreateUpdateQuestions = Enumerable.Range(1, 501).Select(i => ValidShortText()).ToList()
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("CreateUpdateQuestions too many");

        //boundary
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = "Name",
                Description = "Desc",
                CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                },
                Tags = Enumerable.Range(1, 11).Select(i => $"tag{i}").ToList()
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Tags too many");

        //abnormal
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = "Name",
                Description = "Desc",
                CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
                {
                    ValidShortText()
                },
                Tags = new List<string>
                {
                    "validtag",
                    ""
                }
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Tag empty");

        //boundary
        yield return new TestCaseData(
            new UpdateQuestionSetCommand
            {
                Name = "Name",
                Description = "Desc",
                CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
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

    //abnormal
    [Test]
    [TestCaseSource(nameof(InvalidQuestionsForUpdate))]
    public async Task ShouldRejectInvalidQuestionInUpdate(CreateUpdateQuestionDto invalidQuestion)
    {
        var userId = await RunAsDefaultUserAsync();
        var originalQuestionSet = new QuestionSet
        {
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = userId,
            Questions = new List<Question>
            {
                ValidQuestionEntity()
            }
        };
        await AddAsync(originalQuestionSet);

        var command = new UpdateQuestionSetCommand
        {
            QuestionSetId = originalQuestionSet.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                invalidQuestion
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    public static IEnumerable<TestCaseData> InvalidQuestionsForUpdate()
    {
        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = null,
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Update: Type null");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Essay",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Update: Type invalid");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionId = Guid.Empty,
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Update: QuestionId empty");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = null
        }).SetName("Update: MultipleChoice null choices");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>()
        }).SetName("Update: MultipleChoice empty choices");

        //abnormal
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
        }).SetName("Update: MultipleChoice no correct answer");

        //abnormal
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
        }).SetName("Update: MultipleChoice null text");

        //abnormal
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
        }).SetName("Update: MultipleChoice empty text");

        //boundary
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
        }).SetName("Update: MultipleChoice text too long");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = null
        }).SetName("Update: Matching null pairs");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Matching",
            QuestionText = "Ghép đôi",
            Score = 1,
            MatchingPairs = new List<CreateMatchingPairDto>()
        }).SetName("Update: Matching empty pairs");

        //boundary
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
        }).SetName("Update: Matching only one pair");

        //abnormal
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
        }).SetName("Update: Matching null left");

        //abnormal
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
        }).SetName("Update: Matching null right");

        //boundary
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
        }).SetName("Update: Matching left too long");

        //boundary
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
        }).SetName("Update: Matching right too long");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = null
        }).SetName("Update: Ordering null items");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            QuestionText = "Sắp xếp",
            Score = 1,
            OrderingItems = new List<CreateOrderingItemDto>()
        }).SetName("Update: Ordering empty items");

        //boundary
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
        }).SetName("Update: Ordering only one item");

        //abnormal
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
        }).SetName("Update: Ordering duplicate order");

        //abnormal
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
        }).SetName("Update: Ordering null text");

        //abnormal
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
        }).SetName("Update: Ordering empty text");

        //boundary
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
        }).SetName("Update: Ordering text too long");

        //boundary
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
        }).SetName("Update: Ordering negative order");

        //boundary
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
        }).SetName("Update: Ordering order out of range");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = ""
        }).SetName("Update: ShortText empty answer");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = ""
        }).SetName("Update: ShortText null answer");

        //boundary
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1,
            ShortAnswer = new string('x', 1001)
        }).SetName("Update: ShortText answer too long");

        //abnormal
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Update: Missing QuestionText");

        //boundary
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = new string('c', 5001),
            Score = 1,
            ShortAnswer = "Đáp án"
        }).SetName("Update: QuestionText too long");
        //boundary
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = -1,
            ShortAnswer = "Đáp án"
        }).SetName("Update: Negative score");
        //boundary
        yield return new TestCaseData(new CreateUpdateQuestionDto
        {
            Type = "ShortText",
            QuestionText = "Câu hỏi",
            Score = 1001,
            ShortAnswer = "Đáp án"
        }).SetName("Update: Score too high");

        //boundary
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
        }).SetName("Update: MultipleChoice only one choice");
        //boundary
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
        }).SetName("Update: MultipleChoice too many choices");

        //abnormal
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
        }).SetName("Update: Matching empty items");

        //boundary
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
        }).SetName("Update: Matching less than 2 pairs");


        //boundary
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
        }).SetName("Update: Ordering less than 2 items");
    }

    private static Question ValidQuestionEntity() => new Question
    {
        Id = Guid.NewGuid(),
        QuestionText = "Valid existing question",
        Type = QuestionType.ShortText,
        Score = 1,
        DataJson = "{}",
        TextFormat = TextFormat.PlainText
    };
}
