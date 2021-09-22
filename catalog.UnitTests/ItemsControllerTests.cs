using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using catalog.api;
using catalog.api.Controllers;
using catalog.api.DTOS;
using catalog.api.Entities;
using catalog.api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace catalog.UnitTests
{
    public class ItemsControllerTests
    {
        private readonly Mock<IItemsRepository> repositoryStub = new();
        private readonly Mock<ILogger<ItemsController>> loggerStub = new();

        private readonly Random rand = new();

        [Fact]
        public async Task GetItemAsync_WithUnexistingItem_ReturnNotFound() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Item)null);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            // Assert
            // Assert.IsType<NotFoundResult>(result.Result);
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetItemAsync_WithExistingItem_ReturnExpectedItem() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var expectedItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(expectedItem);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            // Assert
            var tmp = result.Result as OkObjectResult;
            var resultobject = tmp.Value as ItemDTO;

            // //For Record Type
            // resultobject.Should().BeEquivalentTo(
            //     expectedItem,
            //     options => options.ComparingByMembers<Item>());

            // For Class Type
            resultobject.Should().BeEquivalentTo(expectedItem);
            // Assert.IsType<ItemDTO>(result.Value);
            // var dto = (result as ActionResult<ItemDTO>).Value;
            // Assert.Equal(expectedItem.Id, dto.Id);
            // Assert.Equal(expectedItem.Name, dto.Name);
        }

        [Fact]
        public async Task GetItemsAsync_WithExistingItems_ReturnAllItems() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };

            repositoryStub.Setup(repo => repo.GetItemsAsync())
                .ReturnsAsync(expectedItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var actualItems = await controller.GetItemsAsync();

            // Assert
            // For Record Type
            // actualItems.Should().BeEquivalentTo(
            //     expectedItems,
            //     options => options.ComparingByMembers<Item>()
            // );

            //For Class Type
            actualItems.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public async Task GetItemsAsync_WithMatchingItems_ReturnMatchingItems() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var allItems = new[] 
            { 
                new Item() { Name = "Potion" },
                new Item() { Name = "Antidote" },
                new Item() { Name = "Hi-Potion" },
            };

            var nameToMatch = "potion";

            repositoryStub.Setup(repo => repo.GetItemsAsync())
                .ReturnsAsync(allItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            IEnumerable<ItemDTO> foundItems = await controller.GetItemsAsync(nameToMatch);

            // Assert
            foundItems.Should().OnlyContain(
                item => item.Name == allItems[0].Name || item.Name == allItems[2].Name
            );
        }

        [Fact]
        public async Task CreateItemAsync_WithItemToCreate_ReturnCreatedItem() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var itemToCreate = new CreateItemDTO(
                Guid.NewGuid().ToString(), 
                Guid.NewGuid().ToString(),  
                rand.Next(1000));
            // {
            //     Name = Guid.NewGuid().ToString(),
            //     Price = rand.Next(1000)
            // };

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.CreateItemAsync(itemToCreate);

            // Assert
            var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDTO;
            createdItem.Should().BeEquivalentTo(
                itemToCreate,
                options => options.ComparingByMembers<ItemDTO>().ExcludingMissingMembers()
            );
            createdItem.Id.Should().NotBeEmpty();
            createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(1000));
        }

        [Fact]
        public async Task UpdateItemAsync_WithExistingItem_ReturnNoContent() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            var itemId = existingItem.Id;
            var itemToUpdate = new UpdateItemDTO(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                existingItem.Price + 3
            );
            // { 
            //     Name = Guid.NewGuid().ToString(), 
            //     Price = existingItem.Price + 3 
            // };

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.UpdateItemAsync(itemId, itemToUpdate);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateItemAsync_WithUnExistingItem_ReturnNotFound() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var unexistingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Item)null);

            var itemId = unexistingItem.Id;
            var itemToUpdate = new UpdateItemDTO(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                unexistingItem.Price + 3
            );
            // { 
            //     Name = Guid.NewGuid().ToString(), 
            //     Price = existingItem.Price + 3 
            // };

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.UpdateItemAsync(itemId, itemToUpdate);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteItemAsync_WithExistingItem_ReturnNoContent() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            var itemId = existingItem.Id;

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.DeleteItemAsync(itemId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteItemAsync_WithUnExistingItem_ReturnNotFound() // UnitOfWork_StateUnderTest_ExpectedBehaviour
        {
            // Arrange
            var unexistingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Item)null);

            var itemId = unexistingItem.Id;

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.DeleteItemAsync(itemId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        private Item CreateRandomItem()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreatedDate = DateTimeOffset.UtcNow
            };
        }
    }
}
