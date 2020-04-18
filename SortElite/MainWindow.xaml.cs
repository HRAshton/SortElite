using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using SortElite.Models;

namespace SortElite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string configTxtPath = "config.txt";

        private readonly List<FileInfo> shortcuts;

        public MainWindow()
        {
            InitializeComponent();

            shortcuts = SchortcutGrouper.CollectShortcuts();
            var data = File.Exists(configTxtPath) ? File.ReadAllText(configTxtPath) : "";
            var config = SchortcutGrouper.LoadConfig(data);

            Cfg.Text = SchortcutGrouper.Serialize(config);

            Button_Click(null, null);
        }

        private void Button_Click(object _1, RoutedEventArgs _2)
        {
            var config = SchortcutGrouper.LoadConfig(Cfg.Text);
            var foldersModel = SchortcutGrouper.ApplyGrouping(config, shortcuts);
            File.WriteAllText(configTxtPath, Cfg.Text);            

            Preview.Clear();
            Preview.AppendText(DisplayPreview(foldersModel));
        }

        private string DisplayPreview(FoldersModel folders)
        {
            static void ProcessRule(ref List<string> lines, FolderModel folder)
            {
                lines.Add($"[{folder.Name}]");
                lines.AddRange(folder.Files.Select(x => x.Name).OrderBy(x => x));
                lines.Add(string.Empty);
            }

            var lines = new List<string>();
            ProcessRule(ref lines, folders.Unknown);
            ProcessRule(ref lines, folders.Broken);
            foreach (var rule in folders.Folders.OrderBy(x => x.Name))
            {
                ProcessRule(ref lines, rule);
            }

            return string.Join("\r\n", lines);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var config = SchortcutGrouper.LoadConfig(Cfg.Text);
            var foldersModel = SchortcutGrouper.ApplyGrouping(config, shortcuts);

            SchortcutGrouper.CookIntoFolder(foldersModel, new DirectoryInfo("Cooked"));
        }
    }
}
