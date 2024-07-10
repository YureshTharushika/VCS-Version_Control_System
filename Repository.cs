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
            Console.WriteLine("Full Path: " + fullPath);

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
            var indexPath = Path.Combine(repoPath, ".myvcs", "index");
            File.AppendAllText(indexPath, $"{relativePath} {hash}\n");

            Console.WriteLine("File added to the staging area.");
        }

        public static void Commit(string repoPath, string message)
        {
            var stagedFiles = GetStagedFiles(repoPath);

            if (!stagedFiles.Any())
            {
                Console.WriteLine("No changes added to commit.");
                return;
            }

            var commitContent = new StringBuilder();
            commitContent.AppendLine($"Commit: {message}");
            commitContent.AppendLine($"Date: {DateTime.Now}");

            foreach (var file in stagedFiles)
            {
                commitContent.AppendLine($"{file.Key} {file.Value}");
            }

            var commitHash = ComputeHash(Encoding.UTF8.GetBytes(commitContent.ToString()));
            var commitPath = Path.Combine(repoPath, ".myvcs", "objects", commitHash);

            File.WriteAllText(commitPath, commitContent.ToString());
            File.WriteAllText(Path.Combine(repoPath, ".myvcs", "HEAD"), commitHash);

            // Clear the staging area
            File.WriteAllText(Path.Combine(repoPath, ".myvcs", "index"), string.Empty);

            Console.WriteLine("Committed changes.");
        }

        public static void CreateBranch(string repoPath, string branchName)
        {
            var head = File.ReadAllText(Path.Combine(repoPath, ".myvcs", "HEAD")).Trim();
            var branchPath = Path.Combine(repoPath, ".myvcs", "refs", "heads", branchName);
            File.WriteAllText(branchPath, head);
            Console.WriteLine($"Branch '{branchName}' created.");
        }

        public static void SwitchBranch(string repoPath, string branchName)
        {
            var branchPath = Path.Combine(repoPath, ".myvcs", "refs", "heads", branchName);
            if (File.Exists(branchPath))
            {
                File.WriteAllText(Path.Combine(repoPath, ".myvcs", "HEAD"), $"ref: refs/heads/{branchName}");
                Console.WriteLine($"Switched to branch '{branchName}'.");
            }
            else
            {
                Console.WriteLine($"Branch '{branchName}' does not exist.");
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
                    foreach (var line in lines.Skip(2)) // Skip the commit message and date
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
