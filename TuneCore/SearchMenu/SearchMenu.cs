using System;
using System.Collections;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using BS_Utils.Utilities;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
//using TuneSaber.Core.Youtube;
using TuneSaber.Core.Spotify;
using System.Threading.Tasks;
using System.Linq;

namespace TuneSaber
{
    public class SearchMenu : PersistentSingleton<SearchMenu>
    {
        public string currentID = "";
        public static string currentTitle = "";
        public static string currentArtist = "";
        public static string currentURL = "";
        public static SpotifyAPI.Web.FullTrack CurrentTrack;
        public static Material RoundedEdge = null!;
        public static List<SpotifyAPI.Web.FullArtist> ars = new List<SpotifyAPI.Web.FullArtist>();
        public static SpotifyAPI.Web.FullArtist CurrentArtist = null!;
        [UIComponent("pl-name")] public TextMeshProUGUI PLName = null!;
        [UIComponent("songtext")] private TextMeshProUGUI SongText = null!;
        [UIComponent("songsubtext")] private TextMeshProUGUI SongSubText = null!;
        [UIComponent("songsubsubtext")] private TextMeshProUGUI SongSubSubText = null!;
        [UIComponent("songartist")] private TextMeshProUGUI SongArtist = null!;
        [UIComponent("like-image")] private Image LikeImage = null!;
        [UIComponent("song-background-image")] private ImageView SongBGImage = null!;
        //[UIComponent("artists-background-image")] private ImageView ArtistsBGImage = null!;
        [UIComponent("playlist-image")] public Image PlaylistImage = null!;
        [UIComponent("likeselected")] private Button LikeButton;
        [UIComponent("addselected")] private Button AddButton;
        [UIComponent("follow-button")] private Button FollowButton;
        [UIValue("source-url1")] public string Image1 = "";
        [UIValue("selectedurl")] public string SelectedURL = "";
        [UIComponent("song-tab")] public VerticalLayoutGroup SongTab;
        [UIComponent("top-left")] public VerticalLayoutGroup TopLeft;
        [UIComponent("top-middle")] public VerticalLayoutGroup TopMiddle;
        [UIComponent("ArtistList")] public CustomListTableData customListTableData;

        private Image placeholder = null!;


        [UIAction("refresh-login")]
        private void RefreshLogin()
        {
            Interaction.Login().Wait();
            Interaction.GetPlaylists(99).Wait();
        }

        [UIAction("artistSelect")]
        public void Select(TableView _, int row)
        {
            CurrentArtist = ars.ElementAt(row);
            CheckFollow();
            Plugin.Log.Notice("Artist " + ars.ElementAt(row).Name + " clicked.");
        }

        [UIAction("bruh-moment")]
        public void RefreshCell(bool isSelected, bool isHighlighted)
        {
            if (isSelected == true)
            {
                Plugin.Log.Notice("Cell Hovered.");
            }
        }
        
        [UIAction("follow-artist")]
        public void FollowArtist()
        {
            Plugin.Log.Info("button pushed");
            ButtonFollow();
        }
        
        private async Task ButtonFollow()
        {
            Plugin.Log.Notice("ButtonFollow()");
            switch(await Interaction.IsArtistFollowed(CurrentArtist))
            {
                case true:
                    Plugin.Log.Notice("Artist is followed");
                    await Interaction.UnfollowArtist(CurrentArtist);
                    await ColorButton(FollowButton, Color.red);

                    break;
                case false:
                    Plugin.Log.Notice("Attempting to run follow interaction");
                    await Interaction.FollowArtist(CurrentArtist);
                    await ColorButton(FollowButton, Color.green);
                    break;
            }
        }

        [UIAction("buttonaddfire")]
        private void ButtonAddFire()
        {
            _ = SongAdd();
        }
        private async Task SongAdd()
        {
            _ = ButtonAdd();
        }

        [UIAction("buttonlikefire")]
        private void ButtonLikeFire()
        {
            _ = ButtonLike();
        }

        private async Task ButtonLike()
        {
            switch (await Interaction.IsSongLiked(currentID))
            {
                case true:
                    await Interaction.DislikeSong(currentID);
                    //LikeButton.transform.Find("Underline").gameObject.GetComponent<Image>().color = Color.red;
                    LikeImage.SetImage("TuneSaber.Icons.heart.png");
                    LikeImage.color = Color.red;
                    break;
                case false:
                    await Interaction.LikeSong(currentID);
                    //LikeButton.transform.Find("Underline").gameObject.GetComponent<Image>().color = Color.green;
                    LikeImage.SetImage("TuneSaber.Icons.heart_filled.png");
                    LikeImage.color = Color.green;
                    break;
            }
        }
        private async Task ButtonAdd()
        {
            switch (await Interaction.IsSongInPlaylist(CurrentTrack.Id, Configuration.PluginConfig.Instance.PlaylistID))
            {
                case true:
                    //await Interaction.RemoveFromPlaylist(CurrentTrack, Configuration.PluginConfig.Instance.PlaylistID);
                    //AddButton.transform.Find("Underline").gameObject.GetComponent<Image>().color = Color.red;
                    //AddButton.SetButtonText("Add");
                    //await Interaction.IsSongInPlaylist(CurrentTrack.Id, Configuration.PluginConfig.Instance.PlaylistID);
                    break;
                case false:
                    await Interaction.AddToPlaylist(CurrentTrack, Configuration.PluginConfig.Instance.PlaylistID);
                    AddButton.transform.Find("Underline").gameObject.GetComponent<Image>().color = Color.green;
                    AddButton.SetButtonText("oof");
                    AddButton.interactable = false;
                    //await Interaction.IsSongInPlaylist(CurrentTrack.Id, Configuration.PluginConfig.Instance.PlaylistID);
                    break;
            }
        }

        #region playlist changing
        [UIAction("playlist-changed")]
        private void PlaylistChanged()
        {
            Plugin.Log.Debug("bruh.");
        }


        #endregion



        public void Start()
        {
            _ = bababooey();
        }
        private async Task bababooey()
        {
            await Interaction.Login();

        }
        private async Task FillSongInfo(string title, string artist)
        {
            string MainTitle = "";
            string SubTitle = "";
            string SubSubTitle = "";
            try
            {
                int SubStart = title.IndexOf('(');
                int SubEnd = title.IndexOf(')');
                int SubSubStart = title.IndexOf(" - ");

                

                if(SubStart != -1) //checks to see if there's a subtitle
                {
                    MainTitle = title.Substring(0, SubStart);
                    SubTitle = title.Substring(SubStart + 1, SubEnd - SubStart - 1);
                    if (SubSubStart != -1) //checks to see if there's a sub sub title
                    {
                        SubSubTitle = title.Substring(SubSubStart + 3);
                    }
                }
                else
                {
                    if (SubSubStart != -1) //checks to see if there's a sub sub title
                    {
                        SubTitle = title.Substring(SubSubStart + 3);
                        MainTitle = title.Substring(0, SubSubStart);
                    }
                    else
                    {
                        MainTitle = title;
                    }
                }
                
                
                
            }
            catch(Exception e) { Plugin.Log.Critical(e.ToString()); }

            SongText.text = MainTitle;
            SongSubText.text = SubTitle;
            SongSubSubText.text = SubSubTitle;
            if(artist.Length > 20)
            {
                SongArtist.text = artist.Substring(0, 20) + "...";
            }
            else
            {
                SongArtist.text = artist;
            }

            SongArtist.text = artist;
        }

        private void BSEvents_levelSelected(LevelCollectionViewController arg1, IPreviewBeatmapLevel arg2)
        {
            customListTableData.data.Clear();
            Reset();
            
            if (arg2.songSubName.Contains(" remix") || arg2.songSubName.Contains(" Remix") || arg2.songSubName.Contains(" mix") || arg2.songSubName.Contains(" Mix"))
            {
                _ = shitfuck(arg2.songName, arg2.songAuthorName, arg2.songSubName, 3);
            }
            else
            {
                _ = shitfuck(arg2.songName, arg2.songAuthorName, "", 3);
            }


        }

        
        private async Task<bool> shitfuck(string song, string artist, string mix, int max)
        {
            
            DisableButtons();

            string query = song + ", " + artist;
            var track = await Interaction.GetQuery(query, max);
            if (track == null)
            {
                Plugin.Log.Info("shit");
                await FillSongInfo("No Results.", "");
                SongBGImage.SetImage("");
                return false;
            }
            else
            {
                try
                {
                    CurrentTrack = track;
                    currentID = track.Id.ToString();
                    SongBGImage.SetImage(track.Album.Images.ElementAt(0).Url.ToString());
                    SongBGImage.color0 = Color.clear;
                    SongBGImage.color1 = Color.white;
                    //SongBGImage.SetField("gradient", true, typeof(bool));
                    //Plugin.Log.Notice("gradient set");
                    string at = "";
                    int artistss = track.Artists.Count;
                    int others = 0;
                    #region artist counting
                    //checks to see if there are more than 2 artists
                    if (track.Artists.Count > 2)
                    {
                        //if there are, cap it at 3
                        artistss = 3;
                        //sets the amount of "others"
                        others = track.Artists.Count - 1;
                    }
                    else
                    {
                        //if there are 1 or 2, just leave the count as-is
                        artistss = track.Artists.Count;
                    }
                    switch (artistss)
                    {
                        //if there's one, leave it alone
                        case 1:
                            at = track.Artists.ElementAt(0).Name.ToString();
                            break;
                        //if there's two, set it accordingly
                        case 2:
                            at = track.Artists.ElementAt(0).Name.ToString() + " & " + track.Artists.ElementAt(1).Name.ToString();
                            break;
                        //if there's 3 ore more, overflow the others
                        case 3:
                            at = track.Artists.ElementAt(0).Name.ToString() + " & " + others.ToString() + " others.";
                            break;
                    }
                    #endregion
                    #region add/like buttons
                    try
                    {
                        switch (await Interaction.IsSongLiked(currentID))
                        {
                            case true:
                                LikeImage.SetImage("TuneSaber.Icons.heart_filled.png");
                                LikeImage.color = Color.green;
                                break;
                            case false:
                                LikeImage.SetImage("TuneSaber.Icons.heart.png");
                                LikeImage.color = Color.red;
                                break;
                        }
                    }
                    catch (Exception e) { Plugin.Log.Critical("song liked error: " + e.ToString()); }
                    try
                    {
                        switch (await Interaction.IsSongInPlaylist(currentID, Configuration.PluginConfig.Instance.PlaylistID))
                        {
                            case true:
                                AddButton.SetButtonText("oof");
                                AddButton.transform.Find("Underline").gameObject.GetComponent<Image>().color = Color.green;
                                AddButton.interactable = false;
                                break;
                            case false:
                                AddButton.SetButtonText("Add");
                                AddButton.transform.Find("Underline").gameObject.GetComponent<Image>().color = Color.red;
                                break;
                        }
                    }
                    catch (Exception e) { Plugin.Log.Critical("song in playlist error: " + e.ToString()); }
                    #endregion
                    currentTitle = track.Name.ToString();
                    currentArtist = at;
                    await FillSongInfo(track.Name.ToString(), at);
                    ars = await Gib(track);
                    GimmieArtists(ars);
                    await CheckFollow();
                    EnableButtons();
                    return true;
                }
                catch(Exception e) { Plugin.Log.Critical(e.ToString()); return false; }
            }
        }
        public async Task<List<SpotifyAPI.Web.FullArtist>> Gib(SpotifyAPI.Web.FullTrack track)
        {
            List<SpotifyAPI.Web.FullArtist> ars = new List<SpotifyAPI.Web.FullArtist>();
            foreach (var artist in track.Artists)
            {
                var ar = await Interaction.GetArtist(artist.Id);
                ars.Add(ar);
            }
            return ars;
        }
        public void GimmieArtists(List<SpotifyAPI.Web.FullArtist> artists)
        {
            
            try
            {
                customListTableData.data.Clear();
                Plugin.Log.Info("fuck you " + artists.Count + " times.");
                foreach (var artist in artists)
                {
                    Plugin.Log.Notice("fetching artist " + artist.Name);
                    //ArtistsBGImage.SetImage(artist.Images.ElementAt(0).Url.ToString());
                    var bruh = new CustomListTableData.CustomCellInfo(artist.Name, artist.Followers.Total.ToString() + " Followers");
                    customListTableData.data.Add(bruh);
                    Plugin.Log.Notice("Added artist " + artist.Name + " to table");
                }
                customListTableData.tableView.ReloadData();
                Task.Delay(500);
                customListTableData.tableView.ReloadData();
                customListTableData.tableView.SelectCellWithIdx(0);
                CurrentArtist = ars.ElementAt(0);
            if (!customListTableData.tableView.visibleCells.Where(x => x.selected).Any())
                customListTableData.tableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Beginning, true);
            }
            catch (Exception e)
            {
                Plugin.Log.Critical(e);
            }
        }

        [UIAction("#post-parse")]
        public void PostParse()
        {
            SongBGImage.color0 = new Color(1, 1, 1, 0.2f);
            SongBGImage.color1 = new Color(1, 1, 1, 0.8f);
            SongBGImage.SetField("_gradient", true);
            SongBGImage.SetField("_gradientDirection", HMUI.ImageView.GradientDirection.Vertical);
            Plugin.Log.Notice("PostParse");
            Material RoundedEdge = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlowRoundEdge").First();
            //ArtistsBGImage.transform.localScale = new Vector3(0, 0, 0);
            SongBGImage.material = RoundedEdge;
            
        }



        #region backend commands
        public void AddTab()
        {
            BSEvents.levelSelected += BSEvents_levelSelected;
            Plugin.Log.Debug("Adding tab");
            GameplaySetup.instance.AddTab("TuneSaber", "TuneSaber.SearchMenu.search-menu-spotify.bsml", this);
        }

        public void RemoveTab()
        {
            BSEvents.levelSelected -= BSEvents_levelSelected;
            Plugin.Log.Debug("Removing tab");
            GameplaySetup.instance.RemoveTab("TuneSaber");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task ColorButton(Button button, Color color)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            button.transform.Find("Underline").gameObject.GetComponent<Image>().color = color;
        }

        private void Reset()
        {
            //CurrentArtists.Clear();
            SongBGImage.SetImage("TuneSaber.Icons.YEP.png");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ColorButton(LikeButton, Color.clear);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            string L = "";
            SongText.text = L;
            SongArtist.text = L;
        }
        private async Task CheckFollow()
        {
            switch (await Interaction.IsArtistFollowed(CurrentArtist))
            {
                case true:
                    Plugin.Log.Notice("Artist is followed");
                    FollowButton.SetButtonText("Unfollow Artist");
                    await ColorButton(FollowButton, Color.green);
                    break;
                case false:
                    Plugin.Log.Notice("Artist is unfollowed");
                    FollowButton.SetButtonText("Follow Artist");
                    await ColorButton(FollowButton, Color.red);
                    break;
            }
        }
        private void DisableButtons()
        {
            FollowButton.interactable = false;
            LikeButton.interactable = false;
            AddButton.interactable = false;
            LikeImage.color = Color.clear;
            LikeButton.transform.Find("BG").gameObject.GetComponent<Image>().color = Color.clear;
        }
        private void EnableButtons()
        {
            FollowButton.interactable = true;
            LikeButton.interactable = true;
            AddButton.interactable = true;
        }
        #endregion
    }
}
