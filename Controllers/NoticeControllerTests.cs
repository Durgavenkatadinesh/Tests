using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RumInplicitAPI.Controllers;
using RumInplicitAPI.Core;
using RumInplicitAPI.Core.ApiModels;
using RumInplicitAPI.Core.Dto;
using RumInplicitAPI.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RumInplicitAPI.Tests.Controllers
{
    public class NoticeControllerTests
    {
        private readonly Mock<IDbAccess> _mockDbAccess;
        private readonly NoticeController _controller;

        public NoticeControllerTests()
        {
            _mockDbAccess = new Mock<IDbAccess>();
            _controller = new NoticeController(_mockDbAccess.Object);
        }

       

        [Fact]
        public async Task GetNotices_ReturnsOk_WithNotices()
        {
            // Arrange
            var filter = new Filter
            {
                page = 1,
                pageSize = 10,
                invoiceId = "INV123"
            };

            var notices = new List<NoticesDTO>
    {
        new NoticesDTO { InvoiceId = "INV123", ResolutionStatus = 1 },
        new NoticesDTO { InvoiceId = "INV124", ResolutionStatus = 2 }
    };
            int totalPages = 1;

            _mockDbAccess.Setup(db => db.GetNoticeByFilter(
                filter.page,
                filter.pageSize,
                filter.invoiceId,
                filter.siteName,
                filter.pmcs,
                filter.accountNo,
                filter.isAssignedData,
                filter.sortColumn,
                filter.sortDirection)).ReturnsAsync((notices, totalPages));

            // Act
            var result = await _controller.GetNotices(filter);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull();
        }


        [Fact]
        public async Task GetNotices_ReturnsServerError_OnException()
        {
            // Arrange
            var filter = new Filter { page = 1, pageSize = 10 };
            _mockDbAccess.Setup(db => db.GetNoticeByFilter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetNotices(filter);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Database error", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetAssignedNotices_ReturnsOkResult_WhenNoticesAreFound()
        {
            // Arrange
            var filter = new Filter { page = 1, pageSize = 10, userId = 1 };
            var notices = new List<NoticesDTO> { new NoticesDTO { Id = 1, InvoiceId = "INV001" } };
            var totalPages = 1;

            _mockDbAccess
                .Setup(db => db.GetAssignedNotices(filter.page, filter.pageSize, filter.userId, null, null, null, null))
                .ReturnsAsync((notices, totalPages));

            // Act
            var result = await _controller.GetAssignedNotices(filter);

            // Assert
            //var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            //var response = okResult.Value.Should().BeOfType<dynamic>().Subject;
            //response.data.Should().BeEquivalentTo(notices);
            //response.totalRows.Should().Be(totalPages);
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAssignedNotices_Returns500_WhenExceptionIsThrown()
        {
            // Arrange
            var filter = new Filter
            {
                page = 1,
                pageSize = 10,
                userId = 1,
                invoiceId = "INV001",
                accountNo = "ACC001",
                sortColumn = "date",
                sortDirection = "asc"
            };

            // Set up the mock to throw an exception when the method is called
            _mockDbAccess
                .Setup(db => db.GetAssignedNotices(
                    filter.page,
                    filter.pageSize,
                    filter.userId,
                    filter.invoiceId,
                    filter.accountNo,
                    filter.sortColumn,
        filter.sortDirection))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAssignedNotices(filter);

            // Assert
            result.Should().BeOfType<ObjectResult>()
        .Which.StatusCode.Should().Be(500);

            var errorResult = result as ObjectResult;
            errorResult.Value.Should().Be("Database error");
        }

        [Fact]
        public async Task UpdateNotice_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updateNotice = new UpdateNotice
            {
                InvoiceId = "INV123",
                ResolutionStatus = 1,
                ChangeReason = 2,
                Remarks = "Updated remarks"
            };

            var notice = new NoticesDTO
            {
                Id = 1,
                InvoiceId = "INV123",
                ResolutionStatus = 1
            };

            _mockDbAccess.Setup(db => db.UpdateNotice(
                updateNotice.InvoiceId,
                updateNotice.ResolutionStatus,
                updateNotice.ChangeReason,
                updateNotice.Remarks)).ReturnsAsync((notice, true));

            // Act
            var result = await _controller.UpdateNotice(updateNotice);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { data = notice });
        }

        [Fact]
        public async Task UpdateNotice_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var updateNotice = new UpdateNotice
            {
                InvoiceId = "INV123",
                ResolutionStatus = 1,
                ChangeReason = 2,
                Remarks = "Updated remarks"
            };

            var notice = new NoticesDTO { InvoiceId = "INV123", ResolutionStatus = 1 };
            _mockDbAccess.Setup(db => db.UpdateNotice(
                updateNotice.InvoiceId,
                updateNotice.ResolutionStatus,
                updateNotice.ChangeReason,
                updateNotice.Remarks)).ReturnsAsync((notice, false));

            // Act
            var result = await _controller.UpdateNotice(updateNotice);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task UpdateNotice_ReturnsNotFound_WhenInvoiceNotFound()
        {
            // Arrange
            var updateNotice = new UpdateNotice
            {
                InvoiceId = "INV123",
                ResolutionStatus = 1,
                ChangeReason = 2,
                Remarks = "Updated remarks"
            };

            _mockDbAccess.Setup(db => db.UpdateNotice(
                updateNotice.InvoiceId,
                updateNotice.ResolutionStatus,
                updateNotice.ChangeReason,
                updateNotice.Remarks)).ReturnsAsync((null, false));

            // Act
            var result = await _controller.UpdateNotice(updateNotice);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("Invoice Not Found");
        }

        [Fact]
        public async Task UpdateNotice_Returns500_WhenExceptionThrown()
        {
            // Arrange
            var updateNotice = new UpdateNotice
            {
                InvoiceId = "INV123",
                ResolutionStatus = 1,
                ChangeReason = 2,
                Remarks = "Updated remarks"
            };

            _mockDbAccess.Setup(db => db.UpdateNotice(
                updateNotice.InvoiceId,
                updateNotice.ResolutionStatus,
                updateNotice.ChangeReason,
                updateNotice.Remarks)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateNotice(updateNotice);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Database error");
        }

        [Fact]
        public async Task AssignNotices_ReturnsCreated()
        {
            // Arrange
            var assignData = new AssignData { assignerId = 1, userId = 2, invoiceIds = new[] { 1, 2 }, userName = "Test User" };

            // Act
            var result = await _controller.AssignNotices(assignData);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task AssignNotices_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            var assignData = new AssignData { assignerId = 1, userId = 2, invoiceIds = new[] { 1, 2 }, userName = "Test User" };

            _mockDbAccess.Setup(db => db.AssignNoticesToUser(assignData.assignerId, assignData.userId, assignData.invoiceIds, assignData.userName))
                         .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.AssignNotices(assignData);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task UnassignNotices_Returns201Created_WhenSuccessful()
        {
            // Arrange
            int[] invoiceIds = { 1, 2, 3 };
            _mockDbAccess.Setup(db => db.UnassignNotice(invoiceIds)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UnassignNotices(invoiceIds);

            // Assert
            result.Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task UnassignNotices_Returns500_WhenExceptionThrown()
        {
            // Arrange
            int[] invoiceIds = { 1, 2, 3 };
            _mockDbAccess.Setup(db => db.UnassignNotice(invoiceIds)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UnassignNotices(invoiceIds);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Database error");
        }

        [Fact]
        public async Task ExportDataAsJSON_ReturnsJsonResult()
        {
            // Arrange
            var noticeInvoices = new List<NoticesDTO>
            {
                new NoticesDTO { Id = 1, InvoiceId = "INV-001", NoticeType = "Type A" }
            };
            _mockDbAccess.Setup(db => db.GetAllNotices()).ReturnsAsync(noticeInvoices);

            // Act
            var result = await _controller.ExportDataAsJSON();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(noticeInvoices, jsonResult.Value);
        }

        [Fact]
        public async Task ExportDataAsJSON_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            var noticeInvoices = new List<NoticesDTO>
            {
                new NoticesDTO { Id = 1, InvoiceId = "INV-001", NoticeType = "Type A" }
            };
            _mockDbAccess.Setup(db => db.GetAllNotices()).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.ExportDataAsJSON();

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}