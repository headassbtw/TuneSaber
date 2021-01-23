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
            SearchMenu.instance.AddTab();
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {

            var es = new ExclamationSong();
            es.Start();
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
