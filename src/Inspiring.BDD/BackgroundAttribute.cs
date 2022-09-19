using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.BDD {
    /// <summary>
    /// Applied to a method to indicate a background for each scenario defined in the same feature class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BackgroundAttribute : Attribute {
    }
}
