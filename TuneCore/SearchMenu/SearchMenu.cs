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

    public class SearchMenu : NotifiableSingleton<SearchMenu>
    {
        public string currentID = "";
        public static string currentTitle = "";
        public static string currentArtist = "";
        public static string currentURL = "";
        public static SpotifyAPI.Web.FullTrack CurrentTrack;
        public static Material RoundedEdge = null!;
        public static List<SpotifyAPI.Web.FullArtist> ars = new List<SpotifyAPI.Web.FullArtist>();
        public static SpotifyAPI.Web.FullArtist CurrentArtist = null!;
        /*[UIComponent("pl-name")] public TextMeshProUGUI PLName = null!;
        [UIComponent("playlist-image")] public Image PlaylistImage = null!;*/
        [UIComponent("songtext")] private TextMeshProUGUI SongText = null!;
        [UIComponent("songsubtext")] private TextMeshProUGUI SongSubText = null!;
        [UIComponent("songsubsubtext")] private TextMeshProUGUI SongSubSubText = null!;
        [UIComponent("songartist")] private TextMeshProUGUI SongArtist = null!;
        [UIComponent("like-image")] private Image LikeImage = null!;
        [UIComponent("add-image")] private Image AddImage = null!;
        [UIComponent("relog-image")] public Image RelogImage = null!;
        [UIComponent("playlist-image")] public Image AddPLButton = null!;
        [UIComponent("queue-image")] public Image QueueImage = null!;
        [UIComponent("song-background-image")] private ImageView SongBGImage = null!;
        //[UIComponent("artists-background-image")] private ImageView ArtistsBGImage = null!;

        [UIComponent("artists-panel")] public HorizontalOrVerticalLayoutGroup ArtistsPanel = null!;
        [UIComponent("twitch-panel")] public HorizontalOrVerticalLayoutGroup TwitchPanel = null!;

        [UIComponent("likeselected")] private Button LikeButton;
        [UIComponent("addselected")] private Button AddButton;
        [UIComponent("follow-button")] private Button FollowButton;
        [UIComponent("relog-button")] public Button RelogButton;
        [UIComponent("queue-button")] public Button QueueButton;
        [UIValue("source-url1")] public string Image1 = "";
        [UIValue("selectedurl")] public string SelectedURL = "";
        [UIComponent("song-tab")] public VerticalLayoutGroup SongTab;
        [UIComponent("top-left")] public VerticalLayoutGroup TopLeft;
        [UIComponent("top-middle")] public VerticalLayoutGroup TopMiddle;
        [UIComponent("ArtistList")] public CustomListTableData customListTableData;


        //twitch stuffs
        [UIValue("twitch-permission-choices")] public List<object> options = new object[] { "Everyone", "Sub", "VIP", "Mod", "Broadcaster" }.ToList();
        [UIComponent("twitch-image")] public Image TwitchImage = null!;
        [UIComponent("twitch-button")] public Button TwitchButton;
        [UIValue("song-bool")] public bool TwitchSongBool = Configuration.PluginConfig.Instance.SongCommandEnabled;
        [UIValue("song-perm-choice")] private string SongPermChoice = Configuration.PluginConfig.Instance.SongCommandPerm;
        [UIValue("playlist-perm-choice")] private string PlaylistPermChoice = Configuration.PluginConfig.Instance.PlaylistCommandPerm;
        [UIValue("admin-perm-choice")] private string AdminPermChoice = Configuration.PluginConfig.Instance.TwitchAdminLevel;
        [UIValue("playlist-bool")] public bool TwitchPlaylistBool = Configuration.PluginConfig.Instance.PlaylistCommandEnabled;

        [UIAction("twitchConfirm")]
        public void ConfirmTwitchSettings()
        {
            Configuration.PluginConfig.Instance.SongCommandEnabled = TwitchSongBool;
            Configuration.PluginConfig.Instance.SongCommandPerm = SongPermChoice;
            Configuration.PluginConfig.Instance.PlaylistCommandEnabled = TwitchPlaylistBool;
            Configuration.PluginConfig.Instance.PlaylistCommandPerm = PlaylistPermChoice;
            Configuration.PluginConfig.Instance.TwitchAdminLevel = AdminPermChoice;
            TwitchPanel.gameObject.SetActive(true);
            ArtistsPanel.gameObject.SetActive(false);
        }
        [UIAction("show-twitch")]
        private void ShowTwitch()
        {
            TwitchPanel.gameObject.SetActive(false);
            ArtistsPanel.gameObject.SetActive(true);
        }

        [UIAction("twitch-settings")]
        public void TwitchSettings()
        {

        }
        private string _text = "something broke";
        [UIValue("addbuttonhoverhint")]
        public string AddHoverHint
        {
            get => _text;
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }
        [UIValue("queuehoverhint")]
        public string QueueHoverHint
        {
            get => _text;
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }

        private Image placeholder = null!;


        [UIAction("refresh-login")]
        private void RefreshLogin()
        {
            System.Threading.Thread loginThread = new System.Threading.Thread(async => Interaction.Login());
            loginThread.Start();
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
                
            }
        }
        
        [UIAction("follow-artist")]
        public void FollowArtist()
        {
            ButtonFollow();
        }
        [UIAction("addtoqueue")]
        public async Task AddToQueue()
        {
            await Interaction.AddQueue(CurrentTrack);
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
                    LikeImage.SetImage("TuneSaber.Icons.heart.png");
                    LikeImage.color = Color.red;
                    break;
                case false:
                    await Interaction.LikeSong(currentID);
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
                    //placeholder function for if i ever figure out how to remove things KEKW
                    break;
                case false:
                    await Interaction.AddToPlaylist(CurrentTrack, Configuration.PluginConfig.Instance.PlaylistID);
                    AddButton.interactable = false;
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

        /*private void MPevents_levelselected(object sender,MultiplayerExtensions.SelectedBeatmapEventArgs arg2)
        {
            var song = SongCore.Loader.GetLevelById(arg2.LevelId);

            if (song.songSubName.Contains(" remix") || song.songSubName.Contains(" Remix") || song.songSubName.Contains(" mix") || song.songSubName.Contains(" Mix"))
            {
                _ = shitfuck(song.songName, song.songAuthorName, song.songSubName, 3);
            }
            else
            {
                _ = shitfuck(song.songName, song.songAuthorName, "", 3);
            }
            Reset();
        }*/
        
        private async Task<bool> shitfuck(string song, string artist, string mix, int max)
        {
            
            DisableButtons();

            string query = song + ", " + artist;
            var track = await Interaction.GetQuery(query, max);
            if (track == null)
            {
                Plugin.Log.Info("No Results");
                await FillSongInfo("No Results.", "");
                SongBGImage.SetImage("");
                SongBGImage.color = Color.clear;
                currentTitle = "sdaijfmchiasoduvbfiusadfv";
                return false;
            }
            else
            {
                try
                {
                    CurrentTrack = track;
                    currentID = track.Id.ToString();
                    SongBGImage.SetImage(track.Album.Images.ElementAt(0).Url.ToString());
                    SongBGImage.color = Color.white;
                    SongBGImage.color0 = Color.clear;
                    SongBGImage.color1 = Color.white;
                    currentURL = track.ExternalUrls.Values.ElementAt(0);
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
                    bool added = true;
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
                                AddImage.color = Color.gray;
                                AddButton.interactable = false;
                                added = true;
                                break;
                            case false:
                                AddImage.color = Color.white;
                                added = false;
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
                    Interaction.CheckPlaylistAddAbility();
                    EnableButtons();
                    switch (added)
                    {
                        case true:
                            AddButton.interactable = false;
                            break;
                        case false:
                            break;
                    }
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
                Plugin.Log.Info("found " + artists.Count + " artists.");
                foreach (var artist in artists)
                {
                    Plugin.Log.Notice("fetching artist " + artist.Name);
                    //ArtistsBGImage.SetImage(artist.Images.ElementAt(0).Url.ToString());
                    var bruh = new CustomListTableData.CustomCellInfo(artist.Name, Core.Tools.TextFormatting.NumberCommas(artist.Followers.Total.ToString()) + " Followers");
                    customListTableData.data.Add(bruh);
                    Plugin.Log.Notice("Added artist " + artist.Name + " to table");
                }
                customListTableData.tableView.ReloadData();
                Task.Delay(500);
                customListTableData.tableView.ReloadData();
                customListTableData.tableView.SelectCellWithIdx(0);
                CurrentArtist = ars.ElementAt(0);
            if (!customListTableData.tableView.visibleCells.Where(x => x.selected).Any())
                customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, true);
            }
            catch (Exception e)
            {
                Plugin.Log.Critical(e);
            }
        }

        

        [UIAction("#post-parse")]
        public void PostParse()
        {
            TwitchPanel.gameObject.SetActive(false);
            Reset();
            switch (Interaction.IsPremium)
            {
                case true:
                    QueueButton.enabled = true;
                    QueueButton.interactable = true;
                    QueueHoverHint = "Add to playback queue";
                    break;
                case false:
                    QueueButton.enabled = false;
                    QueueButton.interactable = false;
                    QueueHoverHint = "Playback queue is not\nmodifiable without premium";
                    break;
            }
            
            RelogImage.SetImage("TuneSaber.Icons.relog.png");
            AddImage.SetImage("TuneSaber.Icons.AddToPlaylist.png");
            QueueImage.SetImage("TuneSaber.Icons.AddToQueue.png");
            LikeImage.SetImage("TuneSaber.Icons.heart.png");
            SongBGImage.color0 = new Color(1, 1, 1, 0.2f);
            SongBGImage.color1 = new Color(1, 1, 1, 0.8f);
            SongBGImage.SetField("_gradient", true);
            SongBGImage.SetField("_gradientDirection", ImageView.GradientDirection.Vertical);
            _ = Interaction.GetPlaylists(99);
            Plugin.Log.Notice("PostParse");
            Material RoundedEdge = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlowRoundEdge").First();
            //ArtistsBGImage.transform.localScale = new Vector3(0, 0, 0);
            SongBGImage.material = RoundedEdge;
            
        }



        #region backend commands
        public void AddTab()
        {
            Plugin.Log.Debug("Adding tab");
            GameplaySetup.instance.AddTab("TuneSaber", "TuneSaber.SearchMenu.search-menu-spotify.bsml", this);
            BSEvents.levelSelected += BSEvents_levelSelected;
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
            customListTableData.data.Clear();
            customListTableData.tableView.ReloadData();
            //CurrentArtists.Clear();
            SongBGImage.SetImage("TuneSaber.Icons.YEP.png");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ColorButton(LikeButton, Color.clear);
            ColorButton(AddButton, Color.clear);
            ColorButton(RelogButton, Color.clear);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            string L = "";
            SongText.text = L;
            SongArtist.text = L;
            SongSubText.text = L;
            SongSubSubText.text = L;
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
            QueueButton.interactable = false;
            AddImage.color = Color.clear;
            LikeImage.color = Color.clear;
            ColorButton(LikeButton, Color.clear);
            ColorButton(AddButton, Color.clear);
        }
        private void EnableButtons()
        {
            QueueButton.interactable = Interaction.IsPremium;
            FollowButton.interactable = true;
            LikeButton.interactable = true;
            switch (Interaction.CheckPlaylistAddAbility())
            {
                case true:
                    AddButton.interactable = true;
                    AddHoverHint = "Add selected song to playlist\n(but not remove it)";
                    break;
                case false:
                    AddHoverHint = "You do not own the current playlist";
                    break;
            }
        }
        #endregion
    }
}
