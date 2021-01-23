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


    class PlaylistViewController : BSMLResourceViewController
    {
        public override string ResourceName => "TuneSaber.PlaylistManagement.PlaylistManager.bsml";
        List<SpotifyAPI.Web.SimplePlaylist> pog = new List<SpotifyAPI.Web.SimplePlaylist>();
        [UIComponent("PlaylistList")] public CustomListTableData customListTableData;

        [UIComponent("playlist-image")] private Image PlaylistImage = null!;
        public string PLName = "";
        public string ImageURL = "";
        [UIComponent("playlist-bg")] public VerticalLayoutGroup PlaylistBG;
        [UIValue("playlist-bg-color")] public Color PLBGColor;


        [UIAction("playlistSelect")]
        private void Select(TableView _, int row)
        {
            Configuration.PluginConfig.Instance.ChosenPlaylist = row;
            Configuration.PluginConfig.Instance.PlaylistID = Core.Spotify.Interaction.playlists.ElementAt(row).Id.ToString();

            var localPl = Core.Spotify.Interaction.playlists.ElementAt(row);
            ImageURL = localPl.Images.ElementAt(0).Url.ToString();
            //SearchMenu.instance.PlaylistImage.SetImage(ImageURL);
            //SearchMenu.instance.PLName.text = PLName;
            Plugin.Log.Notice("Cached playlist " + localPl.Name);
            PlaylistImage.SetImage(localPl.Images.ElementAt(0).Url.ToString());
        }
        

        [UIAction("GetEvent")]
        public void GetEvent()
        {
            Plugin.Log.Debug("Button Pushed");

            Plugin.Log.Debug("SetupList Ran");
            customListTableData.data.Clear();
            SetupList();

            

        }


        [UIAction("#post-parse")]
        public void SetupList()
        {
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
                string subtext = pl.Tracks.Total + " Tracks.";
                var customCellInfo = new CustomListTableData.CustomCellInfo(pl.Name, subtext, sp);
                customListTableData.data.Add(customCellInfo);
                i = i + 1;
            }
            i = 0;
            customListTableData.tableView.ReloadData();
            var selectedPlaylist = Configuration.PluginConfig.Instance.ChosenPlaylist;

            customListTableData.tableView.SelectCellWithIdx(selectedPlaylist);
            if (!customListTableData.tableView.visibleCells.Where(x => x.selected).Any())
                customListTableData.tableView.ScrollToCellWithIdx(selectedPlaylist, TableViewScroller.ScrollPositionType.Beginning, true);
        }

        




        public async Task GetPlaylists(int max)
        {
            pog = await Core.Spotify.Interaction.GetPlaylists(max);
            Plugin.Log.Debug("Found " + pog.Count + " Playlists, the first is " + pog.ElementAt(0).Name + ".");
        }
        
    }
}
