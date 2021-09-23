using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace ACxPlugin
{
    /// <summary>
    /// This is basically a wrapper around PluginLogic that handles the serialization between plugin sessions.
    /// </summary>
    public class ACxPlugin
    {
        PluginLogic pluginLogic;

        /// <summary>
        /// Directory used for storing plugin dll/xml files
        /// </summary>
        public string PluginDirectory { get; private set; }


        /// <summary>
        /// Plugin data file location for serializing PluginLogic instance
        /// </summary>
        public string PluginDatafile
        {
            get { return Path.Combine(PluginDirectory, "PluginData.xml"); }
        }

        #region Startup / Shutdown
        /// <summary>
        /// Called once when the plugin is loaded
        /// </summary>
        public void Startup(NetServiceHost host, CoreManager core, string pluginAssemblyDirectory, string accountName, string characterName, string serverName)
        {
            PluginDirectory = pluginAssemblyDirectory;
            CreateOrRestorePluginLogic();
            pluginLogic.Startup(host, core, pluginAssemblyDirectory, accountName, characterName, serverName);
        }

        /// <summary>
        /// Called when the plugin is shutting down.  Unregister from any events here and do any cleanup.
        /// </summary>
        public void Shutdown()
        {
            pluginLogic?.Shutdown();
            SavePluginLogic();
        }
        #endregion

        /// <summary>
        /// Create a new PluginLogic instance or restore it from serialized XML if available
        /// </summary>
        public void CreateOrRestorePluginLogic()
        {
            if (File.Exists(PluginDatafile))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PluginLogic));
                    using (FileStream fs = new FileStream(PluginDatafile, FileMode.Open))
                    {
                        pluginLogic = (PluginLogic)serializer.Deserialize(fs);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(PluginDirectory, "exceptions.txt"), true))
                    {
                        writer.WriteLine($"{ex}");
                        writer.Close();
                    }
                }
            }

            pluginLogic = new PluginLogic();
        }

        /// <summary>
        /// Serializes PluginLogic instance to xml to be restored on the next session.
        /// </summary>
        private void SavePluginLogic()
        {
            if (pluginLogic == null)
                return;

            XmlSerializer serializer = new XmlSerializer(typeof(PluginLogic));
            using (TextWriter writer = new StreamWriter(PluginDatafile))
            {
                serializer.Serialize(writer, pluginLogic);
                writer.Close();
            }
        }
    }
}
