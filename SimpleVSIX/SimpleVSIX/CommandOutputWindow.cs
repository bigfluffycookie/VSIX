using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.RpcContracts.Commands;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Exception = System.Exception;

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

        private ILogger logger;

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

            try
            {
                var comp = this.package.GetService<SComponentModel, IComponentModel>();
                logger = comp.GetService<ILogger>();
            }
            catch(Exception exception)
            {
                Debug.WriteLine(exception);
            }
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
            logger.Log("Hello\n");
        }
    }
}
