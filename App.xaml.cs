using System.Windows;
using MhwOverlay.UI;
using MhwOverlay.Config;
using MhwOverlay.Log;
using MhwOverlay.Core;

namespace MhwOverlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            Logger.Log(LogLevel.Info,"App started");
            var mainWindow = new MainWindow();
            mainWindow.Show();
            var mainModel = new MainWindowModel(mainWindow);
            mainWindow.SetDataContext(mainModel);
            Logger.MainModel = mainModel;
            if (!AppConfig.Load()){
                Logger.Log(LogLevel.Error,"Config load failed!");
            }
            Logger.Log(LogLevel.Info,"Configs loaded");
            var memoryReader = new MhwMemoryReader();
        }
    }
}
