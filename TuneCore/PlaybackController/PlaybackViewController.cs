using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine.UI;
using TuneSaber.Core.Spotify;
using HMUI;
using UnityEngine;
using System.Collections;

namespace TuneSaber.ViewControllers
{
    class PlaybackViewController : BSMLResourceViewController
    {
        public override string ResourceName => "TuneSaber.PlaybackController.PlaybackControls.bsml";
        [UIComponent("rw-image")] public Image RWimage;
        [UIComponent("rw-button")] public Button RWButton;
        [UIComponent("playpause-image")] public Image Pauseimage;
        [UIComponent("pause-button")] public Button PauseButton;
        [UIComponent("ffw-image")] public Image FFWimage;
        [UIComponent("ffw-button")] public Button FFWButton;
        [UIComponent("time-bar")] public BeatSaberMarkupLanguage.Components.Settings.SliderSetting TimeBar;

        [UIComponent("song-title-text")] public TMPro.TextMeshProUGUI SongTitle;
        [UIComponent("song-artist-text")] public TMPro.TextMeshProUGUI SongArtist;
        [UIComponent("left-time-text")] public TMPro.TextMeshProUGUI LeftTimeText;
        [UIComponent("right-time-text")] public TMPro.TextMeshProUGUI RightTimeText;
        bool repeat = true;

        [UIAction("timebar-seek-to")]
        public void TimeBarSeek()
        {
        }



        [UIAction("rw")]
        public async Task Rewind()
        {
            TimeBar.slider.value = 0.10f;
            TimeBar.ApplyValue();
            await Core.Spotify.Interaction.PlaybackControl(2);
        }
        [UIAction("playpause")]
        public async Task PlayPause()
        {

            switch (await Core.Spotify.Interaction.IsPlayingBack())
            {
                case true:
                    TimeBar.interactable = false;
                    await Core.Spotify.Interaction.PlaybackControl(0);
                    Pauseimage.SetImage("TuneSaber.Icons.Pause.png");
                    break;
                case false:
                    await Core.Spotify.Interaction.PlaybackControl(1);
                    Pauseimage.SetImage("TuneSaber.Icons.Play.png");
                    TimeBar.interactable = true;
                    break;
            }
        }
        [UIAction("ffw")]
        public async Task FastForward()
        {
            TimeBar.slider.value = 0.90f;
            TimeBar.ApplyValue();
            await Core.Spotify.Interaction.PlaybackControl(3);
        }
        [UIAction("refresh")]
        public void Refresh()
        {
            repeat = false;
            Task.Delay(100);
            repeat = true;
            _ = gpb();
        }

        [UIAction("#post-parse")]
        public void PostParse()
        {
            TimeBar.interactable = false;

            RWimage.SetImage("TuneSaber.Icons.Rewind.png");
            Pauseimage.SetImage("TuneSaber.Icons.Play.png");
            FFWimage.SetImage("TuneSaber.Icons.FFW.png");
            switch (Interaction.IsPremium)
            {
                case true:
                    break;
                case false:
                    PauseButton.enabled = false;
                    RWButton.enabled = false;
                    FFWButton.enabled = false;
                    break;

            }


            _ = gpb();
        }
        public async Task gpb()
        {
            var pb = await Interaction.GetPlayback();
            var spotify = await Interaction.GetMyFuckingAPIShit();
            float duration = pb.DurationMs / 1000;
            TimeBar.slider.minValue = 0.00f;
            TimeBar.slider.maxValue = duration;

            _ = RefreshName();
            _ = RefreshTime();

        }
        public async Task RefreshName()
        {
            var pb = await Interaction.GetPlayback();
            
            var spotify = await Interaction.GetMyFuckingAPIShit();

            var cpb = await spotify.Player.GetCurrentPlayback();
            SongTitle.text = pb.Name;
            SongArtist.text = pb.Artists.ElementAt(0).Name;
            int refresh1 = pb.DurationMs - cpb.ProgressMs;
            TimeBar.slider.maxValue = pb.DurationMs / 1000;
        }
        public IEnumerator TimeRefresh( SpotifyAPI.Web.FullTrack playback, SpotifyAPI.Web.CurrentlyPlayingContext context)
        {
            bool repeating = true;
            float duration = playback.DurationMs / 1000;
            
            while (repeating)
            {
                float prog = context.ProgressMs / 1000;
                float minutes = (duration - prog).Minutes();
                float seconds = (duration - prog).Seconds();
                string FormattedTime = minutes.ToString() + ":" + seconds.ToString();
                RightTimeText.text = FormattedTime;
                TimeBar.slider.value = prog;
                yield return new WaitForSecondsRealtime(1);
                for(int i = 0; i < 5; i++)
                {
                    prog += 1;
                    minutes = (duration - prog).Minutes();
                    seconds = (duration - prog).Seconds();
                    FormattedTime = minutes.ToString() + ":" + seconds.ToString();
                    RightTimeText.text = FormattedTime;
                    TimeBar.slider.value = prog;
                    yield return new WaitForSecondsRealtime(1);
                }
            }
        }

        public async Task RefreshTime()
        {
            var pb = await Interaction.GetPlayback();
            var spotify = await Interaction.GetMyFuckingAPIShit();
            var cpb = await spotify.Player.GetCurrentPlayback();
            StartCoroutine(TimeRefresh(pb, cpb));
        }

    }
}
