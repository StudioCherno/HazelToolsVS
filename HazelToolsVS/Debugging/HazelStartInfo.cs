using EnvDTE;
using Mono.Debugging.Soft;
using Mono.Debugging.VisualStudio;

namespace HazelToolsVS.Debugging
{
    public enum HazelSessionType
    {
        PlayInEditor = 0,
        AttachHazelnutDebugger
    }

    internal class HazelStartInfo : StartInfo
    {
        public readonly HazelSessionType SessionType;

        public HazelStartInfo(SoftDebuggerStartArgs args, DebuggingOptions options, Project startupProject, HazelSessionType sessionType)
            : base(args, options, startupProject)
        {
            SessionType = sessionType;
        }
    }
}
