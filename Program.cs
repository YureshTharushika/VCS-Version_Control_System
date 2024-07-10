namespace VCS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                ShowHelp();
                return;
            }

            var command = args[0];
            var targetDirectory = args[1];

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
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a file to add.");
                        return;
                    }
                    Repository.Add(targetDirectory, args[2]);
                    break;
                case "commit":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a commit message.");
                        return;
                    }
                    Repository.Commit(targetDirectory, args[2]);
                    break;
                case "branch":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a branch name.");
                        return;
                    }
                    Repository.CreateBranch(targetDirectory, args[2]);
                    break;
                case "checkout":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a branch name.");
                        return;
                    }
                    Repository.SwitchBranch(targetDirectory, args[2]);
                    break;
                case "status":
                    Repository.Status(targetDirectory);
                    break;
                default:
                    ShowHelp();
                    break;
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
        }
    }
}
