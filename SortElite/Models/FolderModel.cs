using System.Collections.Generic;
using System.IO;

namespace SortElite.Models
{
    public class FolderModel
    {
        public string Name { get; set; }

        public List<FileInfo> Files { get; set; } = new List<FileInfo>();
    }
}