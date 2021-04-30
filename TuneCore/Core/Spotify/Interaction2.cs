using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net;
using System.Text;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using UnityEngine;
using EmbedIO;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TuneSaber.Core.Spotify
{
    class Interaction
    {
        public static int playlistamount = 3;
        public static int playlistindex = 1;
        public static List<SpotifyAPI.Web.SimplePlaylist> playlists = new List<SimplePlaylist>();
        public static PrivateUser CurrentUser = null!;
        public static SpotifyAPI.Web.SimplePlaylist CurrentPlaylist = null!;
        public static string PlaylistOwnerID = "";
        public static int playlistResult = 0;
        public static int songResult = 0;
        public static bool IsPremium = false;

        public static SpotifyClient spotify;
        private static AuthServerHandler.EmbedIOAuthServer _server;
        static public async Task<SpotifyClient> GetMyFuckingAPIShit()
        {
            await Task.Delay(1);
            return spotify;
        }
        static public async Task Login()
        {

            var bob = new System.Threading.Thread(async () => await ListenForTheThing());
            bob.Start();

            var request = new LoginRequest(new Uri("http://localhost:6969/callback"), "8f6a2395551c45588496c7e25a0e2311", LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { Scopes.PlaylistModifyPrivate, Scopes.PlaylistReadPrivate, Scopes.UserLibraryModify, Scopes.UserLibraryRead, Scopes.PlaylistModifyPublic, Scopes.PlaylistReadCollaborative, Scopes.UserFollowModify, Scopes.UserFollowRead, Scopes.UserModifyPlaybackState, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackPosition, Scopes.UserReadPlaybackState }
            };
            Uri urii = request.ToUri();
            try { BrowserUtil.Open(urii); } catch (Exception e) { Plugin.Log.Critical("Browser Open Error" + e.ToString()); }
            Plugin.Log.Debug("browser opened");
            await Task.Delay(100);
            _server = new AuthServerHandler.EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await _server.Start();
            Plugin.Log.Notice("server started");
            _server.AuthorizationCodeReceived += OnAuthCodeGet;

            

            
        }

        static async Task ListenForTheThing()
        {
            HttpListener listener = new HttpListener();
            try
            {
                listener.Prefixes.Add("http://localhost:5001/");
            }catch(Exception e) { Plugin.Log.Critical(e.ToString()); }
            listener.Start();

            Plugin.Log.Notice("now listening for incoming http requests");
            while (true)
            {
                Plugin.Log.Info("AAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                try
                {
                    var context = await listener.GetContextAsync();

                    Plugin.Log.Notice("incoming " + context);
                    //var headers = context.Response.Headers.AllKeys.ToList();
                    var headers2 = context.Request.Headers.AllKeys.ToList();
                    
                    /*Plugin.Log.Debug("response headers");
                    foreach (var header in headers)
                    {
                        
                        Plugin.Log.Info(header.ToString());
                    }*/
                    Plugin.Log.Debug("request headers");
                    foreach (var header in headers2)
                    {
                        

                        Plugin.Log.Info(header.ToString());
                        var deets = context.Request.Headers.GetValues(header).ElementAt(0);
                        
                    }
                    HttpListenerResponse response = context.Response;
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("{\"status\": \"ok\"}");
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                catch(Exception e) { Plugin.Log.Critical(e.ToString()); }
            }
        }
        
        static private async Task OnAuthCodeGet(object sender, AuthObjects response)
        {
            await _server.Stop();
            Plugin.Log.Debug("server stopped");
            try
            {

                Plugin.Log.Notice("access token is: " + response.AccessCode);
                spotify = new SpotifyClient(response.AccessCode);
                var f = await spotify.UserProfile.Current();
                CurrentUser = f;
                Plugin.Log.Info("logged in " + f.DisplayName);
                
                switch (f.Product){
                    case null:
                        IsPremium = false;
                        Plugin.Log.Notice("no premium");
                        break;
                    case "premium":
                        IsPremium = true;
                        Plugin.Log.Notice("has premium");
                        break;
                }
                await GetPlaylists(99);
                CurrentPlaylist = playlists.ElementAt(Configuration.PluginConfig.Instance.ChosenPlaylist);
            }
            catch(Exception e)
            {
                Plugin.Log.Notice("Initial login error \n");
                Plugin.Log.Critical(e.ToString());
                SearchMenu.instance.RelogImage.color = Color.red;
            }
            
        }
        static public async Task<FullTrack> GetQuery(string query, int max)
        {

            var searchitem = new SearchRequest(SearchRequest.Types.Track, query);
            var spotify = await GetMyFuckingAPIShit();
            var results = await spotify.Search.Item(searchitem);
            Plugin.Log.Info("results: " + results.Tracks.Items.Count.ToString());
            int res = results.Tracks.Items.Count;
            if (res == 0)
            {
                return null;
            }
            else
            {
                return results.Tracks.Items.ElementAt(0);
            }

        }
        static public async Task<FullArtist> GetArtist(string ID)
        {
            var spotify = await GetMyFuckingAPIShit();
            var artist = await spotify.Artists.Get(ID);
            return artist;

        }
        static public async Task<List<SimplePlaylist>> GetPlaylists(int max)
        {
            var spotify = await GetMyFuckingAPIShit();
            var user = new PrivateUser();
            try
            {
                user = await spotify.UserProfile.Current();
                Plugin.Log.Info("Logged in user is " + user.DisplayName + " with " + user.Followers.Total.ToString() + " followers.");
            }
            catch (Exception e)
            {
                Plugin.Log.Critical("Login error: " + e.ToString());
            }


            var playlistss = new Paging<SimplePlaylist>();
            try
            {
                var pcur = new PlaylistCurrentUsersRequest();
                pcur.Limit = max;
                playlistss = await spotify.Playlists.GetUsers(user.Id);
                var playlistslist = playlistss.Items;
                Plugin.Log.Info("Found " + playlistslist.Count.ToString() + " playlists.");
                playlists = playlistslist;
                return playlistslist;
            }
            catch (Exception e)
            {
                Plugin.Log.Critical("playlist fetch error: " + e.ToString());
            }
            return null;


        }
        static public async Task<bool> IsSongLiked(string songID)
        {
            try
            {
                var spotify = await GetMyFuckingAPIShit();
                List<string> ID = new List<string>();
                ID.Add(songID);
                var checkrequest = new LibraryCheckTracksRequest(ID);
                var check = await spotify.Library.CheckTracks(checkrequest);
                return check.ElementAt(0);
            }
            catch (Exception e)
            {
                Plugin.Log.Critical(e.ToString());
                return false;
            }
        }
        static public async Task<bool> IsSongInPlaylist(string songID, string playlistID) {
            try
            {
                var spotify = await GetMyFuckingAPIShit();
                var checkrequest = await spotify.Playlists.GetItems(playlistID);

                //i'm about to commit a sin but i do not give a shit, i'll do it later

                foreach (PlaylistTrack<IPlayableItem> item in checkrequest.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        Plugin.Log.Notice("item is a track");
                        switch (track.Id.Equals(songID))
                        {
                            case true:
                                Plugin.Log.Notice("YUP!");
                                return true;
                            case false:
                                Plugin.Log.Notice("Nope.");
                                break;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Plugin.Log.Critical("PL check error: " + e.ToString());
                return false;
            }
            Plugin.Log.Notice("no tracks in the playlist match the song");
            return false;
        }
        static public async Task LikeSong(string songID)
        {
            var spotify = await GetMyFuckingAPIShit();
            List<string> list = new List<string>();
            list.Add(songID);
            var tracksave = new LibrarySaveTracksRequest(list);
            await spotify.Library.SaveTracks(tracksave);
        }
        static public async Task DislikeSong(string songID)
        {
            var spotify = await GetMyFuckingAPIShit();
            List<string> list = new List<string>();
            list.Add(songID);
            var tracksave = new LibraryRemoveTracksRequest(list);
            await spotify.Library.RemoveTracks(tracksave);
        }
        static public async Task AddToPlaylist(FullTrack song, string playlistID)
        {
            var spotify = await GetMyFuckingAPIShit();

            List<string> songs = new List<string>();
            songs.Add(song.Uri);
            var request = new PlaylistAddItemsRequest(songs);

            await spotify.Playlists.AddItems(playlistID, request);
        }
        public static bool CheckPlaylistAddAbility()
        {
            
            switch (CurrentPlaylist.Owner.Id == CurrentUser.Id)
            {
                case true:
                    SearchMenu.instance.AddPLButton.color = Color.white;
                    return true;
                case false:
                    SearchMenu.instance.AddPLButton.color = Color.red;
                    return false;
            }
        }
        static public async Task RemoveFromPlaylist(FullTrack song, string playlistID)
        {
            var spotify = await GetMyFuckingAPIShit();
            var playlists = await spotify.Playlists.GetItems(playlistID);
            int pos = 0;
            foreach (PlaylistTrack<IPlayableItem> traack in playlists.Items)
            {
                pos = playlists.Items.IndexOf(traack);

            }
            var item = new PlaylistRemoveItemsRequest.Item
            {
                Uri = song.Uri
                
        };
            item.Positions = new List<int>();
            item.Positions.Add(pos);
            List<SpotifyAPI.Web.PlaylistRemoveItemsRequest.Item> items = new List<PlaylistRemoveItemsRequest.Item>();
            var request = new PlaylistRemoveItemsRequest();
            request.Tracks.Add(item);
            

            await spotify.Playlists.RemoveItems(playlistID, request);
        }
        static public async Task<bool> IsArtistFollowed(FullArtist ar)
        {
            try
            {
                var spotify = await GetMyFuckingAPIShit();
                List<string> idlist = new List<string>();
                idlist.Add(ar.Id.ToString());
                var fccur = new FollowCheckCurrentUserRequest(FollowCheckCurrentUserRequest.Type.Artist, idlist);
                var cock = await spotify.Follow.CheckCurrentUser(fccur);
                Plugin.Log.Notice("Artist followed: " + cock.ElementAt(0).ToString());
                return cock.ElementAt(0);
            }catch(Exception e) { Plugin.Log.Critical(e.ToString()); return false; }
        }
        static public async Task FollowArtist(FullArtist ar)
        {
            try
            {
                var spotify = await GetMyFuckingAPIShit();
                List<string> idlist = new List<string>();
                idlist.Add(ar.Id.ToString());
                var fr = new FollowRequest(FollowRequest.Type.Artist, idlist);
                Plugin.Log.Info("Attempting to Follow");
                await spotify.Follow.Follow(fr);
            }
            catch(Exception e)
            {
                Plugin.Log.Critical(e.ToString());
            }
        }
        static public async Task UnfollowArtist(FullArtist ar)
        {
            var spotify = await GetMyFuckingAPIShit();
            List<string> idlist = new List<string>();
            idlist.Add(ar.Id.ToString());
            var ufr = new UnfollowRequest(UnfollowRequest.Type.Artist, idlist);
            await spotify.Follow.Unfollow(ufr);
        }
        static public async Task CreatePlaylist(string name,  bool pub, bool collab, string description = null)
        {
            var spotify = await GetMyFuckingAPIShit();
            Plugin.Log.Notice("Creating Playlist " + name);
            PlaylistCreateRequest plcr = new PlaylistCreateRequest(name);
            if(description != null) { plcr.Description = description; }
            plcr.Collaborative = collab;
            plcr.Public = pub;
            var user = await spotify.UserProfile.Current();
            try
            {
                await spotify.Playlists.Create(user.Id, plcr);
            }
            catch(Exception e) { Plugin.Log.Critical(e.ToString()); }
            
        }
        static public async Task EditPlaylist(string id, string name, bool pub, bool collab, string description)
        {
            var spotify = await GetMyFuckingAPIShit();

            var plcdr = new PlaylistChangeDetailsRequest();
            plcdr.Name = name;
            if(description != null) { plcdr.Description = description; }
            plcdr.Collaborative = collab;
            plcdr.Public = pub;
            Plugin.Log.Notice("editing playlist " + name + " " + pub + " " + collab + " " + description);
            try { await spotify.Playlists.ChangeDetails(id, plcdr); }
            catch(Exception e) { Plugin.Log.Critical(e.ToString()); }
        }
        static public async Task UnfollowPlaylist(string id)
        {
            var spotify = await GetMyFuckingAPIShit();
            await spotify.Follow.UnfollowPlaylist(id);
        }
        static public async Task PlaybackControl(int control)
        {
            var spotify = await GetMyFuckingAPIShit();
            if (control == 0) { await spotify.Player.PausePlayback(); }
            if (control == 1) { await spotify.Player.ResumePlayback(); }
            if (control == 2) { await spotify.Player.SkipPrevious(); }
            if (control == 3) { await spotify.Player.SkipNext(); }
        }
        static public async Task<bool> IsPlayingBack() {
            var spotify = await GetMyFuckingAPIShit();
            var playing = await spotify.Player.GetCurrentPlayback();
            return playing.IsPlaying;
        }
        static public async Task<FullTrack> GetPlayback()
        {
            var spotify = await GetMyFuckingAPIShit();
            var pb = await spotify.Player.GetCurrentPlayback();
            FullTrack tr = (FullTrack)pb.Item;
            return tr;
        }
        static public async Task SeekTo(int ms)
        {
            var spotify = await GetMyFuckingAPIShit();
            long pos = Convert.ToInt64(ms.ToString());
            var pstr = new PlayerSeekToRequest(pos);
            await spotify.Player.SeekTo(pstr);
        }
        static public async Task AddQueue(FullTrack track)
        {
            var spotify = await GetMyFuckingAPIShit();
            var patqr = new PlayerAddToQueueRequest(track.Uri);
            try { await spotify.Player.AddToQueue(patqr); }catch(Exception e)
            {
                Plugin.Log.Critical(e.ToString());
            }
        }
    }
}
