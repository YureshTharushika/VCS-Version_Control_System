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
            var fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            var content = File.ReadAllBytes(fullPath);
            var hash = ComputeHash(content);
            var blobPath = Path.Combine(repoPath, ".myvcs", "objects", hash);

            if (!File.Exists(blobPath))
            {
                File.WriteAllBytes(blobPath, content);
                Console.WriteLine("File added to the staging area.");
            }
        }

        public static void Commit(string repoPath, string message)
        {
            // This is a simplified version and does not include staging area handling
            var commit = $"Commit: {message}\nDate: {DateTime.Now}\n";
            var commitHash = ComputeHash(Encoding.UTF8.GetBytes(commit));
            var commitPath = Path.Combine(repoPath, ".myvcs", "objects", commitHash);
            File.WriteAllText(commitPath, commit);
            File.WriteAllText(Path.Combine(repoPath, ".myvcs", "HEAD"), commitHash);
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
