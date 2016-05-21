using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace Serial_Monitor
{
    internal sealed class SerialMonitorCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("edf12506-fbfd-4d38-99e4-f64c7ac9d468");

        private readonly Package package;

        private SerialMonitorCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static SerialMonitorCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new SerialMonitorCommand(package);
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            ToolWindowPane window = this.package.FindToolWindow(typeof(SerialMonitor), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
