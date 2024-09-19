using System.Net;
using DocumentFormat.OpenXml.Office2010.Excel;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RumInplicitAPI.Controllers;
using RumInplicitAPI.Core;
using RumInplicitAPI.Core.Dto;
using RumInplicitAPI.Core.Interfaces;
using Xunit;

namespace RumInplicitAPI.Tests.Controllers
{
    public class PastDueControllerTest
    {
        private readonly Mock<IDbAccess> _dbAccessMock;
        private readonly PastDueController _controller;

        public PastDueControllerTest()
        {
            _dbAccessMock = new Mock<IDbAccess>();
            _controller = new PastDueController(_dbAccessMock.Object);
        }

        [Fact]
        public async Task GetPastDues_ReturnsOkResult_WhenDataIsRetrieved()
        {
            var filter = new Core.Filter
            {
                page = 1,
                pageSize = 10,
                invoiceId = "123",
                siteName = "Site A",
                pmcs = ["PMC1"],
                accountNo = "ACC123",
                isAssignedData = true,
                sortColumn = "date",
                sortDirection = "asc"
            };
            var pastDues = new List<PastDue> { new PastDue() }; //Test Data
            var totalPages = 1;

            _dbAccessMock.Setup(db => db.GetPastDues(
                filter.page,
                filter.pageSize,
                filter.invoiceId,
                filter.siteName,
                filter.pmcs,
                filter.accountNo,
                filter.isAssignedData,
                filter.sortColumn,
                filter.sortDirection))
                .ReturnsAsync((pastDues, totalPages));

            // Act
            var result = await _controller.GetPastDues(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetPastDues_ReturnsInternalServerException()
        {
            //Arrange
            var filter = new Core.Filter();

            _dbAccessMock.Setup(db => db.GetPastDues(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
           It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(),
           It.IsAny<string>(), It.IsAny<string>()))
           .ThrowsAsync(new Exception("Database error"));

            //Act
            var result = await _controller.GetPastDues(filter);

            //Assert
            result.Should().BeOfType<ObjectResult>();
            var okResult = result as ObjectResult;
            okResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetAssignedPastDues_ReturnsOkResult_WhenDataIsRetrieved()
        {
            var filter = new Core.Filter
            {
                page = 1,
                pageSize = 10,
                invoiceId = "123",
                siteName = "Site A",
                pmcs = ["PMC1"],
                accountNo = "ACC123",
                isAssignedData = true,
                sortColumn = "date",
                sortDirection = "asc"
            };
            var pastDues = new List<PastDue> { new PastDue() }; //Test Data
            var totalPages = 1;

            _dbAccessMock.Setup(db => db.GetPastDuesAssigned(
                filter.page,
                filter.pageSize,
                filter.userId,
                filter.invoiceId,
                filter.accountNo,
                filter.sortColumn,
                filter.sortDirection))
                .ReturnsAsync((pastDues, totalPages));

            // Act
            var result = await _controller.GetPastDues(filter);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAssignedPastDues_ReturnsServerError()
        {
            // Arrange
            var filter = new Core.Filter
            {
                page = 1,
                pageSize = 10,
                userId = 500,
                invoiceId = "123",
                accountNo = "ACC123",
                sortColumn = "date",
                sortDirection = "asc"
            };

            // Setup the mock to throw an exception
            _dbAccessMock.Setup(db => db.GetPastDuesAssigned(
                filter.page,
                filter.pageSize,
                filter.userId,
                filter.invoiceId,
                filter.accountNo,
                filter.sortColumn,
                filter.sortDirection))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAssignedPastDues(filter);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);

            var errorResult = result as ObjectResult;
            errorResult.Value.Should().Be("Database error");
        }

        [Fact]
        public async Task GetAssignedPastDues_ReturnsInternalServerException()
        {
            //Arrange
            var filter = new Core.Filter();

            _dbAccessMock.Setup(db => db.GetPastDuesAssigned(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
           It.IsAny<string>(), It.IsAny<string>()))
           .ThrowsAsync(new Exception("Database error"));

            //Act
            var result = await _controller.GetAssignedPastDues(filter);

            //Assert
            result.Should().BeOfType<ObjectResult>();
            var okResult = result as ObjectResult;
            okResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task AssignInvoices_ReturnsOkResult_WhenInvoiceIsAssigned()
        {
            var assignedData = new Core.AssignData
            {
                assignerId = 1,
                userId = 101,
                invoiceIds = new[] { 12 },
                userName = "Karthik"

            };

            _dbAccessMock.Setup(db => db.AssignInvoicesToUser(
                assignedData.assignerId,
                assignedData.userId,
                assignedData.invoiceIds,
                assignedData.userName))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AssignInvoices(assignedData);

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            var okResult = result as StatusCodeResult;
            okResult.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task AssignInvoices_ReturnsInternalServerException()
        {
            //Arrange
            var assignedData = new Core.AssignData();

            _dbAccessMock.Setup(db => db.AssignInvoicesToUser(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int[]>(), It.IsAny<string>()))
           .ThrowsAsync(new Exception("Database error"));

            //Act
            var result = await _controller.AssignInvoices(assignedData);

            //Assert
            result.Should().BeOfType<ObjectResult>();
            var okResult = result as ObjectResult;
            okResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task UnassignInvoices_ReturnsCreatedResult_WhenInvoicesUnassigned()
        {
            // Arrange
            var invoiceIds = new[] { 123, 456 };

            _dbAccessMock.Setup(db => db.UnassignInvoice(invoiceIds))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UnassignInvoices(invoiceIds);

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = result as StatusCodeResult;
            statusCodeResult.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task UnassignInvoices_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var invoiceIds = new[] { 123, 456 };

            _dbAccessMock.Setup(db => db.UnassignInvoice(invoiceIds))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UnassignInvoices(invoiceIds);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Database error");
        }

        [Fact]
        public async Task UpdatePastDues_ReturnsOkResult_WhenInvoiceUpdated()
        {
            // Arrange
            var updatePD = new UpdatePD
            {
                InvoiceId = "123",
                RootCause1 = 1,
                RootCause2 = 2,
                Notes = "Updated notes"
            };

            var pastDue = new PastDue
            {
                InvoiceId = "123",
                RootCause1 = 1,
                RootCause2 = 2,
                Notes = "Updated notes"
            }; // Populate with expected test data

            _dbAccessMock.Setup(db => db.UpdatePastDues(
                updatePD.InvoiceId,
                updatePD.RootCause1,
                updatePD.RootCause2,
                updatePD.Notes))
                .ReturnsAsync((pastDue, true));

            // Act
            var result = await _controller.UpdatePastDues(updatePD);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Ensure the result is not null and cast it to the expected type
            okResult.Should().NotBeNull();
            //var responseData = okResult.Value as dynamic;

            //// Cast responseData.data to PastDue
            //var returnedPastDue = responseData.data as PastDue;

            //// Assert that the returned past due object matches the expected one
            //returnedPastDue.Should().NotBeNull();
            //returnedPastDue.InvoiceId.Should().Be(pastDue.InvoiceId);
            //returnedPastDue.RootCause1.Should().Be(pastDue.RootCause1);
            //returnedPastDue.RootCause2.Should().Be(pastDue.RootCause2);
            //returnedPastDue.Notes.Should().Be(pastDue.Notes);
        }

        [Fact]
        public async Task UpdatePastDues_ReturnsBadRequest_WhenInvoiceNotUpdated()
        {
            // Arrange
            var updatePD = new UpdatePD();

            var pastDue = new PastDue(); // Populate with test data
            _dbAccessMock.Setup(db => db.UpdatePastDues(
                updatePD.InvoiceId,
                updatePD.RootCause1,
                updatePD.RootCause2,
                updatePD.Notes))
                .ReturnsAsync((pastDue, false));

            // Act
            var result = await _controller.UpdatePastDues(updatePD);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task UpdatePastDues_ReturnsNotFound_WhenInvoiceNotFound()
        {
            // Arrange
            var updatePD = new UpdatePD();

            _dbAccessMock.Setup(db => db.UpdatePastDues(
                updatePD.InvoiceId,
                updatePD.RootCause1,
                updatePD.RootCause2,
                updatePD.Notes))
                .ReturnsAsync((null, false));

            // Act
            var result = await _controller.UpdatePastDues(updatePD);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().Be("Invoice Not Found");
        }

        [Fact]
        public async Task UpdatePastDues_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var updatePD = new UpdatePD();

            _dbAccessMock.Setup(db => db.UpdatePastDues(
                updatePD.InvoiceId,
                updatePD.RootCause1,
                updatePD.RootCause2,
                updatePD.Notes))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdatePastDues(updatePD);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Database error");
        }

        [Fact]
        public async Task ExportDataAsJson_ReturnsOk_WhenExportedSuccessfully()
        {
            //Arrange
            var pastDues = new List<PastDue>
            {
                new PastDue { InvoiceId = "123", RootCause1 = 1, RootCause2 = 2, Notes = "Note 1"},
                new PastDue { InvoiceId = "456", RootCause1 = 11, RootCause2 = 22, Notes = "Note 2"}
            };

            _dbAccessMock.Setup(db => db.GetAllPastDues()).ReturnsAsync(pastDues);

            //Act
            var result = await _controller.ExportDataAsJSON();

            //Assert
            result.Should().BeOfType<JsonResult>();
            var jsonResult = result as JsonResult;
            jsonResult.Value.Should().BeEquivalentTo(pastDues);
        }

        [Fact]
        public async Task ExportDataAsJson_ReturnsServerException()
        {
            //Arrange

            _dbAccessMock.Setup(db => db.GetAllPastDues())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.ExportDataAsJSON();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Database error");
        }

    }
}
