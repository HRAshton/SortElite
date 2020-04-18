using System.Collections.Generic;

namespace SortElite.Models
{
    public class RuleModel
    {
        public const string RemoveingGroupName = "Remove";

        public string FolderName { get; set; }

        public List<string> RegexPatterns { get; set; } = new List<string>();
    }
}