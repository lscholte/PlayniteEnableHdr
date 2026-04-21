using System.Threading.Tasks;

namespace HdrManager
{
    public interface IPluginSettingsStore
    {
        Task<IPluginSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(IPluginSettings settings);
    }
}
