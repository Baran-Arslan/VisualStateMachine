using System;
using System.Diagnostics;

namespace iCare.Core {
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional("UNITY_EDITOR")]
    public sealed class StateCategory : System.Attribute {
        internal readonly string Category;

        public StateCategory(string category) {
            this.Category = category;
        }
    }
}