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

---


### PageOptions(int,int,string)

**Method**: `Grad.CsLib.Data.PageOptions.#ctor(int,int,string)`

Page options for database queries.

**PageOffset**: The offset of the page to retrieve.

**PageSize**: The number of items per page.

---


## QueryableExtensions

**Type**: `Grad.CsLib.Data.QueryableExtensions`

Extension methods to perform Order By using strings

---


### System.Linq.IQueryable<T>.OrderByPath(string)

**Method**: `Grad.CsLib.Data.QueryableExtensions.OrderByPath(System.Linq.IQueryable<T>,string)`

Orders an `System.Linq.IQueryable<T>` source based on a specified column path string.
 The column path can include multiple column names separated by commas,
 with optional '-' prefix for descending order.

**columnPath**: A string representing the column path(s) to sort by.
 Columns are separated by commas, and a '-' prefix indicates descending order for a column.

**Returns**: An `System.Linq.IOrderedQueryable<T>` that is ordered based on the column path string provided.

**Throws T:Grad.CsLib.Data.InvalidParameterException**: Thrown when the column path is null, empty, or contains invalid fields.

---


### System.Linq.IQueryable<T>.ApplyFilters(object,string[])

**Method**: `Grad.CsLib.Data.QueryableExtensions.ApplyFilters(System.Linq.IQueryable<T>,object,string[])`

Applies a set of filters to an `System.Linq.IQueryable<T>` source based on the specified filter object.
 Filters are derived from the properties of the filter object, where non-null, non-empty values
 indicate conditions for inclusion. String values are matched exactly, while enumerable string
 values are checked for containment. Boolean values determine the presence or absence of a field.

**filter**: An object containing properties used to filter the source. Each property name is matched
 to a corresponding property in the source type to form filtering conditions.

**except**: A list of property names to exclude from the filtering process, even if they exist in the filter object.

**Returns**: A filtered `System.Linq.IQueryable<T>` sequence, with conditions applied based on the filter object and exclusions.

---


### System.Linq.IQueryable<T>.ApplySorting(Grad.CsLib.Data.PageOptions,System.Linq.Expressions.Expression<Func<T,object>>,System.Linq.Expressions.Expression<Func<T,object>>[])

**Method**: `Grad.CsLib.Data.QueryableExtensions.ApplySorting(System.Linq.IQueryable<T>,Grad.CsLib.Data.PageOptions,System.Linq.Expressions.Expression<Func<T,object>>,System.Linq.Expressions.Expression<Func<T,object>>[])`

Applies sorting to an `System.Linq.IQueryable<T>` source based on the provided `Grad.CsLib.Data.PageOptions`.
 If the `SortBy` property in the `pageOptions` is specified,
 sorting is performed using the column(s) defined in it. If no `SortBy` is provided,
 the default and optional sorting expressions are applied.

**pageOptions**: The `Grad.CsLib.Data.PageOptions` object containing sorting preferences, including the column(s)
 to sort by.

**defaultSort**: An optional default sorting expression to use when no `SortBy` is defined in the
 `pageOptions`.

**thenBySort**: Optional additional sorting expressions to be applied in sequence after the default sorting.

**Returns**: An `System.Linq.IQueryable<T>` that is sorted based on the specified `pageOptions`,
 or using the provided `defaultSort` and `thenBySort` parameters if
 `SortBy` is not specified.

---


### System.Linq.IQueryable<T>.ApplyPaging(Grad.CsLib.Data.PageOptions,System.Linq.Expressions.Expression<Func<T,object>>,System.Linq.Expressions.Expression<Func<T,object>>[])

**Method**: `Grad.CsLib.Data.QueryableExtensions.ApplyPaging(System.Linq.IQueryable<T>,Grad.CsLib.Data.PageOptions,System.Linq.Expressions.Expression<Func<T,object>>,System.Linq.Expressions.Expression<Func<T,object>>[])`

Applies paging to the provided `System.Linq.IQueryable<T>` source based on the given page options.
 If the page options are null, the source is returned as is. Otherwise, the results are paginated
 according to the specified offset and page size.

**pageOptions**: An instance of `Grad.CsLib.Data.PageOptions` containing the offset and page size for pagination.

**defaultSort**: An optional default sort expression to apply if sorting is not explicitly specified in the page options.

**thenBySort**: Additional sort expressions to apply for secondary sorting, in the order they are provided, if applicable.

**Returns**: A `System.Linq.IQueryable<T>` that contains the paginated result based on the provided page options.

---
