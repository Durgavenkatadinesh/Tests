using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RumInplicitAPI.BusinessLogic;
using RumInplicitAPI.Core.ApiModels;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RumInplicitAPI.Tests.DataAccess
{
    public class NoticeDataAccessTest
    {
        private RumInplicitDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RumInplicitDbContext>()
                .UseInMemoryDatabase(databaseName: "RumInplicitDb4")
                .Options;

            return new RumInplicitDbContext(options, null);
        }

        private void SeedDatabase(RumInplicitDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        //GET NOTICES BY FILTER

        [Theory]
        [InlineData(1, 10, "", "", new string[] { }, "", false, "VendorName", "asc")]
        public async Task GetNoticeByFilter_ShouldReturnFilteredNotices(int page, int pageSize, string noticeId, string siteName, string[] pmcs, string accountNo, bool isAssignedData, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var notice = new Notice
            {
                Id = 1,
                CdsEntrySequenceId = 1,
                UnitNo = "U123",
                AccountType = "Type1",
                VendorAccountNo = "V123",
                InvoiceId = 1,
                VendorName = "Vendor A",
                SiteId = 1,
                PmcName = "PMC1",
                VarianceAmount = 100,
                PriorBalance = 50,
                ImpactAmount = 150,
                ImpactDate = DateTime.Now.AddDays(-10),
                PostingDate = DateTime.Now.AddDays(-5),
                NoticeDate = DateTime.Now.AddDays(-7),
                ProcessorName = "Processor A",
                NoticeStatus = "",
                NoticeType = "Type1",
                NoticeId = 1,
                InsertDate = DateTime.Now,
                Status = 1,
                SiteName = "Site A"
            };

            context.Notices.Add(notice);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetNoticeByFilter(page, pageSize, noticeId, siteName, pmcs, accountNo, isAssignedData, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().VendorName.Should().Be("Vendor A");
        }

        //GET ASSIGNED NOTICES

        [Theory]
        [InlineData(1, 10, 1, "", "", "VendorName", "asc")]
        public async Task GetAssignedNotices_ShouldReturnFilteredNotices_ByUserId(int page, int pageSize, int userId, string noticeId, string accountNo, string sortColumn, string sortDirection)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var notice = new Notice
            {
                Id = 1,
                CdsEntrySequenceId = 2,
                UnitNo = "U123",
                AccountType = "Type1",
                VendorAccountNo = "V123",
                InvoiceId = 1,
                VendorName = "Vendor A",
                SiteId = 1,
                PmcName = "PMC1",
                VarianceAmount = 100,
                PriorBalance = 50,
                ImpactAmount = 150,
                ImpactDate = DateTime.Now.AddDays(-10),
                PostingDate = DateTime.Now.AddDays(-5),
                NoticeDate = DateTime.Now.AddDays(-7),
                ProcessorName = "Processor A",
                NoticeStatus = "",
                NoticeType = "Type1",
                NoticeId = 2,
                InsertDate = DateTime.Now,
                Status = 1,
                SiteName = "Site A",
                Remarks = "Remark1",
                ResolutionStatus = 2,
                ChangeReason = 2
            };

            var workDetail = new WorkDetail
            {
                NoticeId = 1,
                UserId = userId
            };

            context.Notices.Add(notice);
            context.WorkDetails.Add(workDetail);
            context.SaveChanges();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, totalRows) = await dbAccess.GetAssignedNotices(page, pageSize, userId, noticeId, accountNo, sortColumn, sortDirection);

            // Assert
            totalRows.Should().Be(1);
            result.Should().HaveCount(1);
            result.First().VendorName.Should().Be("Vendor A");
        }

        //GET ASSIGNED NOTICES TO USERS

        [Theory]
        [InlineData(1, 2, new int[] { 1, 2 }, "User A")]
        public async Task AssignNoticesToUser_ShouldAssignNoticesAndCreateWorkDetails(int assignerId, int userId, int[] noticeIds, string userName)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var notice1 = new Notice
            {
                Id = 1,
                Status = 10000,
                VendorName = "Vendor A",
                InvoiceId = 1
            };

            var notice2 = new Notice
            {
                Id = 2,
                Status = 10000,
                VendorName = "Vendor B",
                InvoiceId = 2
            };

            context.Notices.AddRange(notice1, notice2);
            await context.SaveChangesAsync();

            var dbAccess = new DbAccess(context);

            // Act
            await dbAccess.AssignNoticesToUser(assignerId, userId, noticeIds, userName);

            // Assert
            var assignedNotice1 = await context.Notices.FirstOrDefaultAsync(n => n.Id == 1);
            var assignedNotice2 = await context.Notices.FirstOrDefaultAsync(n => n.Id == 2);
            var workDetail1 = await context.WorkDetails.FirstOrDefaultAsync(wd => wd.NoticeId == 1);
            var workDetail2 = await context.WorkDetails.FirstOrDefaultAsync(wd => wd.NoticeId == 2);

            assignedNotice1!.Status.Should().Be(25000);
            assignedNotice2!.Status.Should().Be(25000);

            workDetail1.Should().NotBeNull();
            workDetail1!.UserName.Should().Be(userName);
            workDetail1.UserId.Should().Be(userId);
            workDetail1.CreatedBy.Should().Be(assignerId);

            workDetail2.Should().NotBeNull();
            workDetail2!.UserName.Should().Be(userName);
            workDetail2.UserId.Should().Be(userId);
            workDetail2.CreatedBy.Should().Be(assignerId);
        }


        //UNASSIGNING NOTICES

        [Theory]
        [InlineData(new int[] { 1, 2 })]
        public async Task UnassignNotice_ShouldUnassignNoticesAndRemoveWorkDetails(int[] noticeIds)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var notice1 = new Notice
            {
                Id = 1,
                Status = 25000, // Initial status
                VendorName = "Vendor A",
                InvoiceId = 1
            };

            var notice2 = new Notice
            {
                Id = 2,
                Status = 25000, // Initial status
                VendorName = "Vendor B",
                InvoiceId = 2
            };

            var workDetail1 = new WorkDetail
            {
                NoticeId = 1,
                UserId = 1,
                UserName = "User A"
            };

            var workDetail2 = new WorkDetail
            {
                NoticeId = 2,
                UserId = 2,
                UserName = "User B"
            };

            context.Notices.AddRange(notice1, notice2);
            context.WorkDetails.AddRange(workDetail1, workDetail2);
            await context.SaveChangesAsync();

            var dbAccess = new DbAccess(context);

            // Act
            await dbAccess.UnassignNotice(noticeIds);

            // Assert
            var updatedNotice1 = await context.Notices.FirstOrDefaultAsync(n => n.Id == 1);
            var updatedNotice2 = await context.Notices.FirstOrDefaultAsync(n => n.Id == 2);

            updatedNotice1.Should().NotBeNull();
            updatedNotice1!.Status.Should().Be(25002); // Status should be updated to 25002

            updatedNotice2.Should().NotBeNull();
            updatedNotice2!.Status.Should().Be(25002); // Status should be updated to 25002

            var workDetailAfterUnassign1 = await context.WorkDetails.FirstOrDefaultAsync(wd => wd.NoticeId == 1);
            var workDetailAfterUnassign2 = await context.WorkDetails.FirstOrDefaultAsync(wd => wd.NoticeId == 2);

            workDetailAfterUnassign1.Should().BeNull(); // Work detail should be removed
            workDetailAfterUnassign2.Should().BeNull(); // Work detail should be removed
        }

        //UPDATE NOTICES

        [Theory]
        [InlineData("1", 2, 3, "Updated Remarks")]
        public async Task UpdateNotice_ShouldUpdateNoticeDetails(string id, int resolutionStatus, int changeReason, string remarks)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var notice = new Notice
            {
                Id = 1,
                NoticeId = 123,
                VendorName = "Vendor A",
                InvoiceId = 1001,
                NoticeType = "Type1",
                NoticeStatus = "Pending",
                NoticeDate = DateTime.Now.AddDays(-10),
                PostingDate = DateTime.Now.AddDays(-5),
                ImpactDate = DateTime.Now.AddDays(-7),
                ImpactAmount = 200,
                PriorBalance = 100,
                VarianceAmount = 50,
                PmcName = "PMC1",
                SiteName = "Site A",
                SiteId = 10,
                AccountType = "Type A",
                UnitNo = "U123",
                InsertDate = DateTime.Now,
                Status = 25000,
                Remarks = "Initial Remarks",
                ResolutionStatus = 1,
                ChangeReason = 1
            };

            context.Notices.Add(notice);
            await context.SaveChangesAsync();

            var dbAccess = new DbAccess(context);

            // Act
            var (result, updated) = await dbAccess.UpdateNotice(id, resolutionStatus, changeReason, remarks);

            // Assert
            updated.Should().BeTrue(); // Ensure the update flag is set to true

            result.Should().NotBeNull();
            result!.ResolutionStatus.Should().Be(resolutionStatus); // Verify the resolution status is updated
            result.ChangeReason.Should().Be(changeReason); // Verify the change reason is updated
            result.Remarks.Should().Be(remarks); // Verify the remarks are updated
            result.Status.Should().Be(25001); // Status should be set to 25001
        }

        //GET ALL NOTICES

        [Fact]
        public async Task GetAllNotices_ShouldReturnAllNoticesWithDetails()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            var notice1 = new Notice
            {
                Id = 1,
                CdsEntrySequenceId = 2,
                UnitNo = "U123",
                AccountType = "Type1",
                VendorAccountNo = "V123",
                InvoiceId = 1001,
                VendorName = "Vendor A",
                SiteId = 1,
                PmcName = "PMC1",
                VarianceAmount = 100,
                PriorBalance = 50,
                ImpactAmount = 150,
                ImpactDate = DateTime.Now.AddDays(-10),
                PostingDate = DateTime.Now.AddDays(-5),
                NoticeDate = DateTime.Now.AddDays(-7),
                ProcessorName = "Processor A",
                NoticeStatus = "Pending",
                NoticeType = "Type1",
                NoticeId = 123,
                InsertDate = DateTime.Now,
                Status = 1,
                SiteName = "Site A",
                Remarks = "Initial Remark",
                ResolutionStatus = 2,
                ChangeReason = 2
            };

            var notice2 = new Notice
            {
                Id = 2,
                CdsEntrySequenceId = 3,
                UnitNo = "U124",
                AccountType = "Type2",
                VendorAccountNo = "V124",
                InvoiceId = 1002,
                VendorName = "Vendor B",
                SiteId = 2,
                PmcName = "PMC2",
                VarianceAmount = 200,
                PriorBalance = 100,
                ImpactAmount = 250,
                ImpactDate = DateTime.Now.AddDays(-12),
                PostingDate = DateTime.Now.AddDays(-8),
                NoticeDate = DateTime.Now.AddDays(-10),
                ProcessorName = "Processor B",
                NoticeStatus = "InProgress",
                NoticeType = "Type2",
                NoticeId = 124,
                InsertDate = DateTime.Now,
                Status = 1,
                SiteName = "Site B",
                Remarks = "Initial Remark 2",
                ResolutionStatus = 3,
                ChangeReason = 3
            };

            var workDetail = new WorkDetail
            {
                NoticeId = 1,
                UserName = "User A"
            };

            context.Notices.AddRange(notice1, notice2);
            context.WorkDetails.Add(workDetail);
            await context.SaveChangesAsync();

            var dbAccess = new DbAccess(context);

            // Act
            var result = await dbAccess.GetAllNotices();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var notice1Dto = result.Find(n => n.Id == 1);
            notice1Dto.Should().NotBeNull();
            notice1Dto!.VendorName.Should().Be("Vendor A");
            notice1Dto.UserName.Should().Be("User A"); // WorkDetail join should populate UserName

            var notice2Dto = result.Find(n => n.Id == 2);
            notice2Dto.Should().NotBeNull();
            notice2Dto!.VendorName.Should().Be("Vendor B");
            notice2Dto.UserName.Should().BeNull(); // WorkDetail doesn't exist for this notice
        }


    }
}