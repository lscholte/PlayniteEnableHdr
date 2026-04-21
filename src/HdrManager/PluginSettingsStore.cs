using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HdrManager
{
    public class PluginSettingsStore : IPluginSettingsStore
    {
        private readonly string _settingsFilePath;
        private readonly JsonSerializerOptions _serializationOptions;
        private readonly JsonSerializerOptions _deserializationOptions;

        public PluginSettingsStore(string settingsDirectory)
        {
            _settingsFilePath = Path.Combine(settingsDirectory, "settings.json");
            _serializationOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            _deserializationOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
        }

        public async Task<IPluginSettings> LoadSettingsAsync()
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<PluginSettings>(json, _deserializationOptions);
                return settings ?? new PluginSettings();
            }

            return new PluginSettings();
        }

        public async Task SaveSettingsAsync(IPluginSettings settings)
        {
            string json = JsonSerializer.Serialize(settings, _serializationOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
    }
}
