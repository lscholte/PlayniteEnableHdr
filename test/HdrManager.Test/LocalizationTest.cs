using NUnit.Framework;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HdrManager.Test
{
    [TestFixture]
    internal class LocalizationTest
    {
        private const string _englishLocale = "en_US";
        private const string _localizationDirectory = "Localization";

        private ResourceDictionary _englishResources;

        [SetUp]
        public void Setup()
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
            ResourceDictionary localizedResources = LoadLocalizedResources(locale);
            Assert.That(localizedResources.Keys, Is.EquivalentTo(_englishResources.Keys));
        }

        private static ResourceDictionary LoadLocalizedResources(string locale)
        {
            var localizationFilePath = Path.Combine(_localizationDirectory, $"{locale}.xaml");
            Assert.That(File.Exists(localizationFilePath), $"File not found: {localizationFilePath}");

            using Stream stream = File.OpenRead(localizationFilePath);
            return (ResourceDictionary)XamlReader.Load(stream);
        }

        private static IEnumerable<string> GetLocales()
        {
            return Directory
                .EnumerateFiles(_localizationDirectory, "*.xaml")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(locale => !string.Equals(locale, _englishLocale));
        }
    }
}
