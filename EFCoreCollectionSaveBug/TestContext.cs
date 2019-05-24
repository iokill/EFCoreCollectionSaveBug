using Microsoft.EntityFrameworkCore;

namespace CollectionSaveBug
{
    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions<TestContext> options) : base(options)
        {
        }

        public DbSet<Level1> Level1 { get; set; }
        public DbSet<Level2> Level2 { get; set; }
    }
}
