using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.RpcContracts.Commands;
using Task = System.Threading.Tasks.Task;

namespace SimpleVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CommandOutputWindow
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7a9fcb09-91b7-4b18-acf2-7a320e74d4ff");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;


        private IVsOutputWindowPane pane;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandOutputWindow"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CommandOutputWindow(AsyncPackage package, OleMenuCommandService commandService)
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
        public static CommandOutputWindow Instance
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
            // Switch to the main thread - the call to AddCommand in Command1's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CommandOutputWindow(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            if (pane == null)
            {
                pane = CreatePane(Guid.NewGuid(), "Step 5", true, false);
            }

            UpdatePane();
        }

        IVsOutputWindowPane CreatePane(Guid paneGuid, string title,
                        bool visible, bool clearWithSolution)
        {
            var output = package.GetService<SVsOutputWindow, IVsOutputWindow>();

            // Create a new pane.
            output.CreatePane(
                ref paneGuid,
                title,
                Convert.ToInt32(visible),
                Convert.ToInt32(clearWithSolution));

            // Retrieve the new pane.
            output.GetPane(ref paneGuid, out pane);

            return pane;
        }

        void UpdatePane()
        {
            pane.OutputStringThreadSafe("Hello!!\n");
        }
    }
}
