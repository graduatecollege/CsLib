# Grad.CsLib.Data

*Generated from Grad.CsLib.Data.xml*


## DatabaseMigrator

**Type**: `Grad.CsLib.Data.DatabaseMigrator`

Handles database migrations using DbUp.

---


### DatabaseMigrator(string)

**Method**: `Grad.CsLib.Data.DatabaseMigrator.#ctor(string)`

Handles database migrations using DbUp and records the migration history into a schema called `DbUp`, which must exist.

**connectionString**: The connection string to the database.

---


### Upgrade()

**Method**: `Grad.CsLib.Data.DatabaseMigrator.Upgrade`

Executes the database migration.

**Returns**: True if successful, false otherwise.

---


## DateOnlyTypeHandler

**Type**: `Grad.CsLib.Data.DateOnlyTypeHandler`

Dapper type handler for DateOnly.

---


## InvalidParameterException

**Type**: `Grad.CsLib.Data.InvalidParameterException`

Represents an exception thrown when an invalid parameter is encountered.

---


## PageOptions

**Type**: `Grad.CsLib.Data.PageOptions`

Page options for database queries.

**PageOffset**: The offset of the page to retrieve.

**PageSize**: The number of items per page.

**SortBy**: The field to sort by.

---


### PageOptions(int,int,string)

**Method**: `Grad.CsLib.Data.PageOptions.#ctor(int,int,string)`

Page options for database queries.

**PageOffset**: The offset of the page to retrieve.

**PageSize**: The number of items per page.

**SortBy**: The field to sort by.

---
