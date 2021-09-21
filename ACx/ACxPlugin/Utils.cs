using Decal.Adapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACxPlugin
{
	public static class Utils
	{
		public static string AssemblyDirectory { get; set; } = "";
		public static string LogPath { get { return Path.Combine(AssemblyDirectory, "Exceptions.txt"); } }

		public static void LogError(Exception ex)
		{
			//LogError(ex.ToString() + "\n");
			//Mag-Tools approach.  Prefer errors in plugin folder
			var writer = new StringBuilder();

			writer.AppendLine("=================================Plugin====================================="); 
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

			LogError(writer.ToString());

		}

		private static void LogError(string message)
		{
			try
			{
				File.AppendAllText(LogPath, message);
			}
			catch { }
		}

		public static void WriteToChat(string message)
		{
			try
			{
				CoreManager.Current.Actions.AddChatText("<ACx>>: " + message, 5);
			}
			catch (Exception ex) {  }
		}

		internal static void Mexec(string command)
		{
			try
			{
				DecalProxy.DispatchChatToBoxWithPluginIntercept($"/ub mexec {command}");
			}
			catch (Exception ex) { LogError(ex); }
		}

		internal static void Command(string command)
		{
			try
			{
				DecalProxy.DispatchChatToBoxWithPluginIntercept(command);
			}
			catch (Exception ex) { LogError(ex); }
		}

		/// <summary>
		/// Just copy method over from PluginLoader so I don't need to reference it
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
	}
}
