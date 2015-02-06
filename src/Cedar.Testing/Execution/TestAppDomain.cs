namespace Cedar.Testing.Execution
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    public static class TestAppDomain
    {
        public static AppDomain Create(string assemblyPath)
        {
            var appDomain = AppDomain.CreateDomain(assemblyPath,
                AppDomain.CurrentDomain.Evidence,
                new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(assemblyPath)
                },
                new PermissionSet(PermissionState.Unrestricted));

            appDomain.UnhandledException += (_, e) => { Console.WriteLine(e.ExceptionObject); };

            return appDomain;
        }
    }
}