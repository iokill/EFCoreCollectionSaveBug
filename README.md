# What?

This shows an interesting behavior of EF Core when modifying an entity and
loading the same entity again before SaveChanges. In such cases added entities
to the initial entities collection are not detected.
