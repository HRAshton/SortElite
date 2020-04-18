using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SortElite
{
    public class IconGrouper
    {
        private const string DefaultStartMenuProgramsPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";
        private const string DefaultConfigPath = "config.json";

        public Dictionary<string, FileInfo[]> SortShortcuts()
        {
            var rawShortcuts = CollectIcons();

        }

        private static List<FileInfo> CollectIcons(DirectoryInfo directoryInfo = null)
        {
            if (directoryInfo == null)
            {
                directoryInfo = new DirectoryInfo(DefaultStartMenuProgramsPath);
            }

            var rootFiles = directoryInfo.GetFiles();
            var subfoldersFiles = directoryInfo.GetDirectories()
                .AsParallel()
                .Select(CollectIcons)
                .SelectMany(fileInfos => fileInfos);

            return rootFiles.Concat(subfoldersFiles).ToList();
        }
    }
}
