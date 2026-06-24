namespace Timespan.GUI.Generators.Attributes;

using System;
using System.Collections.Generic;
using System.Text;

internal static class NamingHelpers {
	/// <summary>
	/// Converts a private field name to a PascalCase public property name.
	/// Strips a leading underscore or lowercase prefix before the first uppercase letter.
	///
	/// Examples:
	///   color1ButtonSelected  -> Color1ButtonSelected
	///   _color1ButtonSelected -> Color1ButtonSelected
	///   isActive              -> IsActive
	/// </summary>
	public static string ToPascalCase(string fieldName) {
		if (string.IsNullOrEmpty(fieldName))
			return fieldName;

		// Strip leading underscore(s)
		int start = 0;
		while (start < fieldName.Length && fieldName[start] == '_')
			start++;

		if (start >= fieldName.Length)
			return fieldName;

		// Uppercase the first real character
		return char.ToUpperInvariant(fieldName[start]) + fieldName.Substring(start + 1);
	}

	/// <summary>Returns the property name + "Property" suffix used for the static field.</summary>
	public static string ToStaticFieldName(string propertyName) => propertyName + "Property";
}
