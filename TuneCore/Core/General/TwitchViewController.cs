using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.XR;
using BS_Utils.Utilities;
using IPA.Config.Data;

namespace TuneSaber.ViewControllers
{
    class TwitchViewController : BSMLResourceViewController
    {
        public override string ResourceName => "TuneSaber.Core.General.Twitch.bsml";
        /*
        [UIValue("channel-text")] public string channelText = Configuration.PluginConfig.Instance.ChannelName;


        [UIValue("song-command-enabled")] public bool SongCommandEnabled = Configuration.PluginConfig.Instance.SongCommandEnabled;
        [UIValue("song-command")] public string SongCommand = Configuration.PluginConfig.Instance.SongChatCommand;
        [UIValue("playlist-command-enabled")] public bool PlaylistCommandEnabled = Configuration.PluginConfig.Instance.PlaylistCommandEnabled;
        [UIValue("playlist-command")] public string PlaylistcommandText = Configuration.PluginConfig.Instance.PlaylistChatCommand;
        
        [UIAction("channel-change")]
        public void ChannelChange()
        {
            
            Configuration.PluginConfig.Instance.ChannelName = channelText;
        }
        [UIAction("song-command-change")]
        public void SongCommandChange()
        {
            Configuration.PluginConfig.Instance.SongChatCommand = SongCommand;
        }
        [UIAction("song-command-toggled")]
        public void SongCommandToggle()
        {
            Configuration.PluginConfig.Instance.SongCommandEnabled = SongCommandEnabled;
        }*/
        /*[UIAction("playlist-command-change")]
        public void PlaylistCommandChange()
        {
            Configuration.PluginConfig.Instance.PlaylistChatCommand = PlaylistcommandText;
        }
        [UIAction("playlist-command-toggled")]
        public void PlaylistCommandToggle()
        {
            Configuration.PluginConfig.Instance.PlaylistCommandEnabled = PlaylistCommandEnabled;
        }*/
    }
}
