using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RumInplicitAPI.BusinessLogic;
using RumInplicitAPI.Core.ApiModels;
using Xunit;

namespace RumInplicitAPI.Tests.DataAccess
{
    public class PastDueDataAccessTest
    {
        private RumInplicitDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RumInplicitDbContext>()
                .UseInMemoryDatabase(databaseName: "RumInplicitDb2")
                .Options;

            var context = new RumInplicitDbContext(options, null);

            return context;
        }

        private void SeedDatabase(RumInplicitDbContext context)
        {
            context.Database.EnsureDeleted(); // Clear existing data
            context.Database.EnsureCreated(); // Create a new empty database
        }

        private void SeedInvoice(RumInplicitDbContext context, int invoiceId)
        {
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = 1,
                CurrentCharges = 1000,
                PriorBalance = 500,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                LateFeeAmount = 50,
                HasContBfs = 1,
                AccountType = "Type1",
                Status = 1
            };

            context.Invoices.Add(invoice);
            context.SaveChanges();
        }

        //GET PASTDUES

        [Theory]
        [InlineData(1, 10, "", "", new string[] { }, "", false, "VendorName", "asc")]
        public async Task GetPastDues_ShouldReturnFilteredInvoices(int page, int pageSize, string invoiceId, string siteName, string[] pmcs, string accountNo, bool isAssignedData, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var invoice = new Invoice
            {
                InvoiceId = 11,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = 1,
                CurrentCharges = 1000,
                PriorBalance = 500,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                LateFeeAmount = 50,
                HasContBfs = 1,
                AccountType = "Type1",
                Status = 1
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = null
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetPastDues(page, pageSize, invoiceId, siteName, pmcs, accountNo, isAssignedData, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().InvoiceId.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData(1, 10, "", "", new string[] { "PMC1" }, "", false, "LateFeeAmount", "desc")]
        public async Task GetPastDues_ShouldReturnFilteredInvoices_ByPmcname_SortedByLateFeeAmountDesc(int page, int pageSize, string invoiceId, string siteName, string[] pmcs, string accountNo, bool isAssignedData, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var invoice = new Invoice
            {
                InvoiceId = 2,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = 1,
                CurrentCharges = 1000,
                PriorBalance = 500,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                LateFeeAmount = 50,
                HasContBfs = 1,
                AccountType = "Type1",
                Status = 1
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = null
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetPastDues(page, pageSize, invoiceId, siteName, pmcs, accountNo, isAssignedData, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().PMCName.Should().Be("PMC1");
            result.First().LateFeeAmount.Should().Be(50);
        }

        [Theory]
        [InlineData(1, 10, "", "", new string[] { }, "V123", false, "VendorName", "asc")]
        public async Task GetPastDues_ShouldReturnFilteredInvoices_ByAccountNumber(int page, int pageSize, string invoiceId, string siteName, string[] pmcs, string accountNo, bool isAssignedData, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var invoice = new Invoice
            {
                InvoiceId = 3,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = 1,
                CurrentCharges = 1000,
                PriorBalance = 500,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                LateFeeAmount = 50,
                HasContBfs = 1,
                AccountType = "Type1",
                Status = 1
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = null
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetPastDues(page, pageSize, invoiceId, siteName, pmcs, accountNo, isAssignedData, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().VendorAccountNo.Should().Be("V123");
        }

        //GET ASSIGNED PASTDUES

        [Theory]
        [InlineData(1, 10, 1, "", "", "VendorName", "asc")]
        public async Task GetPastDuesAssigned_ShouldReturnFilteredInvoices(int page, int pageSize, int userId, string invoiceId, string accountNo, string sortColumn, string sortDirection)
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
                PriorBalanceCalculated = 1,
                CurrentCharges = 1000,
                PriorBalance = 500,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                LateFeeAmount = 50,
                HasContBfs = 1,
                AccountType = "Type1",
                Status = 1
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = userId // Set UserId to match the filter condition
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetPastDuesAssigned(page, pageSize, userId, invoiceId, accountNo, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().VendorName.Should().Be("Vendor A");
        }

        [Theory]
        [InlineData(1, 10, 1, "", "V123", "InvoiceDate", "desc")]
        public async Task GetPastDuesAssigned_ShouldReturnFilteredInvoices_ByAccountNumber(int page, int pageSize, int userId, string invoiceId, string accountNo, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var invoice = new Invoice
            {
                InvoiceId = 5,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = 1,
                CurrentCharges = 1000,
                PriorBalance = 500,
                InvoiceDate = DateTime.Now.AddDays(-10),
                ReceivedDate = DateTime.Now.AddDays(-5),
                SiteName = "Site A",
                LateFeeAmount = 50,
                HasContBfs = 1,
                AccountType = "Type1",
                Status = 1
            };

            var workDetail = new WorkDetail
            {
                InvoiceId = invoice.InvoiceId,
                UserId = userId // Set UserId to match the filter condition
            };

            context.Invoices.Add(invoice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetPastDuesAssigned(page, pageSize, userId, invoiceId, accountNo, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().VendorAccountNo.Should().Be("V123");
        }

        // UPDATE PASTDUES

        [Fact]
        public async Task UpdatePastDues_ShouldUpdateInvoice()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);
            SeedInvoice(context, 1); // Seed with specific invoice ID

            var dbAccess = new DbAccess(context);

            // Act
            var (result, updated) = await dbAccess.UpdatePastDues("1", 2, 3, "Updated notes");

            // Assert
            updated.Should().BeTrue();
            result.Should().NotBeNull();
            result!.RootCause1.Should().Be(2);
            result!.RootCause2.Should().Be(3);
            result!.Notes.Should().Be("Updated notes");

            var updatedInvoice = await context.Invoices.FindAsync(1);
            updatedInvoice.Should().NotBeNull();
            updatedInvoice!.Status.Should().Be(25001);
            updatedInvoice.RootCause1.Should().Be(2);
            updatedInvoice.RootCause2.Should().Be(3);
            updatedInvoice.Notes.Should().Be("Updated notes");
        }

        [Fact]
        public async Task UpdatePastDues_ShouldReturnFalseForNonExistingInvoice()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var dbAccess = new DbAccess(context);

            // Act
            var (result, updated) = await dbAccess.UpdatePastDues("999", 2, 3, "Updated notes");

            // Assert
            updated.Should().BeFalse();
            result.Should().BeNull();
        }
    }
}
