# Grad.CsLib.Data

*Generated from Grad.CsLib.Data.xml*


## DatabaseMigrator

**Type**: `Grad.CsLib.Data.DatabaseMigrator`

Handles database migrations using DbUp.

---


### DatabaseMigrator(string,System.Reflection.Assembly)

**Method**: `Grad.CsLib.Data.DatabaseMigrator.#ctor(string,System.Reflection.Assembly)`

Handles database migrations using DbUp and records the migration history into a schema called `DbUp`, which must exist.

**connectionString**: The connection string to the database.

**assembly**: The assembly containing the SQL scripts to migrate. Defaults to the calling assembly.

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


## DbConnectionFactory

**Type**: `Grad.CsLib.Data.DbConnectionFactory`

A factory for creating and managing SQL database connections asynchronously.

**Remarks**: Register the factory as a singleton in DI:
 
 services.AddSingleton<IDbConnectionFactory>(s => new DbConnectionFactory(builder.Configuration.GetConnectionString("DbName")!));
 
 
 Inject it to your service:
 
 public class MyService(IDbConnectionFactory connectionFactory)
 
 
 And then use it in a method:
 
 await using var connection = await connectionFactory.ConnectAsync();

---


### DbConnectionFactory(string)

**Method**: `Grad.CsLib.Data.DbConnectionFactory.#ctor(string)`

A factory for creating and managing SQL database connections asynchronously.

**Remarks**: Register the factory as a singleton in DI:
 
 services.AddSingleton<IDbConnectionFactory>(s => new DbConnectionFactory(builder.Configuration.GetConnectionString("DbName")!));
 
 
 Inject it to your service:
 
 public class MyService(IDbConnectionFactory connectionFactory)
 
 
 And then use it in a method:
 
 await using var connection = await connectionFactory.ConnectAsync();

---


## IDbConnectionFactory

**Type**: `Grad.CsLib.Data.IDbConnectionFactory`

Defines a factory for creating and managing database connections asynchronously.

---


### ConnectAsync()

**Method**: `Grad.CsLib.Data.IDbConnectionFactory.ConnectAsync`

Creates a new database connection.

**Remarks**: The caller is responsible for closing the connection. The recommended
 pattern is to use `await using` to ensure the connection is closed.

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


## SqlHelpers

**Type**: `Grad.CsLib.Data.SqlHelpers`

Provides utility methods for SQL query construction, sorting, and filtering.

**Remarks**: Only use these in scenarios where the query is very dynamic. Otherwise, it's
 recommended to write complete SQL queries directly for better clarity and maintainability.

---


### BuildOrderBy(T,System.Collections.Generic.Dictionary<string,string>,string,Grad.CsLib.Data.PageOptions)

**Method**: `Grad.CsLib.Data.SqlHelpers.BuildOrderBy(T,System.Collections.Generic.Dictionary<string,string>,string,Grad.CsLib.Data.PageOptions)`

Builds an ORDER BY clause based on a simplified string representation of sorting options,
 typically sent from a web client.

**entity**: EF Core entity type for normalizing columns.

**mapping**: A dictionary mapping property names to entity field names.

**defaultSort**: The default sorting order if no specific sorting is provided.

**pageOptions**: The page options containing sorting information.

**Returns**: The constructed ORDER BY clause.

**Throws T:Grad.CsLib.Data.InvalidParameterException**: Thrown if the sorting parameter is invalid.

---


### BuildPaging(Grad.CsLib.Data.PageOptions,Dapper.DynamicParameters)

**Method**: `Grad.CsLib.Data.SqlHelpers.BuildPaging(Grad.CsLib.Data.PageOptions,Dapper.DynamicParameters)`

Builds a paging clause based on the provided page options.

**pageOptions**: The page options to use for paging.

**parameters**: The dynamic parameters to add paging parameters to.

**Returns**: The SQL paging clause or an empty string if no paging options are provided.

---


### AppendFilters(System.Text.StringBuilder,Dapper.DynamicParameters,System.Collections.Generic.IEnumerable<System.ValueTuple<string,object>>,bool)

**Method**: `Grad.CsLib.Data.SqlHelpers.AppendFilters(System.Text.StringBuilder,Dapper.DynamicParameters,System.Collections.Generic.IEnumerable<System.ValueTuple<string,object>>,bool)`

Appends filter conditions to a SQL WHERE clause based on the specified column-value pairs.
 Handles string, boolean, and collection-based filters dynamically, adding the appropriate SQL syntax.

**where**: The StringBuilder object to which the filter conditions will be appended.

**parameters**: The dynamic parameters object for managing SQL parameterized values.

**filters**: A collection of tuples representing the column names and corresponding filter values.

**booleanNullCheck**: Indicates whether to add IS NULL/IS NOT NULL conditions for boolean values when they are null.
 Defaults to true.

---
