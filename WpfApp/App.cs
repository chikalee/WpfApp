using System;
using System.Diagnostics;
using System.Windows;
using happ = HumanUIBaseApp;
using System.Reflection;

namespace WpfApp
{
    class App
    {
        [STAThread]

        static void Main()

        {
            RhinoInside.Resolver.Initialize();
            Application app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            happ.MainWindow1.assembly = Assembly.GetExecutingAssembly();
            app.StartupUri = new Uri("pack://application:,,,/HumanUIBaseApp;component/MainWindow1.xaml", UriKind.Absolute);
            app.Run();
            Process.GetCurrentProcess().Kill();
        }

        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
            throw new NotImplementedException();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            throw new NotImplementedException();
        }

    }
}

