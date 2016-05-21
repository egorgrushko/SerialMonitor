using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Serial_Monitor
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SerialMonitor))]
    [Guid(SerialMonitorPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class SerialMonitorPackage : Package
    {
        public const string PackageGuidString = "cba4853a-1853-4eae-a437-9572728a1be5";

        public SerialMonitorPackage()
        {
        }

        #region Package Members

        protected override void Initialize()
        {
            SerialMonitorCommand.Initialize(this);
            base.Initialize();
        }

        #endregion
    }
}
