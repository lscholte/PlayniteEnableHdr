using NUnit.Framework;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace HdrManager.Test
{
    [TestFixture]
    internal class LocalizationTest
    {
        private ResourceDictionary _englishResources;

        [SetUp]
        public void Setup()
        {
            _englishResources = LoadLocalizatedResources("en_US");
        }

        [Test]
        public void EnglishHasStrings()
        {
            Assert.That(_englishResources, Has.Count.AtLeast(1));
        }

        [TestCase("es_ES")]
        [TestCase("fr_FR")]
        [TestCase("pt_PT")]
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

        private ResourceDictionary LoadLocalizatedResources(string locale)
        {
            var path = Path.GetFullPath($"Localization/{locale}.xaml");
            Assert.That(File.Exists(path), $"File not found: {path}");

            using (var stream = File.OpenRead(path))
            {
                return (ResourceDictionary)XamlReader.Load(stream);
            }
        }
    }
}
