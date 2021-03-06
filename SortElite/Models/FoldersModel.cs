﻿using System.Collections.Generic;

namespace SortElite.Models
{
    public class FoldersModel
    {
        public FolderModel Unknown { get; set; } = new FolderModel {Name = "Unknown"};

        public FolderModel Removed { get; set; } = new FolderModel {Name = "Removed"};

        public FolderModel Broken { get; set; } = new FolderModel {Name = "Broken"};

        public List<FolderModel> Folders { get; set; } = new List<FolderModel>();
    }
}