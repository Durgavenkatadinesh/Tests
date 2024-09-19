using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RumInplicitAPI.BusinessLogic;
using RumInplicitAPI.Core.ApiModels;
using RumInplicitAPI.Core.DTOs;
using Xunit;

namespace RumInplicitAPI.Tests.DataAccess
{
    public class DbAccessTest
    {
        private RumInplicitDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RumInplicitDbContext>()
                .UseInMemoryDatabase(databaseName: "RumInplicitDb1")
                .Options;

            var context = new RumInplicitDbContext(options, null);

            return context;
        }

        private void SeedDatabase1(RumInplicitDbContext context)
        {
            context.Database.EnsureDeleted(); // Clear existing data
            context.Database.EnsureCreated(); // Create a new empty database
        }



        //GET ALL PMCS

        [Fact]
        public async Task GetPmcs_ShouldReturnDistinctPmcsForLateFeePageType()
        {
            // Arrange

            using var context = GetInMemoryDbContext();
            SeedDatabase1(context);

            var invoice1 = new Invoice
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

            var invoice2 = new Invoice
            {
                InvoiceId = 2,
                Pmcname = "PMC2",
                VendorAccountNo = "V456",
                VendorName = "Vendor B",
                SiteId = 2,
                PriorBalanceCalculated = null,
                CurrentCharges = 2000,
                LateFeeAmount = 100,
                InvoiceDate = DateTime.Now.AddDays(-20),
                ReceivedDate = DateTime.Now.AddDays(-15),
                SiteName = "Site B",
                DueDate = DateTime.Now.AddDays(-10),
                TotalAmountDue = 2100,
                Status = 2,
                Ipservice = "AP Extract",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            var invoice3 = new Invoice
            {
                InvoiceId = 3,
                Pmcname = "PMC1",
                VendorAccountNo = "V789",
                VendorName = "Vendor C",
                SiteId = 3,
                PriorBalanceCalculated = null,
                CurrentCharges = 1500,
                LateFeeAmount = 75,
                InvoiceDate = DateTime.Now.AddDays(-30),
                ReceivedDate = DateTime.Now.AddDays(-25),
                SiteName = "Site C",
                DueDate = DateTime.Now.AddDays(-20),
                TotalAmountDue = 1575,
                Status = 3,
                Ipservice = "Bill Payment Processing",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            context.Invoices.AddRange(invoice1, invoice2, invoice3);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var result = await dbAccess.GetPmcs("latefee");

            // Assert
            result.Should().HaveCount(2); // PMC1 and PMC2 should be returned
            result.Should().Contain("PMC1");
            result.Should().Contain("PMC2");
        }

        [Fact]
        public async Task GetPmcs_ShouldReturnDistinctPmcsForOtherPageType()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase1(context);

            var invoice1 = new Invoice
            {
                InvoiceId = 1,
                Pmcname = "PMC1",
                VendorAccountNo = "V123",
                VendorName = "Vendor A",
                SiteId = 1,
                PriorBalanceCalculated = 1,
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

            var invoice2 = new Invoice
            {
                InvoiceId = 2,
                Pmcname = "PMC2",
                VendorAccountNo = "V456",
                VendorName = "Vendor B",
                SiteId = 2,
                PriorBalanceCalculated = 1,
                CurrentCharges = 2000,
                LateFeeAmount = 100,
                InvoiceDate = DateTime.Now.AddDays(-20),
                ReceivedDate = DateTime.Now.AddDays(-15),
                SiteName = "Site B",
                DueDate = DateTime.Now.AddDays(-10),
                TotalAmountDue = 2100,
                Status = 2,
                Ipservice = "AP Extract",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            var invoice3 = new Invoice
            {
                InvoiceId = 3,
                Pmcname = "PMC1",
                VendorAccountNo = "V789",
                VendorName = "Vendor C",
                SiteId = 3,
                PriorBalanceCalculated = 1,
                CurrentCharges = 1500,
                LateFeeAmount = 75,
                InvoiceDate = DateTime.Now.AddDays(-30),
                ReceivedDate = DateTime.Now.AddDays(-25),
                SiteName = "Site C",
                DueDate = DateTime.Now.AddDays(-20),
                TotalAmountDue = 1575,
                Status = 3,
                Ipservice = "Bill Payment Processing",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            context.Invoices.AddRange(invoice1, invoice2, invoice3);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var result = await dbAccess.GetPmcs("other");

            // Assert
            result.Should().HaveCount(2); 
            result.Should().Contain("PMC1");
            result.Should().Contain("PMC2");
        }

        //ASSIGN INVOICES TO USERS

        [Fact]
        public async Task AssignInvoicesToUser_ShouldAssignInvoicesAndUpdateStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase1(context);

            var invoice1 = new Invoice
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
                Status = 0,
                Ipservice = "Bill Payment Processing",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            var invoice2 = new Invoice
            {
                InvoiceId = 2,
                Pmcname = "PMC2",
                VendorAccountNo = "V456",
                VendorName = "Vendor B",
                SiteId = 2,
                PriorBalanceCalculated = null,
                CurrentCharges = 2000,
                LateFeeAmount = 100,
                InvoiceDate = DateTime.Now.AddDays(-20),
                ReceivedDate = DateTime.Now.AddDays(-15),
                SiteName = "Site B",
                DueDate = DateTime.Now.AddDays(-10),
                TotalAmountDue = 2100,
                Status = 0,
                Ipservice = "AP Extract",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            context.Invoices.AddRange(invoice1, invoice2);
            await context.SaveChangesAsync();

            var dbAccess = new DbAccess(context);

            // Act
            await dbAccess.AssignInvoicesToUser(1, 1, new[] { 1, 2 }, "TestUser");

            // Assert
            var updatedInvoice1 = await context.Invoices.FindAsync(1);
            var updatedInvoice2 = await context.Invoices.FindAsync(2);

            updatedInvoice1.Should().NotBeNull();
            updatedInvoice1.Status.Should().Be(25000);

            updatedInvoice2.Should().NotBeNull();
            updatedInvoice2.Status.Should().Be(25000);

            var workDetails = await context.WorkDetails.ToListAsync();
            workDetails.Should().HaveCount(2);

            workDetails.Should().Contain(wd => wd.InvoiceId == 1 && wd.UserId == 1 && wd.UserName == "TestUser");
            workDetails.Should().Contain(wd => wd.InvoiceId == 2 && wd.UserId == 1 && wd.UserName == "TestUser");
        }

        // UNASSIGNING INVOICE

        [Fact]
        public async Task UnassignInvoice_ShouldUpdateStatusAndRemoveWorkDetail()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase1(context);

            var invoice1 = new Invoice
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
                Status = 25000, // Initial status
                Ipservice = "Bill Payment Processing",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            var invoice2 = new Invoice
            {
                InvoiceId = 2,
                Pmcname = "PMC2",
                VendorAccountNo = "V456",
                VendorName = "Vendor B",
                SiteId = 2,
                PriorBalanceCalculated = null,
                CurrentCharges = 2000,
                LateFeeAmount = 100,
                InvoiceDate = DateTime.Now.AddDays(-20),
                ReceivedDate = DateTime.Now.AddDays(-15),
                SiteName = "Site B",
                DueDate = DateTime.Now.AddDays(-10),
                TotalAmountDue = 2100,
                Status = 25000, // Initial status
                Ipservice = "AP Extract",
                Historical = 0,
                InvoiceStatus = "Active"
            };

            var workDetail1 = new WorkDetail
            {
                InvoiceId = invoice1.InvoiceId,
                UserId = 1,
                UserName = "TestUser",
                CreatedBy = 1,
                CreateDate = DateTime.Now,
                ModifiedBy = 1,
                ModifiedDate = DateTime.Now
            };

            var workDetail2 = new WorkDetail
            {
                InvoiceId = invoice2.InvoiceId,
                UserId = 1,
                UserName = "TestUser",
                CreatedBy = 1,
                CreateDate = DateTime.Now,
                ModifiedBy = 1,
                ModifiedDate = DateTime.Now
            };

            context.Invoices.AddRange(invoice1, invoice2);
            context.WorkDetails.AddRange(workDetail1, workDetail2);
            await context.SaveChangesAsync();

            var dbAccess = new DbAccess(context);

            // Act
            await dbAccess.UnassignInvoice(new[] { 1, 2 });

            // Assert
            var updatedInvoice1 = await context.Invoices.FindAsync(1);
            var updatedInvoice2 = await context.Invoices.FindAsync(2);
            var removedWorkDetail1 = await context.WorkDetails.FirstOrDefaultAsync(wd => wd.InvoiceId == 1);
            var removedWorkDetail2 = await context.WorkDetails.FirstOrDefaultAsync(wd => wd.InvoiceId == 2);

            updatedInvoice1.Should().NotBeNull();
            updatedInvoice1.Status.Should().Be(25002);

            updatedInvoice2.Should().NotBeNull();
            updatedInvoice2.Status.Should().Be(25002);

            removedWorkDetail1.Should().BeNull();
            removedWorkDetail2.Should().BeNull();
        }

        //MAPPING ROOTCAUSE

        [Fact]
        public async Task MapRootCauseIds_ShouldReturnCorrectRootCauseMapping()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase1(context);

            var refDetails = new List<RefDetail>
            {
                new RefDetail { RefCodeId = 1, ParentRootCauseId = 100 }, // Parent 100
                new RefDetail { RefCodeId = 2, ParentRootCauseId = 100 }, // Parent 100
                new RefDetail { RefCodeId = 3, ParentRootCauseId = 200 }, // Parent 200
                new RefDetail { RefCodeId = 4, ParentRootCauseId = 0 },   // No Parent
                new RefDetail { RefCodeId = 5, ParentRootCauseId = 300 }  // Parent 300
            };

            context.RefDetails.AddRange(refDetails);
            await context.SaveChangesAsync();

            var dbAccess = new DbAccess(context);

            // Act
            var rootCauseMap = await dbAccess.MapRootCauseIds();

            // Assert
            rootCauseMap.Should().NotBeNull();
            rootCauseMap.Should().HaveCount(3);

            rootCauseMap[100].Should().BeEquivalentTo(new List<int> { 1, 2 });
            rootCauseMap[200].Should().BeEquivalentTo(new List<int> { 3 });
            rootCauseMap[300].Should().BeEquivalentTo(new List<int> { 5 });

            rootCauseMap.ContainsKey(0).Should().BeFalse();
        }


    }
}
