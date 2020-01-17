using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AwsStack
{
    public static class DockerFolderBuilder
    {
        private static string buildFolder = "cdk_build";

        public static string GetBuildFolder()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), buildFolder);
        }

        public static string CreateBuildFolder()
        {
            CleanUpAllBuildFolders();

            string workingFolder = GetBuildFolder();

            Directory.CreateDirectory(workingFolder);

            DirectoryCopy(Directory.GetCurrentDirectory(),
                                workingFolder,
                                new List<string>() {
                                    "bin",
                                    "obj",
                                    "node_modules",
                                    "cdk.out",
                                    "cdk_build",
                                    ".git",
                                    ".vs",
                                    "packages",
                                    "dist"
                                }
                                , 0);

            return workingFolder;
        }

        public static void CleanUpAllBuildFolders()
        {
            string directory = GetBuildFolder();
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, ICollection<string> excludeFolders, int level)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (DirectoryInfo subdir in dirs.Where(dir => excludeFolders.Contains(dir.Name) == false))
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, excludeFolders, level + 1);
            }
        }
    }

}
