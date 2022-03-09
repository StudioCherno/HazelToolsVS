using System;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;

namespace HazelToolsVS.Debugging
{
	internal class HazelDebuggerSession : SoftDebuggerSession
	{
		private bool m_IsAttached;

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
				base.OnRun(startInfo);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(hazelStartInfo.SessionType.ToString());
			}
		}

		protected override void OnExit()
		{
			if (m_IsAttached)
			{
				base.OnDetach();
			}
			else
			{
				base.OnExit();
			}
		}
	}
}
