using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.FloatingScreen;

namespace TuneSaber.Views
{
    internal class UICreator
    {
        public static TuneSaberFlowCoordinator TuneSaberFlowCoordinator;
        public static bool Created;

        public static void CreateMenu()
        {
            if (!Created)
            {
                MenuButton menuButton = new MenuButton("TuneSaber", "Manage playlists for TuneSaber", ShowFlow);
                MenuButtons.instance.RegisterButton(menuButton);
                Created = true;
            }
        }
        

        public static void ShowFlow()
        {
            if (TuneSaberFlowCoordinator == null)
                TuneSaberFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<TuneSaberFlowCoordinator>();
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(TuneSaberFlowCoordinator);
        }
    }
}
