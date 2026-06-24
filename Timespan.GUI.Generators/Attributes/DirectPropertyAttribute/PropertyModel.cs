namespace Timespan.GUI.Generators.Attributes;

using System;

/// <summary>
/// Immutable data bag that describes one field annotated with
/// <c>[BasicDirectProperty&lt;TOwner&gt;]</c>. Kept simple/serialisable so it
/// can cross the incremental pipeline boundary without issues.
/// </summary>
internal sealed record PropertyModel(
	/// <summary>Fully-qualified namespace of the containing class, or null if global.</summary>
	string? Namespace,

	/// <summary>Simple class name (no namespace).</summary>
	string ClassName,

	/// <summary>
	/// Any containing type names for nested classes, outermost first.
	/// Empty when the class is not nested.
	/// </summary>
	string[] ContainingTypeNames,

	/// <summary>Original field name as written by the user, e.g. <c>color1ButtonSelected</c>.</summary>
	string FieldName,

	/// <summary>C# type keyword / name as a string, e.g. <c>bool</c>, <c>string</c>.</summary>
	string FieldType,

	/// <summary>
	/// Source representation of the field's initialiser, or <c>null</c> when there is none.
	/// Used as the <c>defaultBindingValue</c> argument in <c>RegisterDirect</c>.
	/// </summary>
	string? DefaultValueSource
);