
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace TuneSaber.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual string TwitchAdminLevel { get; set; } = "Mod";
        public virtual string ChannelName { get; set; } = "";
        public virtual string PlaylistID { get; set; } = "";
        
        public virtual string SongChatCommand { get; set; } = "!song";
        public virtual string SongCommandReponse { get; set; } = "/me is currently playing: %T by: %A, %U";
        public virtual bool SongCommandEnabled { get; set; } = true;
        public virtual string SongCommandPerm { get; set; } = "Mod";
        public virtual string PlaylistChatCommand { get; set; } = "!playlist";
        public virtual string PlaylistCommandReponse { get; set; } = "/me is currently using the playlist %N, created by %O, which has %T items";
        public virtual bool PlaylistCommandEnabled { get; set; } = true;
        public virtual string PlaylistCommandPerm { get; set; } = "Mod";
        public virtual bool CurrentlyPollingPlayback { get; set; } = false;
        public virtual int ChosenPlatform { get; set; } = 0;
        public virtual int ChosenPlaylist { get; set; } = 1;
        public virtual bool UsingMultiExtentions { get; set; } = false;
        public virtual bool UsingChatCore { get; set; } = false;

        /*
        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }*/
    }
}
