using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingDuplicateIndexException : Exception 
    {
        public SterlingDuplicateIndexException(string indexName, Type type, string databaseName) : 
        base (string.Format(Exceptions.SterlingDuplicateIndexException, indexName, type.FullName, databaseName))
        {
            
        }        
    }
}
