using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RumInplicitAPI.Controllers;
using RumInplicitAPI.Core.Interfaces;
using Xunit;

namespace RumInplicitAPI.Tests.Controllers
{
    public class PmcsControllerTest
    {
        private readonly Mock<IDbAccess> _dbAccessMock;
        private readonly PmcsController _controller;
        public PmcsControllerTest()
        {
            _dbAccessMock = new Mock<IDbAccess>();
            _controller = new PmcsController(_dbAccessMock.Object);
        }

        [Fact]
        public async Task GetAllPmcs_ReturnsAllPmcs_WhenFound()
        {
            //Arrange
            var pageType = "ConsultantPastDue";
            var expectedPmcs = new List<string> { "PMC1", "PMC2", "PMC3" };

            _dbAccessMock.Setup(db => db.GetPmcs(It.IsAny<string>())).ReturnsAsync(expectedPmcs);

            //Act
            var result = await _controller.GetAllPmcs(pageType);

            //Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedPmcs = okResult.Value as List<string>;

            returnedPmcs.Should().NotBeNull();
            returnedPmcs.Should().BeEquivalentTo(expectedPmcs);
        }

        [Fact]
        public async Task GetAllPmcs_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var pageType = "ConsultantPastDue";

            _dbAccessMock.Setup(db => db.GetPmcs(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllPmcs(pageType);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Database error");
        }
    }
}
