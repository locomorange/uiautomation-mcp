using System;

namespace UIAutomationMCP.Core.Attributes
{
    /// <summary>
    /// Marks an operation class for automatic registration by the source generator
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AutoRegisterOperationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the AutoRegisterOperationAttribute
        /// </summary>
        public AutoRegisterOperationAttribute()
        {
        }
    }
}