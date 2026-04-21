using Moq;
using NUnit.Framework;
using Playnite;

namespace HdrManager.Test
{
    [TestFixture]
    public class PluginServiceProviderFactoryTest
    {
        [Test]
        public void CreateServiceProvider_WithValidApi_ReturnsServiceProvider()
        {
            var mockPlayniteApi = new Mock<IPlayniteApi>();
            mockPlayniteApi
                .SetupGet(api => api.UserDataDir)
                .Returns("/test/data");

            var factory = new PluginServiceProviderFactory();
            var serviceProvider = factory.CreateServiceProvider(mockPlayniteApi.Object);

            Assert.That(serviceProvider, Is.Not.Null);
        }

        [Test]
        public void CreateServiceProvider_WithValidApi_CanResolvePluginSettingsStore()
        {
            var mockPlayniteApi = new Mock<IPlayniteApi>();
            mockPlayniteApi
                .SetupGet(api => api.UserDataDir)
                .Returns("/test/data");

            var factory = new PluginServiceProviderFactory();
            var serviceProvider = factory.CreateServiceProvider(mockPlayniteApi.Object);

            var settingsStore = serviceProvider.GetService(typeof(IPluginSettingsStore));
            Assert.That(settingsStore, Is.Not.Null);
            Assert.That(settingsStore, Is.InstanceOf<IPluginSettingsStore>());
        }

        [Test]
        public void CreateServiceProvider_WithValidApi_CanResolveSystemHdrManager()
        {
            var mockPlayniteApi = new Mock<IPlayniteApi>();
            mockPlayniteApi
                .SetupGet(api => api.UserDataDir)
                .Returns("/test/data");

            var factory = new PluginServiceProviderFactory();
            var serviceProvider = factory.CreateServiceProvider(mockPlayniteApi.Object);

            var systemHdrManager = serviceProvider.GetService(typeof(ISystemHdrManager));
            Assert.That(systemHdrManager, Is.Not.Null);
            Assert.That(systemHdrManager, Is.InstanceOf<ISystemHdrManager>());
        }

        [Test]
        public void CreateServiceProvider_WithNullApi_ThrowsArgumentNullException()
        {
            var factory = new PluginServiceProviderFactory();
            Assert.That(
                () => factory.CreateServiceProvider(null!),
                Throws.ArgumentNullException);
        }

        [Test]
        public void CreateServiceProvider_RegisteredServicesAreSingletons()
        {
            var mockPlayniteApi = new Mock<IPlayniteApi>();
            mockPlayniteApi
                .SetupGet(api => api.UserDataDir)
                .Returns("/test/data");

            var factory = new PluginServiceProviderFactory();
            var serviceProvider = factory.CreateServiceProvider(mockPlayniteApi.Object);

            var settingsStore1 = serviceProvider.GetService(typeof(IPluginSettingsStore));
            var settingsStore2 = serviceProvider.GetService(typeof(IPluginSettingsStore));

            Assert.That(settingsStore1, Is.SameAs(settingsStore2));
        }
    }
}
