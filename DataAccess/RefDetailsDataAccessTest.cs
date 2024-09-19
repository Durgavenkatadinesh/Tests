using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RumInplicitAPI.BusinessLogic;
using RumInplicitAPI.Core.ApiModels;
using RumInplicitAPI.Core.DTOs;
using Xunit;

namespace RumInplicitAPI.Tests.DataAccess
{
    public class RefDetailsDataAccessTest
    {
        private RumInplicitDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RumInplicitDbContext>()
                .UseInMemoryDatabase(databaseName: "RumInplicitDb")
                .Options;

            var context = new RumInplicitDbContext(options, null);

            return context;
        }

        private void SeedDatabase(RumInplicitDbContext context)
        {
            context.Database.EnsureDeleted(); // Clear existing data
            context.Database.EnsureCreated(); // Create a new empty database
        }

        //GET ALL REF DETAILS

        private void SeedRefDetail(RumInplicitDbContext context, int refCodeId, string entityName, string entityValue)
        {
            var refDetail = new RefDetail
            {
                RefCodeId = refCodeId,
                EntityName = entityName,
                EntityValue = entityValue
            };

            context.RefDetails.Add(refDetail);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetAllRefDetails_ShouldReturnAllReferenceDetails()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            SeedDatabase(context);

            // Seed data
            SeedRefDetail(context, 1, "Entity1", "Value1");
            SeedRefDetail(context, 2, "Entity2", "Value2");
            SeedRefDetail(context, 3, "Entity3", "Value3");

            var dbAccess = new DbAccess(context);

            // Act
            var result = await dbAccess.GetAllRefDetails();

            // Assert
            result.Should().HaveCount(4); // Should return 3 seeded ref details + 1 for key 0
            result.Should().ContainKey(1);
            result.Should().ContainValue("Value1");
            result.Should().ContainKey(2);
            result.Should().ContainValue("Value2");
            result.Should().ContainKey(3);
            result.Should().ContainValue("Value3");
            result.Should().ContainKey(0);
            result[0].Should().BeEmpty(); // Value for key 0 should be an empty string
        }
    }
}
