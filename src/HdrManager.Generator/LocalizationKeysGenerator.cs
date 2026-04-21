using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace HdrManager.Generator
{
    [Generator]
    public class LocalizationKeysGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var ftlFile = context
                .AdditionalFiles
                .FirstOrDefault(f => f.Path.EndsWith("en_US.ftl"));

            if (ftlFile == null)
            {
                return;
            }

            var ftlText = ftlFile.GetText(context.CancellationToken)?.ToString();
            if (string.IsNullOrEmpty(ftlText))
            {
                return;
            }

            // Parse Fluent (.ftl) file to extract top-level message ids and their values.
            // A simple parser: lines matching `id = value` (excluding terms starting with '-')
            var keys = new List<string>();
            var values = new Dictionary<string, string>();

            var ftl = ftlText ?? string.Empty;
            var lines = ftl.Replace("\r\n", "\n").Split('\n');
            var messageRegex = new Regex("^\\s*([A-Za-z0-9_][A-Za-z0-9_-]*)\\s*=\\s*(.*)$");
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.TrimStart();
                // Skip comments and terms (terms start with '-')
                if (trimmed.Length == 0 || trimmed.StartsWith("#") || trimmed.StartsWith("-"))
                {
                    continue;
                }

                var m = messageRegex.Match(line);
                if (!m.Success)
                {
                    continue;
                }

                var id = m.Groups[1].Value;
                var valueBuilder = new StringBuilder();
                var firstValue = m.Groups[2].Value.TrimEnd();
                valueBuilder.Append(firstValue);

                // Consume following indented continuation lines as part of the value
                int j = i + 1;
                while (j < lines.Length && lines[j].Length > 0 && char.IsWhiteSpace(lines[j][0]))
                {
                    // append the line without leading indentation
                    valueBuilder.Append('\n');
                    valueBuilder.Append(lines[j].TrimStart());
                    j++;
                }

                i = j - 1;

                keys.Add(id);
                values[id] = valueBuilder.ToString().Trim();
            }

            string className = "LocalizationKeys";

            var sb = new StringBuilder();
            sb.AppendLine("namespace HdrManager.Localization.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Provides constants for all localized string keys.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public static class {className}");
            sb.AppendLine("    {");

            foreach (var key in keys)
            {
                values.TryGetValue(key, out var rawValue);
                rawValue = (rawValue ?? string.Empty).Trim();

                // Escape XML special chars for documentation comment
                var escaped = rawValue.Replace("&", "&amp;")
                                      .Replace("<", "&lt;")
                                      .Replace(">", "&gt;");

                // Add XML documentation comment showing the localized value
                sb.AppendLine("        /// <summary>");
                if (string.IsNullOrEmpty(escaped))
                {
                    sb.AppendLine($"        /// {key}");
                }
                else
                {
                    var docLines = escaped.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
                    foreach (var line in docLines)
                    {
                        sb.AppendLine($"        /// {line}");
                    }
                }
                sb.AppendLine("        /// </summary>");

                sb.AppendLine($"        public const string {Sanitize(key)} = \"{key}\";");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"{className}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private string Sanitize(string key)
        {
            // Convert to PascalCase and ensure a valid identifier.
            // Split on any non-alphanumeric character, capitalize each part and join.
            var parts = Regex.Split(key ?? string.Empty, "[^A-Za-z0-9]+");
            var sb = new StringBuilder();

            // Known overrides for acronym-like tokens so generated identifiers keep expected casing
            // Add more entries here as needed (lowercase -> desired casing)
            var overrides = new Dictionary<string, string>
            {
                { "pc", "PC" },
                { "pcgamingwiki", "PCGamingWiki" }
            };

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                var lower = part.ToLowerInvariant();
                if (overrides.TryGetValue(lower, out var mapped))
                {
                    sb.Append(mapped);
                    continue;
                }

                // Uppercase first char, keep remainder as-is
                var first = part[0];
                sb.Append(char.ToUpperInvariant(first));
                if (part.Length > 1)
                {
                    sb.Append(part, 1, part.Length - 1);
                }
            }

            var result = sb.ToString();
            if (string.IsNullOrEmpty(result))
            {
                return "_";
            }

            // If it starts with a digit, prefix with underscore to make it a valid identifier
            if (!char.IsLetter(result[0]) && result[0] != '_')
            {
                result = "_" + result;
            }

            // Replace any remaining invalid characters with underscores (defensive)
            var finalSb = new StringBuilder();
            foreach (var c in result)
            {
                finalSb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            return finalSb.ToString();
        }
    }
}
