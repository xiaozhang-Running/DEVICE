using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using Moq;

namespace DeviceWarehouse.Tests;

public class ProjectOutboundServiceTests
{
    private readonly Mock<IProjectOutboundService> _mockProjectOutboundService;

    public ProjectOutboundServiceTests()
    {
        _mockProjectOutboundService = new Mock<IProjectOutboundService>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProjectOutboundDto()
    {
        // Arrange
        var outboundId = 1;
        var expectedOutbound = new ProjectOutboundDto
        {
            Id = outboundId,
            OutboundNumber = "OUT-2024-0001",
            OutboundDate = DateTime.Now,
            ProjectName = "Test Project",
            Items = new List<ProjectOutboundItemDto>()
        };

        _mockProjectOutboundService
            .Setup(service => service.GetByIdAsync(outboundId))
            .ReturnsAsync(expectedOutbound);

        // Act
        var result = await _mockProjectOutboundService.Object.GetByIdAsync(outboundId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(outboundId, result.Id);
        Assert.Equal("OUT-2024-0001", result.OutboundNumber);
        Assert.Equal("Test Project", result.ProjectName);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnListOfProjectOutboundDto()
    {
        // Arrange
        var expectedOutbounds = new List<ProjectOutboundDto>
        {
            new ProjectOutboundDto
            {
                Id = 1,
                OutboundNumber = "OUT-2024-0001",
                OutboundDate = DateTime.Now,
                ProjectName = "Test Project 1"
            },
            new ProjectOutboundDto
            {
                Id = 2,
                OutboundNumber = "OUT-2024-0002",
                OutboundDate = DateTime.Now,
                ProjectName = "Test Project 2"
            }
        };

        _mockProjectOutboundService
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(expectedOutbounds);

        // Act
        var result = await _mockProjectOutboundService.Object.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedProjectOutboundDto()
    {
        // Arrange
        var createDto = new CreateProjectOutboundDto
        {
            OutboundDate = DateTime.Now,
            ProjectName = "New Project",
            Items = new List<CreateProjectOutboundItemDto>
            {
                new CreateProjectOutboundItemDto
                {
                    ItemType = ItemType.GeneralEquipment,
                    ItemId = 1,
                    ItemName = "Test Equipment",
                    Quantity = 1
                }
            }
        };

        var expectedOutbound = new ProjectOutboundDto
        {
            Id = 1,
            OutboundNumber = "OUT-2024-0001",
            OutboundDate = createDto.OutboundDate,
            ProjectName = createDto.ProjectName,
            Items = new List<ProjectOutboundItemDto>()
        };

        _mockProjectOutboundService
            .Setup(service => service.CreateAsync(createDto))
            .ReturnsAsync(expectedOutbound);

        // Act
        var result = await _mockProjectOutboundService.Object.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("OUT-2024-0001", result.OutboundNumber);
        Assert.Equal("New Project", result.ProjectName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var outboundId = 1;
        var updateDto = new UpdateProjectOutboundDto
        {
            OutboundNumber = "OUT-2024-0001",
            OutboundDate = DateTime.Now,
            ProjectName = "Updated Project",
            Items = new List<CreateProjectOutboundItemDto>()
        };

        var expectedOutbound = new ProjectOutboundDto
        {
            Id = outboundId,
            OutboundNumber = updateDto.OutboundNumber,
            OutboundDate = updateDto.OutboundDate,
            ProjectName = updateDto.ProjectName,
            Items = new List<ProjectOutboundItemDto>()
        };

        _mockProjectOutboundService
            .Setup(service => service.UpdateAsync(outboundId, updateDto))
            .ReturnsAsync(expectedOutbound);

        // Act
        var result = await _mockProjectOutboundService.Object.UpdateAsync(outboundId, updateDto);

        // Assert
        Assert.NotNull(result);
        _mockProjectOutboundService.Verify(service => service.UpdateAsync(outboundId, updateDto), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var outboundId = 1;

        _mockProjectOutboundService
            .Setup(service => service.CompleteAsync(outboundId))
            .Returns(Task.CompletedTask);

        // Act
        await _mockProjectOutboundService.Object.CompleteAsync(outboundId);

        // Assert
        _mockProjectOutboundService.Verify(service => service.CompleteAsync(outboundId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var outboundId = 1;

        _mockProjectOutboundService
            .Setup(service => service.DeleteAsync(outboundId))
            .Returns(Task.CompletedTask);

        // Act
        await _mockProjectOutboundService.Object.DeleteAsync(outboundId);

        // Assert
        _mockProjectOutboundService.Verify(service => service.DeleteAsync(outboundId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrueWhenOutboundExists()
    {
        // Arrange
        var outboundNumber = "OUT-2024-0001";

        _mockProjectOutboundService
            .Setup(service => service.ExistsAsync(outboundNumber))
            .ReturnsAsync(true);

        // Act
        var result = await _mockProjectOutboundService.Object.ExistsAsync(outboundNumber);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalseWhenOutboundDoesNotExist()
    {
        // Arrange
        var outboundNumber = "OUT-2024-0001";

        _mockProjectOutboundService
            .Setup(service => service.ExistsAsync(outboundNumber))
            .ReturnsAsync(false);

        // Act
        var result = await _mockProjectOutboundService.Object.ExistsAsync(outboundNumber);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAvailableItemsAsync_ShouldReturnListOfAvailableItemDto()
    {
        // Arrange
        var keyword = "test";
        var expectedItems = new List<AvailableItemDto>
        {
            new AvailableItemDto
            {
                id = 1,
                itemType = ItemType.GeneralEquipment,
                itemTypeName = "通用设备",
                name = "Test Equipment",
                availableQuantity = 5
            }
        };

        _mockProjectOutboundService
            .Setup(service => service.GetAvailableItemsAsync(keyword))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _mockProjectOutboundService.Object.GetAvailableItemsAsync(keyword);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count());
    }

    [Fact]
    public async Task GetAvailableItemsPagedAsync_ShouldReturnAvailableItemsResponseDto()
    {
        // Arrange
        var request = new AvailableItemsRequestDto
        {
            Keyword = "test",
            PageNumber = 1,
            PageSize = 10
        };

        var expectedResponse = new AvailableItemsResponseDto
        {
            items = new List<AvailableItemDto>
            {
                new AvailableItemDto
                {
                    id = 1,
                    itemType = ItemType.GeneralEquipment,
                    itemTypeName = "通用设备",
                    name = "Test Equipment",
                    availableQuantity = 5
                }
            },
            totalCount = 1,
            pageNumber = 1,
            pageSize = 10
        };

        _mockProjectOutboundService
            .Setup(service => service.GetAvailableItemsPagedAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockProjectOutboundService.Object.GetAvailableItemsPagedAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.totalCount);
        Assert.Equal(1, result.items.Count);
    }

    [Fact]
    public async Task SearchOutboundsAsync_ShouldReturnListOfProjectOutboundDto()
    {
        // Arrange
        var keyword = "test";
        var expectedOutbounds = new List<ProjectOutboundDto>
        {
            new ProjectOutboundDto
            {
                Id = 1,
                OutboundNumber = "OUT-2024-0001",
                OutboundDate = DateTime.Now,
                ProjectName = "Test Project"
            }
        };

        _mockProjectOutboundService
            .Setup(service => service.SearchOutboundsAsync(keyword))
            .ReturnsAsync(expectedOutbounds);

        // Act
        var result = await _mockProjectOutboundService.Object.SearchOutboundsAsync(keyword);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count());
    }
}
