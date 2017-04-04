# Note on Null Indexes

Sterling currently doesn't support null index values. That doesn't mean it can't handle fields that are nullable, only the serialization of null indexes. For an explanation, read [this thread](http://sterling.codeplex.com/Thread/View.aspx?ThreadId=240351). Be extra careful with this for string types - if you aren't initializing them to string.Empty then you'll need to cast them to that in the index definition as per below.

The solution is simply to cast to a non-nullable type. For example, with this class:

{code:c#}
public class MyTable 
{
   public int Id { get; set; }
   public int? NullableField { get; set; }
   public string Name { get; set; } // could be null
}
{code:c#}

You would simply define the index like this:

{code:c#}
.WithIndex<MyTable,int,int>("Index_NullableField", m=>m.NullableField ?? -1)
.WithIndex<MyStable,string,int>("Index_Name", m=>m.Name ?? string.Empty)
{code:c#}

(or some other acceptable default value)