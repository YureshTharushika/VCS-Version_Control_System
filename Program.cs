namespace VCS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0];

            switch (command)
            {
                case "init":
                    Repository.Init(Environment.CurrentDirectory);
                    break;
                case "add":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a file to add.");
                        return;
                    }
                    Repository.Add(Environment.CurrentDirectory, args[1]);
                    break;
                case "commit":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a commit message.");
                        return;
                    }
                    Repository.Commit(Environment.CurrentDirectory, args[1]);
                    break;
                case "branch":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a branch name.");
                        return;
                    }
                    Repository.CreateBranch(Environment.CurrentDirectory, args[1]);
                    break;
                case "checkout":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify a branch name.");
                        return;
                    }
                    Repository.SwitchBranch(Environment.CurrentDirectory, args[1]);
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
        }
    }
}
