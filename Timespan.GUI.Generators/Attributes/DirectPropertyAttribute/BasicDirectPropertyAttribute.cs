namespace Timespan.GUI.Generators.Attributes;

using System;
using System.Collections.Generic;
using System.Text;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class BasicDirectPropertyAttribute<TOwner> : Attribute {
}

