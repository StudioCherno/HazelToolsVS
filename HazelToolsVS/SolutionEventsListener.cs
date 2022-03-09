using System;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace HazelToolsVS
{
	internal abstract class SolutionEventsListener : IVsSolutionEvents, IDisposable
	{
		private static volatile object m_DisposalLock = new object();
		private uint m_EventsCookie;
		private bool m_Disposed;

		protected SolutionEventsListener(IServiceProvider serviceProvider)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Solution = ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
			Assumes.Present(Solution);
		}

		protected IVsSolution Solution { get; }
		protected IServiceProvider ServiceProvider { get; }

		public void Init()
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			ErrorHandler.ThrowOnFailure(Solution.AdviseSolutionEvents(this, out m_EventsCookie));
		}

		public virtual int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => VSConstants.E_NOTIMPL;
		public virtual int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.E_NOTIMPL;
		public virtual int OnBeforeCloseSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;
		public virtual int OnAfterCloseSolution(object pUnkReserved) => VSConstants.E_NOTIMPL;
		public virtual int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => VSConstants.E_NOTIMPL;
		public virtual int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => VSConstants.E_NOTIMPL;
		public virtual int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => VSConstants.E_NOTIMPL;
		public virtual int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.E_NOTIMPL;
		public virtual int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.E_NOTIMPL;
		public virtual int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.E_NOTIMPL;

		public void Dispose()
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

			if (m_Disposed)
				return;

			lock (m_DisposalLock)
			{
				if (disposing && m_EventsCookie != 0U && Solution != null)
				{
					Solution.UnadviseSolutionEvents(m_EventsCookie);
					m_EventsCookie = 0U;
				}

				m_Disposed = true;
			}
		}
	}
}
