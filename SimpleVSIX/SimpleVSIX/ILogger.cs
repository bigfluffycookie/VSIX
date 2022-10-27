using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;

namespace SimpleVSIX
{
    internal interface ILogger
    {
        void Log(string message);
    }

    [Export(typeof(ILogger))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class Logger : ILogger
    {
        private IVsOutputWindowPane pane;

        internal static readonly Guid PaneId = new Guid("B43B5B29-61EE-4BCF-8C41-6065C5ECF602");

        [ImportingConstructor]
        public Logger([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var outputWindow = serviceProvider.GetService(typeof(IVsOutputWindow)) as IVsOutputWindow;
            CreatePane(outputWindow);
        }

        private void CreatePane(IVsOutputWindow outputWindow)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var guid = PaneId;

            // Create a new pane.
            outputWindow.CreatePane(ref guid, "Step 5", Convert.ToInt32(true), Convert.ToInt32(false));

            // Retrieve the new pane.
            outputWindow.GetPane(ref guid, out pane);
        }

        public void Log(string message)
        {
            pane.OutputStringThreadSafe(message);
        }
    }
}