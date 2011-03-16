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

namespace Wintellect.Sterling.Test.Helpers
{
    public abstract class TestBaseClassModel
    {
        public Guid Key { get; set; }
        public String BaseProperty { get; set; }

        public TestBaseClassModel()
        {
            Key = Guid.NewGuid();
            BaseProperty = "Base Property Value";
        }
    }
}
