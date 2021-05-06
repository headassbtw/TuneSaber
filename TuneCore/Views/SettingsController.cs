using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuneSaber.Views
{
    internal class SettingsController : PersistentSingleton<SettingsController>
    {
        [UIValue("playbackControllerEn")]
        public bool pbcEn
        {
            get => Configuration.PluginConfig.Instance.playbackControllerEnabled;
            set
            {
                Configuration.PluginConfig.Instance.playbackControllerEnabled = value;
            }
        }

    }
}
