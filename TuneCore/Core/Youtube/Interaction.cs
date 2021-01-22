using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;

namespace TuneSaber.Core.Youtube
{
    public class Song : IEquatable<Song>
    {
        public string SongName { get; set; }
        public string SongID { get; set; }
        public override string ToString()
        {
            return "ID: " + SongID + "   Name: " + SongName;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Song objAsSong = obj as Song;
            if (objAsSong == null) return false;
            else return Equals(objAsSong);
        }

        public bool Equals(Song other)
        {
            if (other == null) return false;
            return (this.SongID.Equals(other.SongID));
        }
    }

    internal class Interaction
    {
        public static int maxresults = 3;
        public static int playlistamount = 3;
        public static int playlistindex = 1;
        public static string[,] videoArray = new string[4, maxresults];
        public static string[,] playlistArray = new string[3, 20];
        public static int playlistResult = 0;
        public static int songResult = 0;
        public static string thistype = "";
        public static List<Song> songs = new List<Song>();



        public static UserCredential credential;
        public static YouTubeService youtubeService;
        [STAThread]

        public static void Start()
        {
            _ = Shitfuck2();
            thistype = GetPlaylists(20).GetType().ToString();
            GetPlaylists(20);
        }


        public static async Task Shitfuck2()
        {
            try
            {
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = "858293886951-tgitfjlevicgvgk1bet7d1r9dhm1qs1u.apps.googleusercontent.com",
                            ClientSecret = "P0HkPBMFranVg8mkDil_VvpD"
                        },
                        // This OAuth 2.0 access scope allows for full read/write access to the
                        // authenticated user's account.
                        new[] { YouTubeService.Scope.Youtube },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(thistype)
                    );
                }
                youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    //ApplicationName = this.GetType().ToString()
                });
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical(ex.ToString());
            }
        }
        static void PlaylistMain(string[] args)
        {



        }


        public async Task LikeSong(int song)
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.YoutubeForceSsl },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            string video = videoArray.GetValue(3, song).ToString();
            string videotitle = videoArray.GetValue(1, song).ToString();
            Plugin.Log.Debug("Video: " + videotitle + " with ID: " + video + '\n');
            var RatingLike = VideosResource.RateRequest.RatingEnum.Like;





            string CurrentRating = youtubeService.Videos.GetRating(video).ToString();

            Plugin.Log.Debug("current rating for " + videotitle + " is: " + CurrentRating.ToString());



            try
            {
                Plugin.Log.Debug("Giving " + video + " a " + RatingLike.ToString() + " rating.");

                Plugin.Log.Debug("rating given. \n");
            }
            catch (Exception e)
            {
                Plugin.Log.Debug("Error: " + e.Message);
            }


        }
        public static async Task AddToPlaylist(int playlist, int song)
        {
            Plugin.Log.Info("adding function ran");
            string video = videoArray.GetValue(3, song).ToString();
            Plugin.Log.Debug(video + " is the video ID,");
            string eplaylist = playlistArray.GetValue(2, playlist).ToString();
            Plugin.Log.Debug("and " + eplaylist + " is the playlist ID.");

            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = eplaylist;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = video;
            try
            {
                newPlaylistItem = await youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();
            }
            catch (Exception e)
            {
                Plugin.Log.Debug("Error: " + e.Message);
            }
        }

        public static async Task<int> GetQuery(string term, int maxresults)
        {
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = term; // Replace with your search term.
            searchListRequest.Type = "video";
            searchListRequest.VideoCategoryId = "10";
            searchListRequest.TopicId = "/m/04rlf";
            searchListRequest.MaxResults = maxresults;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            Plugin.Log.Debug("Searching For " + maxresults.ToString() + " Songs.");

            int videonumber = 0;
            int videodisplaynumber = 1;
            // Add each result to the appropriate list, and then display the lists of
            // matching videos.
            foreach (var searchResult in searchListResponse.Items)
            {
                Plugin.Log.Debug("Begginning array writing");
                videoArray.SetValue(searchResult.Snippet.Title.ToString(), 0, videonumber);
                videoArray.SetValue(searchResult.Snippet.ChannelTitle.ToString(), 1, videonumber);
                videoArray.SetValue(searchResult.Snippet.Thumbnails.Medium.Url.ToString(), 2, videonumber);
                videoArray.SetValue(searchResult.Id.VideoId.ToString(), 3, videonumber);
                videonumber = videonumber + 1;
            }
            Plugin.Log.Debug("Wrote " + videonumber.ToString() + " songs to the array.");
            videonumber = 0;
            Plugin.Log.Debug("Search Complete");
            foreach (var searchresult in searchListResponse.Items)
            {

                Plugin.Log.Debug("Video " + videodisplaynumber + ": " + videoArray.GetValue(0, videonumber));
                Plugin.Log.Debug("Channel " + videodisplaynumber + ": " + videoArray.GetValue(1, videonumber));
                Plugin.Log.Debug("Thumbnail URL " + videodisplaynumber + ": " + videoArray.GetValue(2, videonumber));
                Plugin.Log.Debug("Video ID " + videodisplaynumber + ": " + videoArray.GetValue(3, videonumber));
                Plugin.Log.Debug("\n");
                videonumber = videonumber + 1;
                videodisplaynumber = videodisplaynumber + 1;
            }
            videonumber = 0;
            videodisplaynumber = 1;
            Plugin.Log.Debug("fuckshit");
            return 1;
        }

        public async Task GetSongRating(int song)
        {
            string IdToSearch = videoArray.GetValue(3, song).ToString();

            var rating = youtubeService.Videos.GetRating(IdToSearch);
            string videorating = rating.ToString();
            Plugin.Log.Debug("rating: " + videorating);

        }

        public async Task PlaylistToList(string playlistID)
        {

            var playlist = youtubeService.PlaylistItems.List("snippet, id, contentDetails, status");
            playlist.MaxResults = 500;
            playlist.PlaylistId = playlistID;
            var playlistListResponse = await playlist.ExecuteAsync();

            foreach (var videoResult in playlistListResponse.Items)
            {
                songs.Add(new Song() { SongName = videoResult.Snippet.Title, SongID = videoResult.ContentDetails.VideoId });
                Plugin.Log.Debug("Wrote Song: " + videoResult.Snippet.Title + " With ID: " + videoResult.ContentDetails.VideoId + " to the list.");
            }
            await Task.Delay(500);
        }

        public async Task<bool> IsIDInPlaylist(string SongID)
        {
            return songs.Contains(new Song { SongID = SongID, SongName = "" });
        }

        public static async Task<int> GetPlaylists(int maxresults)
        {
            var playlistListRequest = youtubeService.Playlists.List("snippet, contentDetails");
            playlistListRequest.Mine = true;
            playlistListRequest.MaxResults = maxresults;
            var playlistListResponse = await playlistListRequest.ExecuteAsync();



            int playlistnumber = 0;
            int playlistdisplaynumber = 1;
            // Add each result to the appropriate list, and then display the lists of
            // matching playlists.
            foreach (var playlistResult in playlistListResponse.Items)
            {
                playlistArray.SetValue(playlistResult.Snippet.Title.ToString(), 0, playlistnumber);
                playlistArray.SetValue(playlistResult.Snippet.Thumbnails.Medium.Url.ToString(), 1, playlistnumber);
                playlistArray.SetValue(playlistResult.Id.ToString(), 2, playlistnumber);
                playlistnumber = playlistnumber + 1;
                playlistdisplaynumber = playlistdisplaynumber + 1;
            }
            playlistnumber = 0;
            playlistdisplaynumber = 1;
            playlistamount = Int32.Parse(playlistdisplaynumber.ToString());
            Plugin.Log.Debug("Search Complete With " + playlistdisplaynumber.ToString() + " Results.");

            foreach (var playlistresult in playlistListResponse.Items)
            {

                Plugin.Log.Debug("Playlist " + playlistdisplaynumber + ": " + playlistArray.GetValue(0, playlistnumber));
                Plugin.Log.Debug("Thumbnail URL " + playlistdisplaynumber + ": " + playlistArray.GetValue(1, playlistnumber));
                Plugin.Log.Debug("Playlist ID " + playlistdisplaynumber + ": " + playlistArray.GetValue(2, playlistnumber));
                Plugin.Log.Debug("\n");

                playlistnumber = playlistnumber + 1;
                playlistdisplaynumber = playlistdisplaynumber + 1;
            }
            playlistnumber = 0;
            playlistdisplaynumber = 1;

            return playlistamount;
        }
    }
}
