namespace Timespan.GUI.Generators.Attributes;

using System;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class BasicDirectPropertyAttribute<TOwner> : Attribute {
}

