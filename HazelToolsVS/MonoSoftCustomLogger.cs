using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.Debugging.Client;

namespace HazelToolsVS
{
	internal class MonoSoftCustomLogger : ICustomLogger
	{

		private readonly IServiceProvider m_ServiceProvider;

		internal MonoSoftCustomLogger(IServiceProvider serviceProvider)
		{
			m_ServiceProvider = serviceProvider;
		}

		public string GetNewDebuggerLogFilename()
		{
			return "Log.txt";
		}

		public void LogAndShowException(string message, Exception ex)
		{
			LogError(message, ex);
		}

		public void LogError(string message, Exception ex)
		{
			string msg = message + (ex != null ? Environment.NewLine + ex.ToString() : string.Empty);
			VsShellUtilities.ShowMessageBox(
				m_ServiceProvider,
				msg,
				"Hazel Tools Error",
				OLEMSGICON.OLEMSGICON_CRITICAL,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}

		public void LogMessage(string messageFormat, params object[] args)
		{
			string msg = string.Format(messageFormat, args);
			VsShellUtilities.ShowMessageBox(
				m_ServiceProvider,
				msg,
				"Hazel Tools Info",
				OLEMSGICON.OLEMSGICON_INFO,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}
	}
}
