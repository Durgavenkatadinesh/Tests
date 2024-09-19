using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealPage.Logging.Serilog.Middleware;
using RumInplicitAPI.BusinessLogic;
using RumInplicitAPI.Controllers;
using RumInplicitAPI.Core.Interfaces;
using System.Threading.Tasks;
using Xunit;

namespace RumInplicitAPI.Tests.Controllers
{
    public class UserControllerTest
    {
        private readonly Mock<IAuthentication> authenticationMock;
        private readonly Mock<IGeneralAPI> generalAPIMock;
        private readonly Mock<Serilog.ILogger> loggerMock;
        private readonly Mock<IUtility> utilMock;
        private readonly UserController userController;

        public UserControllerTest()
        {
            authenticationMock = new Mock<IAuthentication>();
            generalAPIMock = new Mock<IGeneralAPI>();
            loggerMock = new Mock<Serilog.ILogger>();
            utilMock = new Mock<IUtility>();
            userController = new UserController(authenticationMock.Object, generalAPIMock.Object, loggerMock.Object, utilMock.Object);
        }

        [Fact]
        public async Task GetApplicationToken_ReturnsToken_WhenValidTokenProvided()
        {
            // Arrange
            var token = "validToken";
            var expectedBackOfficeToken = "backOfficeToken";
            var httpContext = new DefaultHttpContext();
            userController.ControllerContext.HttpContext = httpContext;

            if (!httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                httpContext.Request.Headers["Authorization"] = "Bearer " + token;
            }

            authenticationMock.Setup(a => a.Authenticate(httpContext)).Returns(expectedBackOfficeToken);
            utilMock.Setup(u => u.IsSuccess(It.IsAny<int>())).Returns(true);

            // Act
            var result = await userController.GetApplicationToken(token);

            // Assert
            var actionResult = Assert.IsType<ActionResult<string>>(result);
            actionResult.Value.Should().Be(expectedBackOfficeToken);
        }

        [Fact]
        public async Task GetApplicationToken_Returns500_WhenAuthenticationThrowsException()
        {
            // Arrange
            var token = "validToken";
            var httpContext = new DefaultHttpContext();
            userController.ControllerContext.HttpContext = httpContext;

            if (!httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                httpContext.Request.Headers["Authorization"] = "Bearer " + token;
            }

            // Simulate an exception thrown during the authentication process
            authenticationMock
                .Setup(a => a.Authenticate(It.IsAny<HttpContext>()))
                .Throws(new Exception("Authentication failed"));

            // Act
            var result = await userController.GetApplicationToken(token);

            // Assert
            var actionResult = Assert.IsType<ActionResult<string>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            statusCodeResult.StatusCode.Should().Be(500);
        }
    }
}