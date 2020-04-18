using System.Collections.Generic;
using System.IO;

namespace SortElite.Models
{
    public class FoldersModel
    {
        public List<FileInfo> Remove { get; set; }

        public List<FolderModel> Folders { get; set; }
    }
}