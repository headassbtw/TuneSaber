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
        public ChatCore.Services.Twitch.TwitchService twitchService;
        //public ChatCoreInstance streamCore = ChatCoreInstance.Create();
        public void Start()
        {
            var streamCore = ChatCoreInstance.Create();
            var streamingService = streamCore.RunAllServices();
            twitchService = streamingService.GetTwitchService();
            streamingService.OnLogin += StreamingService_OnLogin;
            streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;

        }
        public void StreamingService_OnLogin(IChatService svc)
        {
            if (svc is ChatCore.Services.Twitch.TwitchService twitchService)
            {
                twitchService.JoinChannel(channel);
				twitchService.SendTextMessage("TuneSaber 1.1.0 alpha, unexpectedly-bonks-you edition, " + twitchService.DisplayName + twitchService.LoggedInUser.DisplayName, channel);
                //twitchService.SendTextMessage("vruh", channel);
            }
        }
        public void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
        {
            if (svc is ChatCore.Services.Twitch.TwitchService twitchService)
            {
                if (msg.Message.Contains(Configuration.PluginConfig.Instance.SongChatCommand) && msg.Message.Substring(0, Configuration.PluginConfig.Instance.SongChatCommand.Length).Equals(Configuration.PluginConfig.Instance.SongChatCommand))
                {
                    switch (Configuration.PluginConfig.Instance.SongCommandEnabled)
                    {
                        case true:
                            switch (HasPerms(msg.Sender, 1))
                            {
                                case true:
                                    string r = GetDatMFSong().Result;
                                    twitchService.SendTextMessage(r, channel);
                                    break;
                                case false:
                                    twitchService.SendTextMessage("You do not have permission to perform that command, the current permission level is " + Configuration.PluginConfig.Instance.SongCommandPerm + ".", channel);
                                    break;
                            }
                            break;
                        case false:
                            twitchService.SendTextMessage("That command is currently disabled.", channel);
                            break;
                    }
                }
                if (msg.Message.Contains(Configuration.PluginConfig.Instance.PlaylistChatCommand) && msg.Message.Substring(0, Configuration.PluginConfig.Instance.PlaylistChatCommand.Length).Equals(Configuration.PluginConfig.Instance.PlaylistChatCommand))
                {
                    switch (Configuration.PluginConfig.Instance.PlaylistCommandEnabled)
                    {
                        case true:
                            switch (HasPerms(msg.Sender, 2))
                            {
                                case true:
                                    string r = GetDatMFPlaylist().Result;
                                    twitchService.SendTextMessage(r, channel);
                                    break;
                                case false:
                                    twitchService.SendTextMessage("You do not have permission to perform that command, the current permission level is " + Configuration.PluginConfig.Instance.PlaylistCommandPerm + ".", channel);
                                    break;
                            }
                            break;
                        case false:
                            twitchService.SendTextMessage("That command is currently disabled.", channel);
                            break;
                    }   
                }
                if(msg.Message.Contains("!tscommand") && msg.Message.Substring(0, 10).Equals("!tscommand"))
                {
                    

                    string mess = msg.Message.Substring(11);
                    if (HasPerms(msg.Sender, 0))
                    {



                        if (msg.Message.Contains(" song "))
                        {
                            if (msg.Message.Contains(" on"))
                            {
                                Configuration.PluginConfig.Instance.SongCommandEnabled = true;
                                twitchService.SendTextMessage("Song command is now on.", channel);
                            }
                            if (msg.Message.Contains(" off"))
                            {
                                Configuration.PluginConfig.Instance.SongCommandEnabled = false;
                                twitchService.SendTextMessage("Song command is now off.", channel);
                            }
                        }
                        if (msg.Message.Contains(" playlist "))
                        {
                            if (msg.Message.Contains(" on"))
                            {
                                Configuration.PluginConfig.Instance.PlaylistCommandEnabled = true;
                                twitchService.SendTextMessage("Playlist command is now on.", channel);
                            }
                            if (msg.Message.Contains(" off"))
                            {
                                Configuration.PluginConfig.Instance.PlaylistCommandEnabled = false;
                                twitchService.SendTextMessage("Playlist command is now off.", channel);
                            }
                        }
                    }
                    else
                    {
                        twitchService.SendTextMessage("You do not have permission to perform that command, the current permission level is " + Configuration.PluginConfig.Instance.TwitchAdminLevel + ".", channel);
                    }
                }

                if(msg.Message.Contains("!tsadmin") && msg.Message.Substring(0, 8).Equals("!tsadmin"))
                {
                    if (msg.Sender.IsBroadcaster)
                    {
                        if (msg.Message.Contains("!tsadmin permissionlevel") && msg.Message.Substring(0, 24).Equals("!tsadmin permissionlevel"))
                        {
                            switch (msg.Message.Substring(25))
                            {
                                case "broadcaster":
                                    Configuration.PluginConfig.Instance.TwitchAdminLevel = "broadcaster";
                                    twitchService.SendTextMessage("Only the broadcaster can toggle commands now.", channel);
                                    break;
                                case "mod":
                                    twitchService.SendTextMessage("Only mods can toggle commands now.", channel);
                                    Configuration.PluginConfig.Instance.TwitchAdminLevel = "mod";
                                    break;
                                case "vip+":
                                    twitchService.SendTextMessage("VIPs and above can toggle commands now.", channel);
                                    Configuration.PluginConfig.Instance.TwitchAdminLevel = "vip+";
                                    break;
                                case "vip":
                                    twitchService.SendTextMessage("VIPs can toggle commands now.", channel);
                                    Configuration.PluginConfig.Instance.TwitchAdminLevel = "vip";
                                    break;
                                case "everyone":
                                    twitchService.SendTextMessage("Everyone can toggle commands! Let the chaos begin!", channel);
                                    Configuration.PluginConfig.Instance.TwitchAdminLevel = "everyone";
                                    break;
                                
                            }
                        }
                    }
                    else { twitchService.SendTextMessage("You do not have permission to perform that command, that command is only usable by the broadcaster", channel); }
                }
            }
        }
        public bool HasPerms(IChatUser user, int action)
        {
            //0 is admin, 1 is song, 2 is playlist
            switch (action)
            {
                case 0:
                    if (user.IsBroadcaster) { return true; }
                    switch (Configuration.PluginConfig.Instance.TwitchAdminLevel)
                    {
                        case "Mod":
                            if (user.IsModerator || user.IsBroadcaster) { return true; }
                            else { return false; }
                        case "Broadcaster":
                            if (user.IsBroadcaster) { return true; }
                            else { return false; }
                        case "VIP+":
                            if (user.IsModerator || IsVip(user)) { return true; }
                            else { return false; }
                        case "VIP":
                            return IsVip(user);
                        case "Everyone":
                            return true;
                    }
                    return false;
                case 1:
                    if (user.IsBroadcaster) { return true; }
                    switch (Configuration.PluginConfig.Instance.SongCommandPerm)
                    {
                        case "Mod":
                            if (user.IsModerator || user.IsBroadcaster) { return true; }
                            else { return false; }
                        case "Broadcaster":
                            if (user.IsBroadcaster) { return true; }
                            else { return false; }
                        case "VIP+":
                            if (user.IsModerator || IsVip(user)) { return true; }
                            else { return false; }
                        case "VIP":
                            return IsVip(user);
                        case "Everyone":
                            return true;
                    }
                    return false;
                case 2:
                    if (user.IsBroadcaster) { return true; }
                    switch (Configuration.PluginConfig.Instance.PlaylistCommandPerm)
                    {
                        case "Mod":
                            if (user.IsModerator || user.IsBroadcaster) { return true; }
                            else { return false; }
                        case "Broadcaster":
                            if (user.IsBroadcaster) { return true; }
                            else { return false; }
                        case "VIP+":
                            if (user.IsModerator || IsVip(user)) { return true; }
                            else { return false; }
                        case "VIP":
                            return IsVip(user);
                        case "Everyone":
                            return true;
                    }
                    return false;
            }
            return false;
        }
        public async Task<string> GetDatMFSong()
        {
            string title = SearchMenu.currentTitle;
            string artist = SearchMenu.currentArtist;
            string URL = SearchMenu.currentURL;
            if(SearchMenu.currentTitle.Equals("sdaijfmchiasoduvbfiusadfv"))
            {
                return "The current song is not on spotify.";
            }
            else
            {
                string Response = Configuration.PluginConfig.Instance.SongCommandReponse;
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
                string Response = Configuration.PluginConfig.Instance.PlaylistCommandReponse;
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
        public bool IsVip(IChatUser user)
        {
            foreach(IChatBadge badge in user.Badges)
            {
                if (badge.Id.Equals("TwitchGlobalBadge_vip1"))
                {
                    return true;
                }
            }
            return false;
        }
        public async Task SendErrorMessage(string type, string error)
        {
            twitchService.SendTextMessage(type + " Error: " + error, channel);
        }
    }
}
