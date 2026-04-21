using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace HdrManager.Test
{
    [TestFixture]
    internal class LocalizationTest
    {
        private const string _englishLocale = "en_US";
        private const string _localizationDirectory = "Localization";

        private readonly HashSet<string> _englishResources;

        public LocalizationTest()
        {
            _englishResources = LoadLocalizedResources(_englishLocale);
        }

        [Test]
        public void EnglishHasStrings()
        {
            Assert.That(_englishResources, Has.Count.AtLeast(1));
        }

        [TestCaseSource(nameof(GetLocales))]
        public void AllEnglishKeysExistInLocale(string locale)
        {
            var localizedResources = LoadLocalizedResources(locale);
            Assert.That(localizedResources, Is.EquivalentTo(_englishResources));
        }

        private static HashSet<string> LoadLocalizedResources(string locale)
        {
            var localizationFilePath = Path.Combine(_localizationDirectory, $"{locale}.ftl");
            Assert.That(File.Exists(localizationFilePath), $"File not found: {localizationFilePath}");

            var content = File.ReadAllText(localizationFilePath);
            var lines = content.Replace("\r\n", "\n").Split('\n');
            var messageRegex = new System.Text.RegularExpressions.Regex("^\\s*([A-Za-z0-9_][A-Za-z0-9_-]*)\\s*=\\s*(.*)$");
            var keys = new HashSet<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.TrimStart();
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
                keys.Add(id);
            }

            return keys;
        }

        private static IEnumerable<string> GetLocales()
        {
            return Directory
                .EnumerateFiles(_localizationDirectory, "*.ftl")
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .Where(locale => !string.Equals(locale, _englishLocale));
        }
    }
}
