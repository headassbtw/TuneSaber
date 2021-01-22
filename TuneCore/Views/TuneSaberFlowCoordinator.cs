using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HMUI;
using TuneSaber.ViewControllers;

namespace TuneSaber.Views
{
    internal class TuneSaberFlowCoordinator : FlowCoordinator
    {
        private PlaylistViewController _playlistViewController;
        private TwitchViewController _twitchViewController;
        public void Awake()
        {
            if (!_playlistViewController)
                _playlistViewController = BeatSaberUI.CreateViewController<PlaylistViewController>();
            if (!_twitchViewController)
                _twitchViewController = BeatSaberUI.CreateViewController<TwitchViewController>();
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (firstActivation)
                {
                    SetTitle("Playlists");
                    showBackButton = true;
                    ProvideInitialViewControllers(_playlistViewController, null, _twitchViewController);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
