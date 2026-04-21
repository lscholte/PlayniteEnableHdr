using Playnite.Markup;

namespace HdrManager.Markup
{
    public class LocalizedString : LocStringMarkup
    {
        public LocalizedString()
            : base(Plugin.Id)
        {
        }

        public LocalizedString(string stringId)
            : base(Plugin.Id, stringId)
        {
        }
    }
}
