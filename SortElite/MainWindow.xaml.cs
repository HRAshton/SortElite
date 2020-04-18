using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;
using SortElite.Models;

namespace SortElite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<FileInfo> shortcuts = new List<FileInfo>();

        public MainWindow()
        {
            InitializeComponent();

            ConfigurationModel config;
            try
            {
                config = JsonConvert.DeserializeObject<ConfigurationModel>(File.ReadAllText("cfg.json"));
            }
            catch
            {
                config = new ConfigurationModel();
            }

            CollectIcons(new DirectoryInfo());
            Cfg.Text = CompileConfig(config);
            Button_Click(null, null);
        }

        private void CollectIcons(DirectoryInfo directoryInfo)
        {
            shortcuts.AddRange(directoryInfo.GetFiles());

            directoryInfo.GetDirectories().ToList().ForEach(CollectIcons);
        }

        private void Button_Click(object _1, RoutedEventArgs _2)
        {
            ParseConfig();

            var sortedIcons = SortIcons();

            Preview.Clear();
            Preview.AppendText(CompilePreview(sortedIcons));
        }

        private Dictionary<string, List<FileInfo>> SortIcons()
        {
            var sortedIcons = new Dictionary<string, List<FileInfo>>();
            var ungrouped = shortcuts.ToList();

            sortedIcons.Add("unsorted", new List<FileInfo>());

            foreach (var cfg in config)
            {
                var folder = cfg.Key;
                if (!sortedIcons.ContainsKey(folder))
                {
                    sortedIcons.Add(folder, new List<FileInfo>());
                }

                foreach (var rule in cfg.Value)
                {
                    var regex = new Regex(rule,
                        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

                    foreach (var icon in shortcuts)
                    {
                        if (!regex.IsMatch(icon.FullName))
                            continue;

                        sortedIcons[folder].Add(icon);
                        ungrouped.RemoveAll(x => string.Compare(x.Name, icon.Name, StringComparison.OrdinalIgnoreCase) == 0);
                    }
                }
            }

            sortedIcons["unsorted"].AddRange(ungrouped);

            return sortedIcons;
        }

        private void ParseConfig()
        {
            config.Clear();

            var currentFolder = "";
            foreach (var line in Cfg.Text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith('['))
                {
                    currentFolder = line.TrimStart('[').TrimEnd(']');
                    config.Add(currentFolder, new List<string>());
                }
                else
                {
                    config[currentFolder].Add(line);
                }
            }

            File.WriteAllText("cfg.json", JsonConvert.SerializeObject(config));
        }

        private string CompilePreview(Dictionary<string, List<FileInfo>> preview)
        {
            var n = preview.ToDictionary(p => p.Key,
                p => p.Value.Select(x => x.Name).ToList());
            return CompileConfig(n);
        }

        private string CompileConfig(Dictionary<string, List<string>> config)
        {
            var str = "";
            foreach (var f in config)
            {
                str += ($"[{f.Key}]\r\n");

                foreach (var l in f.Value)
                {
                    str += (l + "\r\n");
                }

                str += ("\r\n");
            }

            return str;
        }

        private void Compile(object sender, RoutedEventArgs e)
        {
            ParseConfig();
            var sortedIcons = SortIcons();

            Directory.Delete("Cookied", true);
            var root = Directory.CreateDirectory("Cookied");

            foreach (var group in sortedIcons)
            {
                if (group.Key == "removed")
                {
                    continue;
                }

                if (group.Key == "unsorted")
                {
                    foreach (var icon in group.Value)
                    {
                        icon.CopyTo(Path.Combine(root.FullName, icon.Name), true);
                    }
                    continue;
                }


                var folder = root.CreateSubdirectory(group.Key);
                
                foreach (var icon in group.Value)
                {
                    icon.CopyTo(Path.Combine(folder.FullName, icon.Name), true);
                }
            }
        }
    }
}
