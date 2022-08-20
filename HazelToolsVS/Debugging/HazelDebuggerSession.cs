using System;
using System.ComponentModel.Design;
using System.IO;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using Mono.Debugger.Soft;

namespace HazelToolsVS.Debugging
{
	internal class HazelDebuggerSession : SoftDebuggerSession
	{
		private bool m_IsAttached;
		private MenuCommand m_AttachToHazelnutMenuItem;

		/*internal HazelDebuggerSession(MenuCommand attachToHazelnutMenuItem)
		{
			m_AttachToHazelnutMenuItem = attachToHazelnutMenuItem;
		}*/

		protected override void OnRun(DebuggerStartInfo startInfo)
		{
			var hazelStartInfo = startInfo as HazelStartInfo;

			switch (hazelStartInfo.SessionType)
			{
			case HazelSessionType.PlayInEditor:
				break;
			case HazelSessionType.AttachHazelnutDebugger:
			{
				m_IsAttached = true;
				base.OnRun(hazelStartInfo);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(hazelStartInfo.SessionType.ToString());
			}
		}

		protected override void OnConnectionError(Exception ex)
		{
			// The session was manually terminated
			if (HasExited)
			{
				base.OnConnectionError(ex);
				return;
			}

			if (ex is VMDisconnectedException || ex is IOException)
			{
				HasExited = true;
				base.OnConnectionError(ex);
				return;
			}

			string message = "An error occured when trying to attach to Hazelnut. Please make sure that Hazelnut is running and that it's up-to-date.";
			message += Environment.NewLine;
			message += string.Format("Message: {0}", ex != null ? ex.Message : "No error message provided.");

			if (ex != null)
			{
				message += Environment.NewLine;
				message += string.Format("Source: {0}", ex.Source);
				message += Environment.NewLine;
				message += string.Format("Stack Trace: {0}", ex.StackTrace);

				if (ex.InnerException != null)
				{
					message += Environment.NewLine;
					message += string.Format("Inner Exception: {0}", ex.InnerException.ToString());
				}
			}
			
			_ = HazelToolsPackage.Instance.ShowErrorMessageBoxAsync("Connection Error", message);
			base.OnConnectionError(ex);
		}

		protected override void OnExit()
		{
			if (m_IsAttached)
			{
				m_IsAttached = false;
				base.OnDetach();
			}
			else
			{
				base.OnExit();
			}
		}
	}
}
