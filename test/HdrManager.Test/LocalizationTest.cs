using NUnit.Framework;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Collections.Generic;
using System;

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
            _englishResources = LoadLocalizatedResources(_englishLocale);
        }

        [Test]
        public void EnglishHasStrings()
        {
            Assert.That(_englishResources, Has.Count.AtLeast(1));
        }

        [TestCaseSource(nameof(GetLocales))]
        public void AllEnglishKeysExistInLocale(string locale)
        {
            ResourceDictionary localizedResources = LoadLocalizatedResources(locale);
            using (Assert.EnterMultipleScope())
            {
                foreach (var key in _englishResources.Keys)
                {
                    Assert.That(localizedResources, Does.ContainKey(key));
                }
            }
        }

        private static ResourceDictionary LoadLocalizatedResources(string locale)
        {
            var localizationFilePath = Path.Combine(_localizationDirectory, $"{locale}.xaml");
            Assert.That(File.Exists(localizationFilePath), $"File not found: {localizationFilePath}");

            using Stream stream = File.OpenRead(localizationFilePath);
            return (ResourceDictionary)XamlReader.Load(stream);
        }

        private static IEnumerable<string> GetLocales()
        {
            foreach (var file in Directory.EnumerateFiles(_localizationDirectory, "*.xaml"))
            {
                string locale = Path.GetFileNameWithoutExtension(file);
                if (string.Equals(locale, _englishLocale))
                {
                    continue;
                }

                yield return locale;
            }
        }
    }
}
