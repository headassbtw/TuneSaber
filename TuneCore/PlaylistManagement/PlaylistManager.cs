using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.XR;
using BS_Utils.Utilities;
using IPA.Config.Data;


namespace TuneSaber.ViewControllers
{


    public class PlaylistViewController : BSMLResourceViewController
    {
        private bool PlaylistJustCreated = false;
        public override string ResourceName => "TuneSaber.PlaylistManagement.PlaylistManager.bsml";
        List<SpotifyAPI.Web.SimplePlaylist> pog = new List<SpotifyAPI.Web.SimplePlaylist>();
        [UIComponent("PlaylistList")] public CustomListTableData customListTableData;

        [UIValue("cpl-name")] public string CPlName = "";
        [UIComponent("cname-setting")] public BeatSaberMarkupLanguage.Components.Settings.StringSetting CNameSetting;
        [UIValue("cpl-desc")] public string CPLDesc = "";
        [UIComponent("cdesc-setting")] public BeatSaberMarkupLanguage.Components.Settings.StringSetting CDescSetting;
        [UIValue("cpublic-bool")] public bool CPublic = false;
        [UIValue("ccollab-bool")] public bool CCollab = false;
        [UIValue("epl-name")] public string EPlName = "";
        [UIComponent("ename-setting")] public BeatSaberMarkupLanguage.Components.Settings.StringSetting ENameSetting;
        [UIValue("epl-desc")] public string EPLDesc = "";
        [UIComponent("edesc-setting")] public BeatSaberMarkupLanguage.Components.Settings.StringSetting EDescSetting;
        [UIValue("epublic-bool")] public bool EPublic = false;
        [UIValue("ecollab-bool")] public bool ECollab = false;

        [UIComponent("refresh-image")] Image RefreshImage = null!;
        [UIComponent("delete-image")] Image DeleteImage = null!;
        [UIComponent("edit-image")] Image EditImage = null!;

        [UIComponent("edit-button")] private Button EditButton;
        [UIComponent("delete-button")] private Button DeleteButton;


        [UIComponent("playlist-image")] private Image PlaylistImage = null!;
        public string ImageURL = "";
        [UIComponent("playlist-bg")] public VerticalLayoutGroup PlaylistBG;
        [UIValue("playlist-bg-color")] public Color PLBGColor;

        


        [UIAction("playlistSelect")]
        public void Select(TableView _, int row)
        {
            Configuration.PluginConfig.Instance.ChosenPlaylist = row;
            try
            {
                Configuration.PluginConfig.Instance.PlaylistID = Core.Spotify.Interaction.playlists.ElementAt(row).Id.ToString();
            }
            catch (Exception e) { }

            var localPl = Core.Spotify.Interaction.playlists.ElementAt(row);
            if(localPl.Images.Count > 0)
            {
                PlaylistImage.SetImage(localPl.Images.ElementAt(0).Url.ToString());
            }
            else
            {
                PlaylistImage.SetImage("");
            }
            ENameSetting.text.text = Core.Spotify.Interaction.playlists.ElementAt(row).Name;
            ENameSetting.Text = Core.Spotify.Interaction.playlists.ElementAt(row).Name;
            ENameSetting.associatedValue.SetValue(Core.Spotify.Interaction.playlists.ElementAt(row).Name);
            EPlName = ENameSetting.associatedValue.GetValue().ToString();
            EDescSetting.text.text = Core.Spotify.Interaction.playlists.ElementAt(row).Description;
            EDescSetting.Text = Core.Spotify.Interaction.playlists.ElementAt(row).Description;
            EDescSetting.associatedValue.SetValue(Core.Spotify.Interaction.playlists.ElementAt(row).Description);
            EPLDesc = EDescSetting.associatedValue.GetValue().ToString();
            ECollab = Core.Spotify.Interaction.playlists.ElementAt(row).Collaborative;
            EPublic = Core.Spotify.Interaction.playlists.ElementAt(row).Public.Value;
            Core.Spotify.Interaction.CurrentPlaylist = Core.Spotify.Interaction.playlists.ElementAt(row);
            Plugin.Log.Notice("Cached playlist " + localPl.Name);
            switch (Core.Spotify.Interaction.CheckPlaylistAddAbility())
            {
                case true:
                    EditButton.interactable = true;
                    EditButton.enabled = true;
                    EditImage.color = Color.white;
                    DeleteButton.interactable = true;
                    DeleteButton.enabled = true;
                    DeleteImage.color = Color.red;
                    SearchMenu.instance.AddHoverHint = "Add selected song to playlist\n(but not remove it)";
                    SearchMenu.instance.AddPLButton.color = Color.white;
                    break;
                case false:
                    EditButton.interactable = false;
                    EditButton.enabled = false;
                    EditImage.color = Color.gray;
                    DeleteButton.interactable = false;
                    DeleteButton.enabled = false;
                    DeleteImage.color = Color.gray;
                    SearchMenu.instance.AddHoverHint = "You do not own the current playlist\nand cannot edit it";
                    SearchMenu.instance.AddPLButton.color = Color.red;
                    break;
            }
            Core.Spotify.Interaction.CheckPlaylistAddAbility();
        }
        

        [UIAction("GetEvent")]
        public void GetEvent()
        {
            Plugin.Log.Debug("Button Pushed");

            Plugin.Log.Debug("SetupList Ran");
            customListTableData.data.Clear();
            SetupList();

            

        }

        [UIAction("get-playlists")]
        public void ShitfuckBecauseThisIsAOneTimeFunctionThatIWillNotRunAnyOtherPlaceAndWouldBeQuiteStupidToPutAProperNameOnAndWouldBeAWasteOfTimeBecauseItIsOneTimeUse()
        {
            SetupList();
        }

        [UIAction("#post-parse")]
        public void PostParse()
        {
            DeleteImage.SetImage("TuneSaber.Icons.Delete.png");
            EditImage.SetImage("TuneSaber.Icons.Edit.png");
            SetupList();
        }

        public void SetupList()
        {
            Core.Spotify.Interaction.GetPlaylists(99);
            customListTableData.data.Clear();

            
            int plcount = Core.Spotify.Interaction.playlists.Count;
            Plugin.Log.Debug(plcount.ToString() + " playlists counted");
            
            int i = 0;
            foreach(var pl in Core.Spotify.Interaction.playlists)
            {
                Texture2D tx = Texture2D.blackTexture;
                //Texture2D tx = Core.Spotify.Interaction.playlist_textures.ElementAt(i);
                var sp = Sprite.Create(tx, new Rect(0.0f, 0.0f, tx.width, tx.height), new Vector2(0.5f, 0.5f), 100.0f);
                Plugin.Log.Info("yep");
                string subtext = pl.Description;
                var customCellInfo = new CustomListTableData.CustomCellInfo(pl.Name, subtext, sp);
                customListTableData.data.Add(customCellInfo);
                i = i + 1;
            }
            i = 0;
            customListTableData.tableView.ReloadData();
            int selectedPlaylist = 0;
            switch (PlaylistJustCreated)
            {
                case true:
                    selectedPlaylist = 0;
                    PlaylistJustCreated = false;
                    break;
                case false:
                    selectedPlaylist = Configuration.PluginConfig.Instance.ChosenPlaylist;
                    break;
            }
             
            //PlaylistImage.SetImage(Core.Spotify.Interaction.playlists.ElementAt(selectedPlaylist).Images.ElementAt(0).Url.ToString());
            customListTableData.tableView.SelectCellWithIdx(selectedPlaylist);
            if (!customListTableData.tableView.visibleCells.Where(x => x.selected).Any())
                customListTableData.tableView.ScrollToCellWithIdx(selectedPlaylist, TableView.ScrollPositionType.Beginning, true);
            
            Select(customListTableData.tableView, selectedPlaylist);
        }

        [UIAction("delete-playlist")]
        public async Task Delet()
        {
            await Core.Spotify.Interaction.EditPlaylist(Configuration.PluginConfig.Instance.PlaylistID, "to be deleted", false, false, "i hate the API too");
            await Core.Spotify.Interaction.UnfollowPlaylist(Configuration.PluginConfig.Instance.PlaylistID);
            await GetPlaylists(99);
            SetupList();
        }

        [UIAction("editPlaylist")]
        public async Task EditPlaylist()
        {
            await Core.Spotify.Interaction.EditPlaylist(Configuration.PluginConfig.Instance.PlaylistID, ENameSetting.text.text, EPublic, ECollab, EDescSetting.text.text);
            await GetPlaylists(99);
            SetupList();
        }
        [UIAction("createPlaylist")]
        public async Task CreatePlaylist()
        {
            await Core.Spotify.Interaction.CreatePlaylist(CNameSetting.text.text, CPublic, CCollab, CDescSetting.text.text);
            await GetPlaylists(99);
            PlaylistJustCreated = true;
            SetupList();
        }
        public async Task GetPlaylists(int max)
        {
            pog = await Core.Spotify.Interaction.GetPlaylists(max);
            Plugin.Log.Debug("Found " + pog.Count + " Playlists, the first is " + pog.ElementAt(0).Name + ".");
        }
        
    }
}
