using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPA.Utilities.Async;
using IPALogger = IPA.Logging.Logger;
using TuneSaber;

namespace TuneSaber
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("TuneSaber initialized.");
            
            //Auth.Main();
            //Log.Info("tried to start main.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        [Init]
        public void InitWithConfig(Config conf)
        {
            
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            SearchMenu.instance.AddTab();
            Log.Debug("Config loaded");

            
        }
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            //checking to see if multiplayer extentions is installed, in order to use the utilities it has
            var plugins = IPA.Loader.PluginManager.EnabledPlugins.ToList();
            try
            {
                Configuration.PluginConfig.Instance.UsingMultiExtentions = false;
                Configuration.PluginConfig.Instance.UsingChatCore = false;
                foreach (var plugin in plugins)
                {
                    if(!Configuration.PluginConfig.Instance.UsingMultiExtentions)
                    {
                        switch (plugin.Name == "MultiplayerExtensions")
                        {
                            case true:
                                Configuration.PluginConfig.Instance.UsingMultiExtentions = true;
                                Log.Debug("MultiplayerExtentions Found");
                                break;
                            case false:
                                Configuration.PluginConfig.Instance.UsingMultiExtentions = false;
                                break;
                        }
                    }
                    
                    if (!Configuration.PluginConfig.Instance.UsingChatCore)
                    {
                        switch (plugin.Name == "ChatCore")
                        {
                            case true:
                                Configuration.PluginConfig.Instance.UsingChatCore = true;
                                var es = new ExclamationSong();
                                es.Start();
                                Log.Debug("ChatCore Found");
                                break;
                            case false:
                                Configuration.PluginConfig.Instance.UsingChatCore = false;
                                break;
                        }
                    }
                    
                }
            }
            catch (Exception e) { Log.Critical("Optional Depencency Detection Error: " + e.ToString()); }
            SearchMenu.instance.AddTab();


            Views.UICreator.CreateMenu();
            BS_Utils.Utilities.BSEvents.OnLoad();
            Log.Debug("OnApplicationStart");
            new GameObject("TuneSaberController").AddComponent<TuneSaberController>();

        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
            SearchMenu.instance.RemoveTab();

        }
    }
}
