using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using HazelToolsVS.Debugging;
using Microsoft.VisualStudio;
using Mono.Debugging.VisualStudio;
using EnvDTE;
using Mono.Debugging.Soft;
using System.Net;

namespace HazelToolsVS
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class AttachHazelnutCommand
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 256;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("7ccae1a8-81a9-4346-aaa2-8cad5e772749");

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly AsyncPackage package;

		private IVsSolutionBuildManager m_SolutionBuildManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttachHazelnutCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">Command service to add command to, not null.</param>
		private AttachHazelnutCommand(AsyncPackage package, OleMenuCommandService commandService)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static AttachHazelnutCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static async Task InitializeAsync(AsyncPackage package)
		{
			// Switch to the main thread - the call to AddCommand in AttachHazelnutCommand's constructor requires
			// the UI thread.
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new AttachHazelnutCommand(package, commandService);
			Instance.m_SolutionBuildManager = await package.GetServiceAsync(typeof(IVsSolutionBuildManager)) as IVsSolutionBuildManager;
		}

		/// <summary>
		/// This function is the callback used to execute the command when the menu item is clicked.
		/// See the constructor to see how the menu item is associated with this function using
		/// OleMenuCommandService service and MenuCommand class.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event args.</param>
		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			m_SolutionBuildManager.get_StartupProject(out var vsHierarchy);
			vsHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object projectObj);
			Project project = projectObj as Project;

			int port = 2550;

			var startArgs = new SoftDebuggerConnectArgs(project.Name, IPAddress.Parse("127.0.0.1"), port) { MaxConnectionAttempts = 3 };

			var startInfo = new HazelStartInfo(startArgs, null, project, HazelSessionType.AttachHazelnutDebugger)
			{
				WorkingDirectory = HazelToolsPackage.Instance.SolutionEventsListener?.SolutionDirectory
			};

			var session = new HazelDebuggerSession();
			session.Breakpoints.Clear();
			var launcher = new MonoDebuggerLauncher(new Progress<string>());
			launcher.StartSession(startInfo, session);
		}
	}
}
