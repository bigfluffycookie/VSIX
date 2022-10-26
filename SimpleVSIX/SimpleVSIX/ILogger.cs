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
        internal static readonly Guid PaneId = new Guid("A2BA99A1-99DD-47DD-9E21-579A738C7BA6");
        private IVsOutputWindow outputWindow;
        private IVsOutputWindowPane pane;

        [ImportingConstructor]
        public Logger([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            outputWindow = serviceProvider.GetService(typeof(IVsOutputWindow)) as IVsOutputWindow;
            CreatePane();
        }

        private void CreatePane()
        {
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