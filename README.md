# What's this?

This a collection of EF Core oddities that were reported to the EF Core
development team for further investigation.

## EFCoreCollectionBug

This shows an interesting behavior of EF Core when modifying an entity and
loading the same entity again before `SaveChanges()`. In this case, added entities
to the initial entities collection are not detected.

Reported as https://github.com/aspnet/EntityFrameworkCore/issues/15794

## EFCoreSortedSetTestCase

Demonstrates the behavior of a `SortedSet` entity collection that after
clearing and adding new entries, looses some of its entries after calling
`SaveChanges()`.

Reported as https://github.com/aspnet/EntityFrameworkCore/issues/16161
