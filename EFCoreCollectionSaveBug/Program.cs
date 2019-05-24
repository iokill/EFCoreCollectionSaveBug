using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CollectionSaveBug
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .EnableSensitiveDataLogging()
                .UseSqlite("Data Source=database.db")
                .Options;

            var context = new TestContext(options);
            context.Database.EnsureCreated();

            // populate DB
            var entity1 = new Level1
            {
                Children = 
                {
                    new Level2
                    {
                        Children =
                        {
                            new Level3 { Id = 1, Value = "1" },
                            new Level3 { Id = 2, Value = "2" },
                        },
                    },
                },
            };
            var entity2 = new Level1
            {
                Children =
                {
                    new Level2
                    {
                        Children =
                        {
                            new Level3 { Id = 3, Value = "3" },
                            new Level3 { Id = 4, Value = "4" },
                        },
                    },
                },
            };
            context.Add(entity1);
            context.Add(entity2);
            context.SaveChanges();
            var lvl1Id = entity1.Id;
            var lvl2Id = entity1.Children.First().Id;

            // modify entity1
            context = new TestContext(options);
            var lvl2 = context.Level2
                .Include(l2 => l2.Children)
                .Single(l2 => l2.Id == lvl2Id);
            Console.WriteLine($"{lvl2.Children.Count} children (initially).");

            lvl2.Children.First().Value = "changed";
            lvl2.Children.Add(new Level3 { Value = "5" });

            // load other parents
            // XXX: this seems to confuse change tracking in EF Core to
            // ignore the previously added Level3 instance
            entity1 = context.Level1
                .Include(l1 => l1.Children)
                    .ThenInclude(l2 => l2.Children)
                .Single(l1 => l1.Id == lvl1Id);
            Console.WriteLine($"{lvl2.Children.Count} children (after modify, before save).");

            // this should update the first parent's children with an addtional entry
            // and one modification to an existing entry
            context.SaveChanges();

            // check what was inserted
            context = new TestContext(options);
            lvl2 = context.Level2
                .Include(l2 => l2.Children)
                .Single(l2 => l2.Id == lvl2Id);
            Console.WriteLine($"{lvl2.Children.Count} children (after save). Should be 3!");
            Console.ReadKey();
        }
    }

    public class Level1
    {
        public int Id { get; set; }
        public ICollection<Level2> Children { get; set; } = new List<Level2>();
    }

    public class Level2
    {
        public int Id { get; set; }
        public ICollection<Level3> Children { get; set; } = new List<Level3>();

        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Level2)obj;

            return Id == other.Id && Enumerable.SequenceEqual(Children, other.Children);
        }


        public override int GetHashCode()
        {
            return Tuple.Create(Id, Children).GetHashCode();
        }
    }

    public class Level3
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = (Level3)obj;

            return Id == that.Id && Value == that.Value;
        }


        public override int GetHashCode()
        {
            return Tuple.Create(Id, Value).GetHashCode();
        }
    }
}
