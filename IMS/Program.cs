namespace IMS
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var log = new IMS.Services.ApplicationLog();
            log.WriteAsync("Information", "ApplicationStarted", "Archive Auditor started.").GetAwaiter().GetResult();
            Application.Run(new Form1());
            log.WriteAsync("Information", "ApplicationStopped", "Archive Auditor stopped.").GetAwaiter().GetResult();
        }
    }
}
