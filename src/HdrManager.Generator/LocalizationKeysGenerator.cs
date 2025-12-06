using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
            var xamlFile = context
                .AdditionalFiles
                .FirstOrDefault(f => f.Path.EndsWith("en_US.xaml"));

            if (xamlFile == null)
            {
                return;
            }

            var xamlText = xamlFile.GetText(context.CancellationToken)?.ToString();
            if (string.IsNullOrEmpty(xamlText))
            {
                return;
            }

            // Parse XAML as XML
            var doc = XDocument.Parse(xamlText);

            // Define the XAML namespace for "x"
            XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

            // Collect keys
            var keys = doc
                .Descendants()
                .Where(e => e.Attribute(xNamespace + "Key") != null)
                .Select(e => e.Attribute(xNamespace + "Key")!.Value)
                .ToList();

            string className = "LocalizationKeys";

            var sb = new StringBuilder();
            sb.AppendLine("namespace HdrManager.Localization.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    public static class {className}");
            sb.AppendLine("    {");

            foreach (var key in keys)
            {
                sb.AppendLine($"        public const string {Sanitize(key)} = \"{key}\";");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"{className}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private string Sanitize(string key)
        {
            // Replace invalid identifier chars with underscores
            var sb = new StringBuilder();
            foreach (var c in key)
            {
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }
            return sb.ToString();
        }
    }
}
