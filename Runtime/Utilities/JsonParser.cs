using System;
using System.Text;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Lightweight JSON field extraction without external parser dependencies.
	/// Extracts top-level field values from JSON strings.
	/// </summary>
	public static class JsonParser
	{
		/// <summary>
		/// Extracts the value of a top-level string field, with escape sequence handling.
		/// Returns null if the key is not found.
		/// </summary>
		public static string ExtractString(string json, string key)
		{
			var search = $"\"{key}\":\"";
			var start = json.IndexOf(search, StringComparison.Ordinal);
			if (start < 0) return null;

			start += search.Length;

			var sb = new StringBuilder();
			for (var i = start; i < json.Length; i++)
			{
				if (json[i] == '\\' && i + 1 < json.Length)
				{
					sb.Append(json[i + 1]);
					i++;
					continue;
				}

				if (json[i] == '"') break;
				sb.Append(json[i]);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Extracts the raw value of a top-level JSON field (object, array, string, or primitive).
		/// Returns the raw JSON substring including quotes for strings, braces for objects, etc.
		/// Returns null if the key is not found.
		/// </summary>
		public static string ExtractValue(string json, string key)
		{
			var search = $"\"{key}\":";
			var start = json.IndexOf(search, StringComparison.Ordinal);
			if (start < 0) return null;

			start += search.Length;

			// Skip whitespace
			while (start < json.Length && char.IsWhiteSpace(json[start])) start++;
			if (start >= json.Length) return null;

			var ch = json[start];

			// Object or array — find matching brace/bracket
			if (ch == '{' || ch == '[')
			{
				var open = ch;
				var close = ch == '{' ? '}' : ']';
				var depth = 1;
				var pos = start + 1;
				var inString = false;

				while (pos < json.Length && depth > 0)
				{
					var c = json[pos];

					if (inString)
					{
						if (c == '\\') { pos++; }
						else if (c == '"') { inString = false; }
					}
					else
					{
						if (c == '"') { inString = true; }
						else if (c == open) { depth++; }
						else if (c == close) { depth--; }
					}

					pos++;
				}

				return json.Substring(start, pos - start);
			}

			// String value
			if (ch == '"')
			{
				var end = start + 1;
				while (end < json.Length)
				{
					if (json[end] == '\\') { end += 2; continue; }
					if (json[end] == '"') break;
					end++;
				}

				return json.Substring(start, end - start + 1);
			}

			// Primitive (number, bool, null)
			{
				var end = start;
				while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']' && !char.IsWhiteSpace(json[end]))
				{
					end++;
				}

				return json.Substring(start, end - start);
			}
		}

		/// <summary>
		/// Escapes a string for safe inclusion in a JSON value.
		/// </summary>
		public static string Escape(string value)
		{
			if (string.IsNullOrEmpty(value)) return value;

			return value
				.Replace("\\", "\\\\")
				.Replace("\"", "\\\"")
				.Replace("\n", "\\n")
				.Replace("\r", "\\r")
				.Replace("\t", "\\t");
		}
	}
}
