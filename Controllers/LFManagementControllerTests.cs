using Microsoft.AspNetCore.Mvc;
using Moq;
using RumInplicitAPI.Controllers;
using RumInplicitAPI.Core.Interfaces;
using RumInplicitAPI.Core;
using RumInplicitAPI.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using FluentAssertions;

namespace RumInplicitAPI.Tests.Controllers
{



    public class LfManagementControllerTests
    {
        private readonly Mock<IDbAccess> _mockDbAccess;
        private readonly LfManagementController _controller;

        public LfManagementControllerTests()
        {
            _mockDbAccess = new Mock<IDbAccess>();
            _controller = new LfManagementController(_mockDbAccess.Object);
        }


        [Fact]
        public async Task GetAllLfByFilter_ReturnsServerError()
        {
            // Arrange
            var filter = new Filter { page = 1, pageSize = 10 };
            _mockDbAccess.Setup(db => db.GetLateFeesByLimit(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAllLfByFilter(filter);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            
            //use any one both arre same assertt and flntassrt

            //Fluent Asserions
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetAllLfByFilter_ReturnsOk_WithLateFees()
        {
            // Arrange
            var filter = new Filter { page = 1, pageSize = 10 };
            var lateFees = new List<LateFee> { new LateFee { /* populate properties */ } };
            _mockDbAccess.Setup(db => db.GetLateFeesByLimit(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync((lateFees, 1));

            // Act
            var result = await _controller.GetAllLfByFilter(filter);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task AssignInvoices_ReturnsCreated_WhenSuccessful()
        {
            // Arrange
            var assignData = new AssignData { assignerId = 1, userId = 2, invoiceIds = new[] { 1, 2 }, userName = "Test User" };
            _mockDbAccess.Setup(db => db.AssignInvoicesToUser(assignData.assignerId, assignData.userId, assignData.invoiceIds, assignData.userName))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AssignInvoices(assignData);

            // Assert
            result.Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task AssignInvoices_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            var assignData = new AssignData { assignerId = 1, userId = 2, invoiceIds = new[] { 1, 2 }, userName = "Test User" };
            _mockDbAccess.Setup(db => db.AssignInvoicesToUser(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[]>(), It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.AssignInvoices(assignData);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task UnassignInvoices_ReturnsCreated_WhenSuccessful()
        {
            // Arrange
            var invoiceIds = new[] { 1, 2, 3 };
            _mockDbAccess.Setup(db => db.UnassignInvoice(invoiceIds))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UnassignInvoices(invoiceIds);

            // Assert
            result.Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task UnassignInvoices_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            var invoiceIds = new[] { 1, 2, 3 };
            _mockDbAccess.Setup(db => db.UnassignInvoice(It.IsAny<int[]>()))
                         .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UnassignInvoices(invoiceIds);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetAssignedLateFee_ReturnsOk_WithLateFees()
        {
            // Arrange
            var filter = new Filter { page = 1, pageSize = 10, userId = 1 };
            var lateFees = new List<LateFee> { new LateFee { /* populate properties */ } };
            _mockDbAccess.Setup(db => db.GetLateFeesAssigned(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((lateFees, 1));

            // Act
            var result = await _controller.GetAssignedLateFee(filter);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAssignedLateFee_ReturnServerError_WhenExceptionOccurs()
        {
            //Arrange
            var filter = new Filter { page = 1, pageSize = 10, userId = 1 };
            var lateFees = new List<LateFee> { new LateFee { } };
            _mockDbAccess.Setup(db => db.GetLateFeesAssigned(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));

            //Act
            var result = await _controller.GetAssignedLateFee(filter);

            //Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task UpdateLateFee_ReturnsOk_WhenUpdatedSuccessfully()
        {
            // Arrange
            var updateLF = new UpdateLF
            {
                InvoiceId = "123",
                RootCause1 = 1,
                RootCause2 = 2,
                CreditMethod = 1,
                ExpDateToCredit = DateTime.Now.AddMonths(1),
                RequestStatus = 1,
                InvoiceSource = 1,
                WaiverStatus = 0,
                ApprovedAmount = 100.00,
                DeclinedReason = null,
                Remarks = "Updated remarks"
            };

            var lateFee = new LateFee
            {
                InvoiceId = "123",
                PMCName = "PMC1",
                SiteName = "Site1",
                VendorName = "Vendor1",
                ApprovedAmount = 100.00
            };

            _mockDbAccess.Setup(db => db.UpdateLateFee(
                updateLF.InvoiceId,
                updateLF.RootCause1,
                updateLF.RootCause2,
                updateLF.CreditMethod,
                updateLF.ExpDateToCredit,
                updateLF.RequestStatus,
                updateLF.InvoiceSource,
                updateLF.WaiverStatus,
                updateLF.ApprovedAmount,
                updateLF.DeclinedReason,
                updateLF.Remarks))
            .ReturnsAsync((lateFee, true));

            // Act
            var result = await _controller.UpdateLateFee(updateLF);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { data = lateFee });
        }

        [Fact]
        public async Task UpdateLateFee_ReturnsBadRequest_WhenNotUpdated()
        {
            //Arrange
            var updateLF = new UpdateLF
            {
                InvoiceId = "123",
            };
            _mockDbAccess.Setup(db => db.UpdateLateFee(
                 updateLF.InvoiceId,
                updateLF.RootCause1,
                updateLF.RootCause2,
                updateLF.CreditMethod,
                updateLF.ExpDateToCredit,
                updateLF.RequestStatus,
                updateLF.InvoiceSource,
                updateLF.WaiverStatus,
                updateLF.ApprovedAmount,
                updateLF.DeclinedReason,
                updateLF.Remarks)).ReturnsAsync((new LateFee(), false));

            //Act
            var result=await _controller.UpdateLateFee(updateLF);

            //Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task UpdateLateFee_ReturnsNotFound_WhenInvoiceNotFound()
        {
            // Arrange
            var updateLF = new UpdateLF
            {
                InvoiceId = "123",
                // Set other properties...
            };

            _mockDbAccess.Setup(db => db.UpdateLateFee(
                updateLF.InvoiceId,
                updateLF.RootCause1,
                updateLF.RootCause2,
                updateLF.CreditMethod,
                updateLF.ExpDateToCredit,
                updateLF.RequestStatus,
                updateLF.InvoiceSource,
                updateLF.WaiverStatus,
                updateLF.ApprovedAmount,
                updateLF.DeclinedReason,
                updateLF.Remarks))
            .ReturnsAsync((null, true));

            // Act
            var result = await _controller.UpdateLateFee(updateLF);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Invoice Not Found");
        }

        [Fact]
        public async Task UpdateLateFee_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            var updateLF = new UpdateLF
            {
                InvoiceId = "123",
                // Set other properties...
            };

            _mockDbAccess.Setup(db => db.UpdateLateFee(
                updateLF.InvoiceId,
                updateLF.RootCause1,
                updateLF.RootCause2,
                updateLF.CreditMethod,
                updateLF.ExpDateToCredit,
                updateLF.RequestStatus,
                updateLF.InvoiceSource,
                updateLF.WaiverStatus,
                updateLF.ApprovedAmount,
                updateLF.DeclinedReason,
                updateLF.Remarks))
            .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.UpdateLateFee(updateLF);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                  .Which.StatusCode.Should().Be(500);
        }



        [Fact]
        public async Task GetAllLateFees_ReturnsOk_WithLateFees()
        {
            // Arrange
            var lateFees = new List<LateFee> { new LateFee { /* populate properties */ } };
            _mockDbAccess.Setup(db => db.GetAllLateFees()).ReturnsAsync(lateFees);

            // Act
            var result = await _controller.GetAllLateFees();

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllLateFees_ReturnsBadRequest_WhenResultIsNull()
        {
            // Arrange
            var lateFees = new List<LateFee> { new LateFee { /* populate properties */ } };
            _mockDbAccess
                .Setup(db => db.GetAllLateFees())
                .ReturnsAsync((List<LateFee>)null); // Simulating a null result

            // Act
            var result = await _controller.GetAllLateFees();

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task GetAllLateFees_ReturnServerError_WhenExceptionOccurs()
        {
            //Arrange
            var lateFees = new List<LateFee> { new LateFee { } };
            _mockDbAccess.Setup(db => db.GetAllLateFees())
                .ThrowsAsync(new Exception("Test Exception"));

            //Act
            var result = await _controller.GetAllLateFees();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ExportDataAsJSON_ReturnsJsonResult_WithLateFees()
        {
            // Arrange
            var lateFeeInvoices = new List<LateFee> { new LateFee { /* populate properties */ } };
            _mockDbAccess.Setup(db => db.GetAllLateFees()).ReturnsAsync(lateFeeInvoices);

            // Act
            var result = await _controller.ExportDataAsJSON();

            // Assert
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().BeEquivalentTo(lateFeeInvoices);
        }

        [Fact]
        public async Task ExportDataAsJSON_ReturnsServerError_WhenExceptionOccurs()
        {
            //Assign
            var lateFeeInvoices = new List<LateFee> { new LateFee { } };
            _mockDbAccess.Setup(db => db.GetAllLateFees()).ThrowsAsync(new Exception("Test Exception"));

            //Act
            var result = await _controller.ExportDataAsJSON();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}