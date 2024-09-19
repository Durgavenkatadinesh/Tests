using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RumInplicitAPI.BusinessLogic;
using RumInplicitAPI.Core.ApiModels;
using Xunit;

namespace RumInplicitAPI.Tests.DataAccess
{
    public class LateFeeDataAccessTest
    {
        private RumInplicitDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RumInplicitDbContext>()
                .UseInMemoryDatabase(databaseName: "RumInplicitDb3")
                .Options;

            var context = new RumInplicitDbContext(options, null);

            return context;
        }

        private void SeedDatabase(RumInplicitDbContext context)
        {
            context.Database.EnsureDeleted(); // Clear existing data
            context.Database.EnsureCreated(); // Create a new empty database
        }

        //GET ALL LATEFEES

        [Fact]
        public async Task GetAllLateFees_ShouldReturnLateFees()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            // Seed test data
            var invoice1 = new Invoice
            {
                InvoiceId = 1,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                PostingDate = DateTime.Now.AddDays(-30),
                InvoiceDate = DateTime.Now.AddDays(-30),
                SiteName = "Site A",
                Ipservice = "Bill Payment Processing",
                Historical = 0,
                InvoiceStatus = "Active",
                LateFeeAmount = 100,
                TotalAmountDue = 200,
                ReceivedDate = DateTime.Now.AddDays(-15),
                DueDate = DateTime.Now.AddDays(-10),
                RootCause1 = 101,
                RootCause2 = 102,
                CreditMethod = 1,
                ExpDateToCredit = DateTime.Now.AddDays(30),
                RequestStatus = 1,
                WaiverStatus = 201,
                ApprovedAmount = 50,
                DeclinedReason = 20,
                Remarks = "Remarks1",
                InvoiceSource = 1
            };

            var workDetail1 = new WorkDetail
            {
                InvoiceId = invoice1.InvoiceId,
                UserName = "User1",
                CreateDate = DateTime.Now.AddDays(-5)
            };

            context.Invoices.Add(invoice1);
            context.WorkDetails.Add(workDetail1);

            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var result = await dbAccess.GetAllLateFees();

            // Assert
            result.Should().HaveCount(1);
            var lateFee = result.First();
            lateFee.PMCName.Should().Be("PMC1");
            lateFee.VendorAccountNo.Should().Be("V123");
            lateFee.VendorName.Should().Be("Vendor A");
        }

        //GET LATEFEES BY LIMIT

        [Theory]
        [InlineData(1, 10, "", "", new string[] { }, "", false, "VendorName", "asc")]
        public async Task GetLateFeesByLimit_ShouldReturnFilteredLateFees(int page, int pageSize, string invoiceId, string siteName, string[] pmcs, string accountNo, bool isAssignedData, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var invoice = new Invoice
            {
                InvoiceId = 1,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = null,
                CurrentCharges = 1000,
                LateFeeAmount = 50,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                DueDate = DateTime.Now.AddDays(-1),
                TotalAmountDue = 1050,
                Status = 1,
                Ipservice = "Bill Payment Processing",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = null, // Adjust as needed for the isAssignedData flag
                CreateDate = DateTime.Now
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetLateFeesByLimit(page, pageSize, invoiceId, siteName, pmcs, accountNo, isAssignedData, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().VendorName.Should().Be("Vendor A");
        }

        [Theory]
        [InlineData(1, 10, "", "", new string[] { "PMC1" }, "", false, "LateFeeAmount", "desc")]
        public async Task GetLateFeesByLimit_ShouldReturnFilteredLateFees_SortedByLateFeeAmountDesc(int page, int pageSize, string invoiceId, string siteName, string[] pmcs, string accountNo, bool isAssignedData, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var invoice = new Invoice
            {
                InvoiceId = 1,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = null,
                CurrentCharges = 1000,
                LateFeeAmount = 50,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                DueDate = DateTime.Now.AddDays(-1),
                TotalAmountDue = 1050,
                Status = 1,
                Ipservice = "Bill Payment Processing",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = null,
                CreateDate = DateTime.Now
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetLateFeesByLimit(page, pageSize, invoiceId, siteName, pmcs, accountNo, isAssignedData, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().LateFeeAmount.Should().Be(50);
        }

        //GET LATEFEE ASSIGNED
        private void SeedInvoice(RumInplicitDbContext context, int invoiceId, int userId)
        {
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = null,
                CurrentCharges = 1000,
                PriorBalance = 500,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                LateFeeAmount = 50,
                HasContBfs = 1,
                AccountType = "Type1",
                Status = 1,
                Ipservice = "Bill Payment Processing"
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = userId,
                UserName = "Test User",
                CreateDate = DateTime.Now
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();
        }
        private void SeedInvoice(RumInplicitDbContext context, int invoiceId, int userId, decimal lateFeeAmount)
        {
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                Pmcname = "PMCName" + invoiceId,
                VendorAccountNo = "AccountNo" + invoiceId,
                VendorName = "VendorName" + invoiceId,
                PostingDate = DateTime.Now,
                InvoiceDate = DateTime.Now.AddDays(-10),
                SiteName = "SiteName" + invoiceId,
                DueDate = DateTime.Now.AddDays(10),
                LateFeeAmount = lateFeeAmount,
                TotalAmountDue = 1000,
                ReceivedDate = DateTime.Now,
                Status = 0,
                RootCause1 = 1,
                RootCause2 = 2,
                Notes = "Some notes",
                Historical = 0,
                InvoiceStatus = "Active",
                PriorBalanceCalculated = null
            };

            context.Invoices.Add(invoice);

            var workDetail = new WorkDetail
            {
                InvoiceId = invoiceId,
                UserId = userId,
                UserName = "UserName" + userId,
                CreatedBy = userId,
                CreateDate = DateTime.Now,
                ModifiedBy = userId,
                ModifiedDate = DateTime.Now,
                NoticeId = null
            };

            context.WorkDetails.Add(workDetail);

            context.SaveChanges();
        }

        [Theory]
        [InlineData(1, 10, 1, "", "", "InvoiceDate", "asc")]
        public async Task GetLateFeesAssigned_ShouldReturnFilteredAndSortedInvoices(int page, int pageSize, int userId, string invoiceId, string accountNo, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);
            SeedInvoice(context, 1, userId);
            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetLateFeesAssigned(page, pageSize, userId, invoiceId, accountNo, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            var firstResult = result.First();
            firstResult.VendorName.Should().Be("Vendor A");
            firstResult.LateFeeAmount.Should().Be(50);
            firstResult.InvoiceId.Should().Be("1");
        }

        [Theory]
        [InlineData(1, 10, 1, "", "V123", "VendorName", "asc")]
        public async Task GetLateFeesAssigned_ShouldReturnFilteredInvoices_ByAccountNumber(int page, int pageSize, int userId, string invoiceId, string accountNo, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);
            SeedInvoice(context, 1, userId);
            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetLateFeesAssigned(page, pageSize, userId, invoiceId, accountNo, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().VendorAccountNo.Should().Be("V123");
        }

        //UPDATE LATE FEES
        private void SeedInvoice(RumInplicitDbContext context, int invoiceId)
        {
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                Pmcname = "Test PMC",
                VendorAccountNo = "12345",
                VendorName = "Test Vendor",
                PostingDate = DateTime.Now,
                InvoiceDate = DateTime.Now.AddDays(-30),
                SiteName = "Test Site",
                DueDate = DateTime.Now.AddDays(-15),
                LateFeeAmount = 100,
                TotalAmountDue = 150,
                ReceivedDate = DateTime.Now.AddDays(-10),
                CreditMethod = 1,
                ExpDateToCredit = DateTime.Now.AddMonths(1),
                RequestStatus = 1,
                InvoiceSource = 1,
                WaiverStatus = 1,
                ApprovedAmount = 75.00,
                DeclinedReason = 0,
                Remarks = "Initial Remarks",
                RootCause1 = 1,
                RootCause2 = 2,
                Status = 10000 // Some initial status
            };

            context.Invoices.Add(invoice);
            context.SaveChanges();
        }

        [Theory]
        [InlineData(1, 2, 3, 4, "2024-12-31", 5, 6, 7, 8.50, 9, "Updated Remarks")]
        public async Task UpdateLateFee_ShouldUpdateLateFee(int invoiceId, int? rootCause1, int? rootCause2, int? creditMethod, string expDateToCreditStr, int? requestStatus, int? invoiceSource, int? waiverStatus, double? approvedAmount, int? declinedReason, string? remarks)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);
            SeedInvoice(context, invoiceId);
            var dbAccess = new DbAccess(context);

            // Convert the string to DateTime
            DateTime? expDateToCredit = string.IsNullOrEmpty(expDateToCreditStr)
                ? (DateTime?)null
                : DateTime.Parse(expDateToCreditStr);

            // Act
            var (result, updated) = await dbAccess.UpdateLateFee(
                invoiceId.ToString(),
                rootCause1,
                rootCause2,
                creditMethod,
                expDateToCredit,
                requestStatus,
                invoiceSource,
                waiverStatus,
                approvedAmount,
                declinedReason,
                remarks
            );

            // Assert
            updated.Should().BeTrue();
            result.Should().NotBeNull();
            result!.RootCause1.Should().Be(rootCause1);
            result!.RootCause2.Should().Be(rootCause2);
            result!.CreditMethod.Should().Be(creditMethod);
            result!.ExpDateToCredit.Should().Be(expDateToCredit);
            result!.RequestStatus.Should().Be(requestStatus);
            result!.InvoiceSource.Should().Be(invoiceSource);
            result!.WaiverStatus.Should().Be(waiverStatus);
            result!.ApprovedAmount.Should().Be(approvedAmount);
            result!.DeclinedReason.Should().Be(declinedReason);
            result!.Remarks.Should().Be(remarks);
            result!.Status.Should().Be(25001); // Status after update
        }

    }
}
