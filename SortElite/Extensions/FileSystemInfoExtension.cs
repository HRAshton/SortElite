using System.IO;

namespace SortElite.Extensions
{
    public static class FileSystemInfoExtension
    {
        public static void SilentForceDelete(this FileSystemInfo fileSystemInfo)
        {
            if (!fileSystemInfo.Exists)
            {
                return;
            }

            if (fileSystemInfo is DirectoryInfo directoryInfo)
            {
                foreach (var childInfo in directoryInfo.GetFileSystemInfos())
                {
                    childInfo.SilentForceDelete();
                }
            }

            fileSystemInfo.Attributes = FileAttributes.Normal;
            fileSystemInfo.Refresh();
            fileSystemInfo.Delete();
            fileSystemInfo.Refresh();
        }
    }
}