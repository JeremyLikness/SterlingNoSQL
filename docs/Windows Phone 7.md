# Windows Phone 7 

Read the [Related Blog Post](http://csharperimage.jeremylikness.com/2010/12/using-sterling-in-windows-phone-7.html)

To use Sterling with Windows Phone 7, refer to the _WindowsPhoneSterlingSln.Sln_ project. This is a full example of Sterling with Windows Phone 7.

## Step One: Create Your Database

{code:C#}
namespace WindowsPhoneSterling.Sterling
{
    public class ItemDatabase : BaseDatabaseInstance 
    {

        public override string Name
        {
            get { return "ItemDatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> _RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<ItemViewModel,int>(i=>i.Id)
            };
        }
    }
}
{code:C#}

## Step Two: Wire in the App.Xaml.cs Support (Tombstoning/etc)

Add references for the database engine, the database, and the logger.

{code:C#}
private static ISterlingDatabaseInstance _database = null;
private static SterlingEngine _engine = null;
private static SterlingDefaultLogger _logger = null;
{code:C#}

Provide a public property to access the database:

{code:C#}
public static ISterlingDatabaseInstance Database
{
    get
    {
        return _database;
    }
}
{code:C#}

Create methods to activate the engine (when the application starts or returns from deactivation) and deactivation (when the app is being forced to the background). It is recommended you restart the engine and flush/dispose it to provide full support for tombstoning - this ensures all data is flushed to isolated storage and available again when the application wakes back up.

{code: C#}
private void _ActivateEngine()
{
    _engine = new SterlingEngine();
    _logger = new SterlingDefaultLogger(SterlingLogLevel.Information);
    _engine.Activate();
    _database = _engine.SterlingDatabase.RegisterDatabase<ItemDatabase>();
}

private void _DeactivateEngine()
{
    _logger.Detach();
    _engine.Dispose(); // this flushes to isolated storage
    _database = null;
    _engine = null;
}
{code: C#}

Call the activation method in Application_Launching and Application_Activated. Call the deactivate methods on Application_Deactivated and _Application_Closing.

## Step Three: Save Data to Sterling

Call the _Save_ method on the database to save an entity you defined for your database.

{code: C#}
private void _SetupData()
{
    var sampleData = new List<ItemViewModel>()
    {
        new ItemViewModel() { LineOne = "runtime one", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu" },
        new ItemViewModel() { LineOne = "runtime two", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus" },
        new ItemViewModel() { LineOne = "runtime three", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent" },
        new ItemViewModel() { LineOne = "runtime four", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos" },
        new ItemViewModel() { LineOne = "runtime five", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur" },
        new ItemViewModel() { LineOne = "runtime six", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent" },
        new ItemViewModel() { LineOne = "runtime seven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat" },
        new ItemViewModel() { LineOne = "runtime eight", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum" },
        new ItemViewModel() { LineOne = "runtime nine", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu" },
        new ItemViewModel() { LineOne = "runtime ten", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus" },
        new ItemViewModel() { LineOne = "runtime eleven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent" },
        new ItemViewModel() { LineOne = "runtime twelve", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos" },
        new ItemViewModel() { LineOne = "runtime thirteen", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur" },
        new ItemViewModel() { LineOne = "runtime fourteen", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent" },
        new ItemViewModel() { LineOne = "runtime fifteen", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat" },
        new ItemViewModel() { LineOne = "runtime sixteen", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum" }
    };

    var idx = 0;

    foreach (var item in sampleData)
    {
        idx++;
        item.Id = idx;
        App.Database.Save(item);                
    }            
}
{code: C#}

## Step Four: Load Data

This example queries the keys for the view model. For each one, the _LazyValue_ is accessed. The entity will not be fully deserialized unless you access the lazy value or explicitly call load:

{code: C#}
foreach (var item in App.Database.Query<ItemViewModel, int>())
{
    Items.Add(item.LazyValue.Value);
}
{code: C#}