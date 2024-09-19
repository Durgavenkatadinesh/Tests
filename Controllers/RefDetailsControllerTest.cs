using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RumInplicitAPI.Controllers;
using RumInplicitAPI.Core.DTOs;
using RumInplicitAPI.Core.Interfaces;
using Xunit;

namespace RumInplicitAPI.Tests.Controllers
{
    public class RefDetailsControllerTest
    {
        private readonly RefDetailsController _controller;
        private readonly Mock<IDbAccess> _dbAccessMock;

        public RefDetailsControllerTest()
        {
            _dbAccessMock = new Mock<IDbAccess>();
            _controller = new RefDetailsController(_dbAccessMock.Object);
        }

        [Fact]
        public async Task GetMappedRefDetails_ReturnsOkResult_WhenDataIsRetrieved()
        {
            // Arrange
            var expectedResult = new Dictionary<int, string?>
            {
                { 1, "Entity1" },
                { 2, "Entity2" },
                { 0, "" }
            };

            _dbAccessMock
                .Setup(x => x.GetAllRefDetails())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetMappedRefDetails();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]

        public async Task GetMappedRefDetails_ReturnsServerException()
        {
            //Arrange
            var mappedDetails = new List<string>();

            _dbAccessMock.Setup(db => db.GetAllRefDetails()).ThrowsAsync(new Exception("Database Error"));

            //Act
            var result = await _controller.GetMappedRefDetails();

            //Assert
            result.Should().BeOfType<ObjectResult>();
            var serverException = result as ObjectResult;
            serverException.StatusCode.Should().Be(500);
            serverException.Value.Should().BeEquivalentTo("Database Error");
        }

        [Fact]
        public async Task GetMappedRootCauses_ReturnsOkResult_WhenDataIsRetrieved()
        {
            // Arrange
            var expectedResult = new Dictionary<int, List<int>>
            {
                { 1, new List<int> { 101, 102 } },
                { 2, new List<int> { 201 } }
            };

            _dbAccessMock
                .Setup(x => x.MapRootCauseIds())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetMappedRootCauses();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task GetMappedRootCauses_Returns500_WhenExceptionIsThrown()
        {
            // Arrange
            _dbAccessMock
                .Setup(x => x.MapRootCauseIds())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetMappedRootCauses();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Database error");
        }

    }
}
