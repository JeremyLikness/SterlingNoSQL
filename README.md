# Sterling NoSQL Object-Oriented Database 

This is the project for Sterling, and object-oriented database Jeremy Likness created to meet the growing demand for queryable, persistent storage in Silverlight and Windows Phone apps in the early two-thousand tens.

![Sterling Database](./Sterling-Final-Small.png) 

This project was migrated from CodePlex. You can read the legacy documentation [here](./docs/Home.md).

If you want to browse the code, the "heart" or "brains" of Sterling are contained in [SerializationHelper.cs](./src/SterlingSln/Wintellect.Sterling/Serialization/SerializationHelper.cs). The rest of the codebase can be inferred from there. 

At the time, Sterling was designed to be an unobtrusive solution for taking existing C# classes and persisting them without having to modify the classes themselves. A common use case for this was "tombstoning" Windows phone apps, which referred to saving the View Model that contained the app state when the app is swapped to the background so it can be restored later.

A few features of Sterling: 

* It could serialize almost any class "as is" 
* For special cases you could define your own serializers 
* It supported "byte interceptors" to modify the raw streams (i.e. for compression or encryption, etc.) 
* It supported multiple databases 
* You could define triggers to be called as data was saved (for validation) or loaded (for computed fields, etc.)
* Lambda expressions defined indexes (i.e. index on name: entity => entity.name) 
* The storage was decoupled from the database engine, so you could write your own persistence or leverage the built-in engines including a server side storage engine that used the file system, an isolated storage mechanism for phones and an in-memory default engine for cache

Sterling was later ported to [support WinRT apps](https://github.com/Wintellect/SterlingDB).

The original [online user guide](https://sites.google.com/site/sterlingdatabase/) still exists. 

For nostalgia, you can still browse [old Twitter conversations about Sterling](https://twitter.com/search?q=%23sterlingdb). 
