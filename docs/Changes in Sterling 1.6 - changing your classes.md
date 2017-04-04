Up until SterlingDB 1.5.0, it was hard to change or update your domain classes. Sterling didn't yet have a mechanism of knowing what had changed. Several solutions were proposed and we moved on from there. As of Sterling 1.6.0, SterlingDB now supports removing properties, adding properties, renaming properties, and renaming classes.

## Backwards compatibility

At the moment (Sterling 1.6), there is no backwards compatibility with Sterling 1.5. Sterling 1.6 will not know how to read a Sterling 1.5 database. If you're starting from scratch with 1.6, you're good to go. We will look into this and see if we can find a solution so that 1.5 databases can be updated to a 1.6 database.

Sterling 1.6.1 is also not backwards compatible with 1.6.0 because for DateTime properties, the Kind is also serialized. If this is a problem for people, we could introduce a flag to determine if the Kind should be serialized/deserialized. But because these two releases were release so closely in time, chances are slim anybody would have released a product with Sterling 1.6.

## Removing properties

SterlingDB will simply ignore a serialized property that it no longer finds in the current class. After saving this instance again, the value will no longer be serialized.

## Adding properties

SterlingDB will fall back to the default value for a property that has been added to a class (e.g. null, 0, string.Empty, etc.).

## Renaming properties

If a property has been renamed, SterlingDB will not find the original property for the serialized value, so it would ignore it and lose the value. You can solve this by registering an ISterlingPropertyConverter:
{{
    public class MyApplicationDatabase: BaseDatabaseInstance
    {
         // Register your tables,...

        protected internal override void RegisterPropertyConverters()
        {
            RegisterPropertyConverter(new CustomerPropertyConverter());
        }
    }
}}
An ISterlingPropertyConverter could look something like this:
{{
    public class CustomerPropertyConverter : ISterlingPropertyConverter
    {
        public Type IsConverterFor()
        {
            return typeof (Customer);
        }

        public void SetValue(object instance, string oldPropertyName, object value)
        {
            if (oldPropertyName == "SecondName" )
            {
                ((Customer)instance).FamilyName = value.ToString();  
            }
        }
    }
}}
Of course, you can make this as complex as you like or need (calling other pieces of code, adding logic to derive a new value for the renamed property, etc.). Just implement the two methods and you're set to go.

## Renaming classes

SterlingDB will serialize the class name of an object. If a class has been renamed, SterlingDB needs a way of knowing the new class name. This can be achieved by registering an ISterlingTypeResolver:
{{
    public class MyApplicationDatabase : BaseDatabaseInstance
    {
         // Register your tables,...

        protected internal override void RegisterTypeResolvers()
        {
            RegisterTypeResolver( new ChangingTypeFirstToSecondVersionResolver ());
        }
    }
}}
Your ISterlingTypeResolver could look like this:
{{
    public class ClientTypeResolver : ISterlingTypeResolver
    {
        public Type ResolveTableType(string fullTypeName)
        {
            if (fullTypeName.Contains("Customer"))
            {
                return typeof(Client);
            }

            return null;
        }
    }
}}
In theory, you could just register one ISterlingTypeResolver and put all your logic for renamed classes in there, but I believe it makes sense to create an ISterlingTypeResolver per class that has been renamed.

## Removing classes

Removing a class will now still cause a SterlingTableNotFoundException. This is a difficult one to tackle, because usually, you would want an exception. Possibly, we could use the type-resolving mechanism here, but that would require further investigation. You could also catch the exception and check if it really is an exception or just a type you've removed.