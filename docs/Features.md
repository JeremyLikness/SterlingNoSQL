# Features

* Full [User's Guide](http://www.sterlingdatabase.com/)
* [Saves complex object graphs](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/c-saving-instances), including children of children to any depth, including classes and structs
* Handles both properties and fields
* [Suppress serialization](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/c-saving-instances) by type, property, or field 
* Handles cycle detection and circular references
* Is dirty support using a user-supplied predicate
* In memory database 
* Elevated trust database for Silvelright OOB applications that uses the local file system instead of isolated storage 
* Isolated storage versions for Silverlight and Windows Phone 
* File system version for the server/CLR desktop 
* Table definitions can be added "on the fly" 
* [Easy configuration](http://www.sterlingdatabase.com/sterling-user-guide/getting-started): provide a table type, a key type, and a lambda expression for the key and you're ready to start saving and loading
* Support for interfaces and abstract classes (serializes the implemented type)
* "Save as" to save derived types to the base class
* Full foreign key suport: child objects are saved in separate tables 
* Compact binary serialization leaves smaller footprint on disk than JSON, XML, and other serialization strategies
* [Support for encryption](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/g-triggers-and-interceptors)
* [Support for compression](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/g-triggers-and-interceptors)
* Supports all CRUD operations: [Load](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/d-loading-instances), [Save](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/c-saving-instances) (asynchronous save for lists), [Delete, Truncate, and Full Database Purge](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/f-delete-truncate-and-purge)
* Allows multiple databases per application for partitioning and/or versioning 
* [Automatic type support for](http://www.sterlingdatabase.com/sterling-user-guide/5-serializers): 
	* Primitive Types
	* Nullable Types 
	* Strings
	* Byte arrays
	* DateTime
	* Timespan
	* Guid
	* Enums
	* Decimal
	* Lists
	* Dictionaries 
	* Arrays
	* WriteableBitmap
* Custom type support for any other type using [custom serializers](http://www.sterlingdatabase.com/sterling-user-guide/5-serializers)
* [Full logging support](http://www.sterlingdatabase.com/sterling-user-guide/6-logging)
* [Full key support](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/a-keys) for _any_ type of key including composite keys
* [Full index support](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/b-indexes) using lambda expressions 
* [LINQ to Object queries on indexes and keys](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/e-queries-and-filters)
* Lazy loading from queries
* Built-in [caching](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/e-queries-and-filters)
* [Database backup and restore](http://www.sterlingdatabase.com/sterling-user-guide/3-the-sterling-engine/b-database-backup-and-restore)
* [Support for before save, after save, and delete triggers](http://www.sterlingdatabase.com/sterling-user-guide/4-databases/g-triggers-and-interceptors)
* [Automatic key generation using triggers](http://www.sterlingdatabase.com/sterling-user-guide/7-sterling-recipes/a-key-generation-auto-identity)
* Constraints using triggers
* Automatic update of create and modify dates and dirty flags using triggers 
* Supports Silverlight 4, Silverlight 5, .NET 4.0 (server/desktop) and Windows Phone 7  
* DLL is less than 100KB 