using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CollectionSaveBug
{
    class Program
    {
        static void SetupDb(DbContextOptions<TestContext> dbOptions)
        {
            var context = new TestContext(dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // populate DB
            var entity1 = new Level1
            {
                Children =
                {
                    new Level2
                    {
                        Name = "Foo",
                        Radius = 10,
                    },
                    new Level2
                    {
                        Name = "Bar",
                        Radius = 20,
                    },
                },
            };
            context.Add(entity1);
            context.SaveChanges();
            Console.WriteLine($"{entity1.Children.Count} children initially - expecting 2.");
        }

        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .EnableSensitiveDataLogging()
                .UseSqlite("Data Source=database.db")
                .Options;

            SetupDb(options);

            // manipulate Level1.Children collection
            var context = new TestContext(options);
            var lvl1 = context.Level1
                .Include(l1 => l1.Children)
                .First();
            
            lvl1.Children.Clear();
            lvl1.Children.Add(new Level2 { Name = "Foo", Radius = 10 });
            lvl1.Children.Add(new Level2 { Name = "Quz", Radius = 30 });
            context.SaveChanges();

            Console.WriteLine($"{lvl1.Children.Count} children after SaveChanges() - expecting 2!");
            Console.ReadKey();
        }
    }

    public class Level1
    {
        public int Id { get; set; }

        [Required]
        public ICollection<Level2> Children { get; set; } = new SortedSet<Level2>();
    }

    public class Level2 : IComparable<Level2>
    {
        public int Id { get; set; }
        public int Radius { get; set; }
        public string Name { get; set; }

        public Level1 Level1 { get; set; }
        public int Level1Id { get; set; }

        public int CompareTo(Level2 other)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Level2)obj;
            return Id == other.Id
                && Radius == other.Radius
                && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Id, Radius, Name).GetHashCode();
        }
    }
}
