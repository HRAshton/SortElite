using System.IO;
using System.Windows;

namespace SortElite
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 2)
            {
                var shortcuts = SchortcutGrouper.CollectShortcuts();
                var data = File.ReadAllText(e.Args[0]);
                var config = SchortcutGrouper.LoadConfig(data);
                var foldersModel = SchortcutGrouper.ApplyGrouping(config, shortcuts);

                SchortcutGrouper.CookIntoFolder(foldersModel, new DirectoryInfo(e.Args[1]));
            }
            else
            {
                new MainWindow().ShowDialog();
            }
        }
    }
}