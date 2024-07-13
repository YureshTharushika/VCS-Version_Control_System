namespace VCS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    ShowHelp();
                    return;
                }

                var targetDirectory = NormalizePath(args[0]);
                var command = args[1].ToLower();

                if (!Directory.Exists(targetDirectory))
                {
                    Console.WriteLine("The specified directory does not exist.");
                    return;
                }

                switch (command)
                {
                    case "init":
                        Repository.Init(targetDirectory);
                        break;
                    case "add":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: vcs <targetDirectory> add <filePath>");
                            return;
                        }
                        Repository.Add(targetDirectory, args[2]);
                        break;
                    case "commit":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: vcs <targetDirectory> commit <message>");
                            return;
                        }
                        Repository.Commit(targetDirectory, args[2]);
                        break;
                    case "branch":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: vcs <targetDirectory> branch <branchName>");
                            return;
                        }
                        Repository.CreateBranch(targetDirectory, args[2]);
                        break;
                    case "checkout":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: vcs <targetDirectory> checkout <branchName>");
                            return;
                        }
                        Repository.SwitchBranch(targetDirectory, args[2]);
                        break;
                    case "status":
                        Repository.Status(targetDirectory);
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("SimpleVCS commands:");
            Console.WriteLine("  init                Initialize a new repository");
            Console.WriteLine("  add <file>          Add a file to the staging area");
            Console.WriteLine("  commit <message>    Commit changes");
            Console.WriteLine("  branch <name>       Create a new branch");
            Console.WriteLine("  checkout <name>     Switch to a branch");
            Console.WriteLine("  status              Show the current status");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  vcs /path/to/repo init");
            Console.WriteLine("  vcs /path/to/repo add file.txt");
            Console.WriteLine("  vcs /path/to/repo commit \"Initial commit\"");
            Console.WriteLine("  vcs /path/to/repo branch new-branch");
            Console.WriteLine("  vcs /path/to/repo checkout new-branch");
            Console.WriteLine("  vcs /path/to/repo status");
        }

        static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }
    }
}
