namespace Serial_Monitor
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Runtime.InteropServices;

    [Guid("decfc908-9657-44ef-beea-8eecc6efceab")]
    public class SerialMonitor : ToolWindowPane
    {
        public SerialMonitor() : base(null)
        {
            this.Caption = "Serial Monitor";
            this.Content = new SerialMonitorControl();
        }
    }
}
