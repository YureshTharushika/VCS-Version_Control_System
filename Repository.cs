using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VCS
{
    internal static class Repository
    {

        public static void Init(string path)
        {
            var repoPath = Path.Combine(path, ".myvcs");
            Directory.CreateDirectory(repoPath);
            Directory.CreateDirectory(Path.Combine(repoPath, "objects"));
            Directory.CreateDirectory(Path.Combine(repoPath, "refs", "heads"));
            File.WriteAllText(Path.Combine(repoPath, "HEAD"), "ref: refs/heads/master");
            Console.WriteLine("Initialized empty VCS repository in " + repoPath);
        }

        public static void Add(string repoPath, string filePath)
        {
            var fullPath = Path.Combine(repoPath, filePath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("File not found: " + fullPath);
                return;
            }

            var content = File.ReadAllBytes(fullPath);
            var hash = ComputeHash(content);
            var blobPath = Path.Combine(repoPath, ".myvcs", "objects", hash);

            if (!File.Exists(blobPath))
            {
                File.WriteAllBytes(blobPath, content);
            }

            var relativePath = Path.GetRelativePath(repoPath, fullPath);
            AddToIndex(repoPath, relativePath, hash);

            Console.WriteLine("File added to the staging area.");
        }

        public static void Commit(string repoPath, string message)
        {
            var stagedFiles = GetStagedFiles(repoPath);

            var commit = new StringBuilder();
            commit.AppendLine($"Commit: {message}");
            commit.AppendLine($"Date: {DateTime.Now}");

            foreach (var file in stagedFiles)
            {
                commit.AppendLine($"{file.Key} {file.Value}");
            }

            var commitHash = ComputeHash(Encoding.UTF8.GetBytes(commit.ToString()));
            var commitPath = Path.Combine(repoPath, ".myvcs", "objects", commitHash);
            File.WriteAllText(commitPath, commit.ToString());

            var headPath = Path.Combine(repoPath, ".myvcs", "HEAD");
            var headContent = File.ReadAllText(headPath).Trim();
            if (headContent.StartsWith("ref: "))
            {
                var branchPath = Path.Combine(repoPath, ".myvcs", headContent.Substring(5));
                File.WriteAllText(branchPath, commitHash);
            }
            else
            {
                File.WriteAllText(headPath, commitHash);
            }

            // Clear the index after commit
            ClearIndex(repoPath);

            Console.WriteLine("Committed changes.");
        }

        private static void ClearIndex(string repoPath)
        {
            var indexPath = Path.Combine(repoPath, ".myvcs", "index");
            File.WriteAllText(indexPath, string.Empty);
        }

        private static void AddToIndex(string repoPath, string filePath, string fileHash)
        {
            var indexPath = Path.Combine(repoPath, ".myvcs", "index");
            File.AppendAllText(indexPath, $"{filePath} {fileHash}\n");
        }

        public static void CreateBranch(string repoPath, string branchName)
        {
            var headCommit = GetHeadCommit(repoPath);
            var branchPath = Path.Combine(repoPath, ".myvcs", "refs", "heads", branchName);
            File.WriteAllText(branchPath, headCommit);
            Console.WriteLine($"Branch '{branchName}' created.");
        }

        public static void SwitchBranch(string repoPath, string branchName)
        {
            var branchPath = Path.Combine(repoPath, ".myvcs", "refs", "heads", branchName);
            if (!File.Exists(branchPath))
            {
                Console.WriteLine($"Branch '{branchName}' does not exist.");
                return;
            }

            var newHeadCommit = File.ReadAllText(branchPath).Trim();
            var currentHeadCommit = GetHeadCommit(repoPath);

            UpdateWorkingDirectory(repoPath, currentHeadCommit, newHeadCommit);

            File.WriteAllText(Path.Combine(repoPath, ".myvcs", "HEAD"), $"ref: refs/heads/{branchName}");
            Console.WriteLine($"Switched to branch '{branchName}'.");
        }

        private static void UpdateWorkingDirectory(string repoPath, string oldCommitHash, string newCommitHash)
        {
            var oldCommitFiles = GetCommitFiles(repoPath, oldCommitHash);
            var newCommitFiles = GetCommitFiles(repoPath, newCommitHash);

            // Remove files that are in the old commit but not in the new commit
            foreach (var oldFile in oldCommitFiles)
            {
                if (!newCommitFiles.ContainsKey(oldFile.Key))
                {
                    var filePath = Path.Combine(repoPath, oldFile.Key);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            // Add or update files from the new commit
            foreach (var newFile in newCommitFiles)
            {
                var filePath = Path.Combine(repoPath, newFile.Key);
                var blobPath = Path.Combine(repoPath, ".myvcs", "objects", newFile.Value);
                File.WriteAllBytes(filePath, File.ReadAllBytes(blobPath));
            }

            // Clear the index and add the new commit files to the index
            ClearIndex(repoPath);
            foreach (var newFile in newCommitFiles)
            {
                AddToIndex(repoPath, newFile.Key, newFile.Value);
            }
        }

        public static void Status(string repoPath)
        {
            var workingDirectoryFiles = Directory.GetFiles(repoPath, "*", SearchOption.AllDirectories)
                .Where(file => !file.Contains(".myvcs"))
                .Select(file => new { Path = file, Hash = ComputeHash(File.ReadAllBytes(file)) })
                .ToDictionary(f => Path.GetRelativePath(repoPath, f.Path), f => f.Hash);

            var stagedFiles = GetStagedFiles(repoPath);
            var headCommit = GetHeadCommit(repoPath);
            var lastCommitFiles = GetCommitFiles(repoPath, headCommit);

            var modifiedFiles = workingDirectoryFiles
                .Where(file => lastCommitFiles.ContainsKey(file.Key) && lastCommitFiles[file.Key] != file.Value)
                .Select(file => file.Key)
                .ToList();

            var addedFiles = stagedFiles
                .Where(file => !lastCommitFiles.ContainsKey(file.Key))
                .Select(file => file.Key)
                .ToList();

            var untrackedFiles = workingDirectoryFiles
                .Where(file => !stagedFiles.ContainsKey(file.Key) && !lastCommitFiles.ContainsKey(file.Key))
                .Select(file => file.Key)
                .ToList();

            if (modifiedFiles.Any() || addedFiles.Any())
            {
                Console.WriteLine("Changes to be committed:");
                foreach (var file in modifiedFiles)
                {
                    Console.WriteLine("\tmodified: " + file);
                }
                foreach (var file in addedFiles)
                {
                    Console.WriteLine("\tnew file: " + file);
                }
            }

            if (untrackedFiles.Any())
            {
                Console.WriteLine("\nUntracked files:");
                foreach (var file in untrackedFiles)
                {
                    Console.WriteLine("\t" + file);
                }
            }

            if (!modifiedFiles.Any() && !addedFiles.Any() && !untrackedFiles.Any())
            {
                Console.WriteLine("Nothing to commit, working tree clean.");
            }
            else if (!modifiedFiles.Any() && !addedFiles.Any())
            {
                Console.WriteLine("No modified files to be committed.");
            }
        }

        private static Dictionary<string, string> GetStagedFiles(string repoPath)
        {
            var indexPath = Path.Combine(repoPath, ".myvcs", "index");
            var stagedFiles = new Dictionary<string, string>();

            if (File.Exists(indexPath))
            {
                var lines = File.ReadAllLines(indexPath);
                foreach (var line in lines)
                {
                    var parts = line.Split(' ');
                    if (parts.Length == 2)
                    {
                        stagedFiles[parts[0]] = parts[1];
                    }
                }
            }

            return stagedFiles;
        }

        private static string GetHeadCommit(string repoPath)
        {
            var headPath = Path.Combine(repoPath, ".myvcs", "HEAD");
            if (File.Exists(headPath))
            {
                var headContent = File.ReadAllText(headPath).Trim();
                if (headContent.StartsWith("ref: "))
                {
                    var branchPath = Path.Combine(repoPath, ".myvcs", headContent.Substring(5));
                    if (File.Exists(branchPath))
                    {
                        return File.ReadAllText(branchPath).Trim();
                    }
                }
                else
                {
                    return headContent;
                }
            }
            return null;
        }

        private static Dictionary<string, string> GetCommitFiles(string repoPath, string commitHash)
        {
            var commitFiles = new Dictionary<string, string>();
            if (commitHash != null)
            {
                var commitPath = Path.Combine(repoPath, ".myvcs", "objects", commitHash);
                if (File.Exists(commitPath))
                {
                    var lines = File.ReadAllLines(commitPath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(' ');
                        if (parts.Length == 2)
                        {
                            commitFiles[parts[0]] = parts[1];
                        }
                    }
                }
            }
            return commitFiles;
        }

        private static string ComputeHash(byte[] content)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(content);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
