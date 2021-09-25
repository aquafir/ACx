using Decal.Adapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ACx.Lib
{
    static class Utils
    {
        #region Decal Helpers
        /// <summary>
        /// Attempts to write a message to the ingame chat window.  If there is no character logged in
        /// this will fail.
        /// </summary>
        /// <param name="cmd"></param>
        public static void WriteToChat(string cmd)
        {
            try
            {
                CoreManager.Current.Actions.AddChatText(cmd, 5);
            }
            catch { }
        }
        #endregion

        /// <summary>
        /// Returns true if a character is logged in
        /// </summary>
        /// <returns></returns>
        public static bool IsLoggedIn()
        {
            try
            {
                return CoreManager.Current.CharacterFilter.LoginStatus > 0;
            }
            catch { }

            return false;
        }

        #region Logging
        public static string LogPath { get { return Path.Combine(ACx.PluginAssemblyDirectory, "Exceptions.txt"); } }

        /// <summary>
        /// Logs an exception to exceptions.txt file next to the plugin dll
        /// </summary>
        /// <param name="ex">exception to log</param>
        public static void LogException(Exception ex)
        {
            try
            {
                //WriteLog(ex.ToString() + "\n");
                //Mag-Tools style-logs.
                var writer = new StringBuilder();

                writer.AppendLine("=================================Loader=====================================");
                writer.AppendLine(DateTime.Now.ToString());
                writer.AppendLine("Error: " + ex.Message);
                writer.AppendLine("Source: " + ex.Source);
                writer.AppendLine("Stack: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    writer.AppendLine("Inner: " + ex.InnerException.Message);
                    writer.AppendLine("Inner Stack: " + ex.InnerException.StackTrace);
                }
                writer.AppendLine("============================================================================");
                writer.AppendLine("\r\n");

                WriteLog(writer.ToString());
            }
            catch { }
        }

        public static void WriteLog(string message)
        {
            try
            {
                File.AppendAllText(LogPath, message);
            }
            catch { }
        }
        #endregion
    }
}
