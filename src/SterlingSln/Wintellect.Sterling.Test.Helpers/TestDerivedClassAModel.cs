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
    public class TestDerivedClassAModel : TestBaseClassModel
    {
        public String PropertyA { get; set; }

        public TestDerivedClassAModel()
            : base()
        {
            PropertyA = "Property A value";
        }
    }
}
