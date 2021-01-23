using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatCore;
using ChatCore.Interfaces;
using SpotifyAPI.Web;
using TuneSaber.Core.Spotify;
using BS_Utils;

namespace TuneSaber
{
    class ExclamationSong
    {
        public string Usernamequestionmark = "";
        public string channel = Configuration.PluginConfig.Instance.ChannelName;
        //public ChatCoreInstance streamCore = ChatCoreInstance.Create();
        public void Start()
        {
            var streamCore = ChatCoreInstance.Create();
            var streamingService = streamCore.RunAllServices();
            var twitchService = streamingService.GetTwitchService();
            streamingService.OnLogin += StreamingService_OnLogin;
            streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;

        }
        public void StreamingService_OnLogin(IChatService svc)
        {
            if (svc is ChatCore.Services.Twitch.TwitchService twitchService)
            {
                twitchService.JoinChannel(channel);
                //twitchService.SendTextMessage("vruh", channel);
            }
        }
        public void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
        {
            if (svc is ChatCore.Services.Twitch.TwitchService twitchService)
            {
                if (msg.Message.Contains(Configuration.PluginConfig.Instance.SongChatCommand))
                {
                    if (msg.Message.Substring(0, Configuration.PluginConfig.Instance.SongChatCommand.Length).Equals(Configuration.PluginConfig.Instance.SongChatCommand))
                    {
                        string r = GetDatMFSong().Result;
                        twitchService.SendTextMessage(r, channel);
                    }
                }
                if (msg.Message.Contains(Configuration.PluginConfig.Instance.PlaylistChatCommand))
                {
                    if (msg.Message.Substring(0, Configuration.PluginConfig.Instance.PlaylistChatCommand.Length).Equals(Configuration.PluginConfig.Instance.PlaylistChatCommand))
                    {
                        string r = GetDatMFPlaylist().Result;
                        twitchService.SendTextMessage(r, channel);
                    }
                }
            }
        }
        public async Task<string> GetDatMFSong()
        {
            string title = SearchMenu.currentTitle;
            string artist = SearchMenu.currentArtist;
            string URL = SearchMenu.currentURL;
            if(SearchMenu.currentTitle.Equals("nopeee"))
            {
                return "The current song is not on spotify.";
            }
            else
            {
                string Response = "/me is currently playing: %T by: %A, %U";
                Response = Response.Replace("%T", title);
                Response = Response.Replace("%A", artist);
                Response = Response.Replace("%U", URL);
                return Response;
            }
            

        }
        public async Task<string> GetDatMFPlaylist()
        {
            var playlist = Interaction.playlists.ElementAt(Configuration.PluginConfig.Instance.ChosenPlaylist);

            if ((bool)(playlist.Public = true))
            {
                string Response = "/me is currently using the playlist %N, created by %O, which has %T items";
                Response.Replace("%N", playlist.Name);
                Response.Replace("%O", playlist.Owner.DisplayName);
                Response.Replace("%T", playlist.Tracks.Items.Count.ToString());
                return Response;
            }
            else
            {
                return "The current playlist is private.";
            }
        }
    }
}
