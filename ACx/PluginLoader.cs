using Decal.Adapter;
using ACx.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ACx
{
    [FriendlyName("ACx")]
    public class ACx : FilterBase
    {
        public string AccountName;
        public string CharacterName;
        public string ServerName;
        public Dictionary<int, string> Characters = new Dictionary<int, string>();

        private object pluginInstance;
        private Assembly pluginAssembly;
        private Type pluginType;
        private FileSystemWatcher pluginWatcher = null;

        private bool pluginsReady = false;
        private bool isLoaded;
		private DateTime lastLoad;
		private readonly double TIME_BETWEEN_PLUGIN_RELOAD = 1000;

		/// <summary>
		/// Namespace of the plugin we want to hot reload
		/// </summary>
		public static string PluginAssemblyNamespace { get { return "ACxPlugin.ACxPlugin"; } }

        /// <summary>
        /// File name of the plugin we want to hot reload
        /// </summary>
        public static string PluginAssemblyName { get { return "ACxPlugin.dll"; } }

        /// <summary>
        /// Assembly directory (contains both loader and plugin dlls)
        /// </summary>
        public static string PluginAssemblyDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Assembly.GetAssembly(typeof(ACx)).Location);
            }
        }

        /// <summary>
        /// Full path to plugin assembly
        /// </summary>
        public string PluginAssemblyPath
        {
            get
            {
                return System.IO.Path.Combine(PluginAssemblyDirectory, PluginAssemblyName);
            }
        }

        #region FilterBase overrides
        /// <summary>
        /// This is called when the filter is started up.  This happens when ac client is first started
        /// </summary>
        protected override void Startup()
        {
            try
            {
                // subscribe to built in decal events
                ServerDispatch += FilterCore_ServerDispatch;
                ClientDispatch += FilterCore_ClientDispatch;
                Core.PluginInitComplete += Core_PluginInitComplete;
                Core.PluginTermComplete += Core_PluginTermComplete;

                // watch the PluginAssemblyName for file changes
                pluginWatcher = new FileSystemWatcher();
                pluginWatcher.Path = PluginAssemblyDirectory;
                pluginWatcher.NotifyFilter = NotifyFilters.LastWrite;
                pluginWatcher.Filter = PluginAssemblyName;
                pluginWatcher.Changed += PluginWatcher_Changed; ;
                pluginWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }

        /// <summary>
        /// This is called when the filter is shut down. This happens once when the game is closing.
        /// </summary>
        protected override void Shutdown()
        {
            try
            {
                Core.PluginInitComplete -= Core_PluginInitComplete;
                Core.PluginTermComplete -= Core_PluginTermComplete;
                UnloadPluginAssembly();
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }
        #endregion

        #region Decal Event Handlers
        /// <summary>
        /// Decal ServerDispatch event handler.  This is called when ac client recieves a network message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterCore_ServerDispatch(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                // see https://acemulator.github.io/protocol/ for protocol documentation
                switch (e.Message.Type)
                {
                    case 0xF658: // LoginCharacterSet S2C
                        AccountName = e.Message.Value<string>("zonename");
                        int characterCount = e.Message.Value<int>("characterCount");
                        MessageStruct characters = e.Message.Struct("characters");
                        Characters.Clear();

                        for (int i = 0; i < characterCount; i++)
                        {
                            int id = characters.Struct(i).Value<int>("character");
                            string name = characters.Struct(i).Value<string>("name");
                            Characters.Add(id, name);
                        }
                        break;

                    case 0xF7E1: // Login_WorldInfo
                        ServerName = e.Message.Value<string>("server");
                        break;
                }
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }

        /// <summary>
        /// Handle decal ClientDispatch event.  This is called when ac client emits a network message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterCore_ClientDispatch(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                if (e.Message.Type == 0xF657)
                { // SendEnterWorld C2S
                    int loginId = Convert.ToInt32(e.Message["character"]);

                    if (Characters.ContainsKey(loginId))
                    {
                        CharacterName = Characters[loginId];
                    }
                }
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }

        private void Core_PluginInitComplete(object sender, EventArgs e)
        {
            try
            {
                pluginsReady = true;
                LoadPluginAssembly();
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }

        private void Core_PluginTermComplete(object sender, EventArgs e)
        {
            try
            {
                pluginsReady = false;
                UnloadPluginAssembly();
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }
        #endregion

        #region Plugin Loading/Unloading
        internal void LoadPluginAssembly()
        {
            var timeLapsedLastLoad = DateTime.Now - lastLoad;
            if (timeLapsedLastLoad.TotalMilliseconds < TIME_BETWEEN_PLUGIN_RELOAD)
            {
                //Utils.WriteToChat("Too soon to reload.");
                return;
            }
            lastLoad = DateTime.Now;


            try
            {
                if (!pluginsReady)
                    return;

                if (isLoaded)
                {
                    UnloadPluginAssembly();
                    Utils.WriteToChat($"Reloading {PluginAssemblyName} after {timeLapsedLastLoad.TotalSeconds} seconds.");
                }

                pluginAssembly = Assembly.Load(File.ReadAllBytes(PluginAssemblyPath));
                pluginType = pluginAssembly.GetType(PluginAssemblyNamespace);
                pluginInstance = Activator.CreateInstance(pluginType);

                var startupMethod = pluginType.GetMethod("Startup");
                startupMethod.Invoke(pluginInstance, new object[] {
                    Host,
                    Core,
                    PluginAssemblyDirectory,
                    AccountName,
                    CharacterName,
                    ServerName
                });

                isLoaded = true;
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }

        private void UnloadPluginAssembly()
        {
            try
            {
                if (pluginInstance != null && pluginType != null)
                {
                    MethodInfo shutdownMethod = pluginType.GetMethod("Shutdown");
                    shutdownMethod.Invoke(pluginInstance, null);
                    pluginInstance = null;
                    pluginType = null;
                }
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }

        private void PluginWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                LoadPluginAssembly();
            }
            catch (Exception ex) { Utils.LogException(ex); }
        }
        #endregion
    }
}
