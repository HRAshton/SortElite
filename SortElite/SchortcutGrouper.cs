using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SortElite.Models;
using SortElite.Extensions;

namespace SortElite
{
    public class SchortcutGrouper
    {
        private const string DefaultStartMenuProgramsPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";

        public static List<FileInfo> CollectShortcuts(DirectoryInfo directoryInfo = null)
        {
            directoryInfo ??= new DirectoryInfo(DefaultStartMenuProgramsPath);

            var rootFiles = directoryInfo.GetFiles();
            var subfoldersFiles = directoryInfo.GetDirectories()
                .AsParallel()
                .Select(CollectShortcuts)
                .SelectMany(fileInfos => fileInfos);

            var collectedShortcuts = rootFiles.Concat(subfoldersFiles)
                .GroupBy(x => x.Name)
                .Select(x => x.First()) // Distinct by Name property.
                .ToList();

            return collectedShortcuts;
        }

        public static ConfigurationModel LoadConfig(string serialized)
        {
            var config = new ConfigurationModel();

            var currentRule = new RuleModel();
            foreach (var line in serialized.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith('['))
                {
                    if (currentRule.RegexPatterns.Any())
                    {
                        config.Rules.Add(currentRule);

                        currentRule = new RuleModel();
                    }

                    currentRule.FolderName = line.TrimStart('[').TrimEnd(']');
                }
                else
                {
                    currentRule.RegexPatterns.Add(line);
                }
            }

            config.Rules.Add(currentRule);

            return config;
        }

        public static string Serialize(ConfigurationModel configuration)
        {
            var lines = new List<string>();
            foreach (var rule in configuration.Rules)
            {
                lines.Add($"[{rule.FolderName}]");
                lines.AddRange(rule.RegexPatterns);
                lines.Add(string.Empty);
            }

            return string.Join("\r\n", lines);
        }

        public static FoldersModel ApplyGrouping(ConfigurationModel configuration, List<FileInfo> rawShortcuts)
        {
            var filtered = rawShortcuts.ToList();
            var groupedModel = new FoldersModel();

            var removingRule = configuration.Rules.SingleOrDefault(x => x.FolderName == RuleModel.RemoveingGroupName);
            if (removingRule != null)
            {
                foreach (var pattern in removingRule.RegexPatterns)
                {
                    var regex = new Regex(pattern,
                        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

                    var fileInfosToRemove = rawShortcuts.Where(fileInfo => regex.IsMatch(fileInfo.Name));
                    foreach (var fileInfo in fileInfosToRemove)
                    {
                        filtered.Remove(fileInfo);
                        groupedModel.Removed.Files.Add(fileInfo);
                    }
                }
            }


            groupedModel.Broken.Files = filtered.Where(fileInfo =>
                {
                    if (!fileInfo.Extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    var lnkToFile = LnkToFile(fileInfo.FullName);
                    if (lnkToFile == null)
                    {
                        return false; // Cannot check "empty" links.
                    }

                    var result = !File.Exists(lnkToFile);

                    return result;
                })
                .ToList();


            foreach (var rule in configuration.Rules)
            {
                var shortcuts = new List<FileInfo>();

                foreach (var pattern in rule.RegexPatterns)
                {
                    var regex = new Regex(pattern,
                        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

                    var entry = filtered.Where(fileInfo => regex.IsMatch(fileInfo.Name));

                    shortcuts.AddRange(entry);
                }

                groupedModel.Folders.Add(new FolderModel
                {
                    Name = rule.FolderName,
                    Files = shortcuts
                });
            }

            var flatGrouped = groupedModel.Folders
                .SelectMany(x => x.Files)
                .ToArray();
            groupedModel.Unknown = new FolderModel
            {
                Name = "Unknown",
                Files = filtered.Except(flatGrouped).ToList()
            };

            return groupedModel;
        }

        public static void CookIntoFolder(FoldersModel groupedModel, DirectoryInfo targetFolder)
        {
            targetFolder.SilentForceDelete();

            targetFolder.Create();

            foreach (var folderModel in groupedModel.Folders.Concat(new[] { groupedModel.Unknown, groupedModel.Broken }))
            {
                var folder = targetFolder.CreateSubdirectory(folderModel.Name);

                foreach (var icon in folderModel.Files)
                {
                    var destFileName = Path.Combine(folder.FullName, icon.Name);
                    icon.CopyTo(destFileName, true);
                    File.SetAttributes(destFileName, FileAttributes.Normal);
                }
            }
        }


        private static string LnkToFile(string fileLink)
        {
            var path = File.ReadAllText(fileLink)
                .Split("\0", StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length > 5 && x.Contains(":\\"))
                .OrderBy(x => x.Length)
                .LastOrDefault();

            return path;
        }
    }
}