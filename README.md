VCS-Version Control System
=========

VCS is a basic version control system implemented in C# using .NET Core. It provides essential functionalities like initializing a repository, adding files to the staging area, committing changes, creating branches, switching branches, and checking the status of the repository.

Features
--------

-   **Initialize a Repository**: Create a new repository.
-   **Add Files**: Add files to the staging area.
-   **Commit Changes**: Commit staged changes with a message.
-   **Create Branches**: Create new branches.
-   **Switch Branches**: Switch between branches.
-   **Status Check**: Show the current status of the repository, including staged, modified, and untracked files.

Usage
-----

### Commands

-   `init` - Initialize a new repository in the specified directory.
-   `add <file>` - Add a file to the staging area.
-   `commit <message>` - Commit changes with a message.
-   `branch <name>` - Create a new branch.
-   `checkout <name>` - Switch to an existing branch.
-   `status` - Show the current status of the repository.

### Command-Line Usage

    ```
    vcs <targetDirectory> <command> [options]
    ```
    
### Examples

1. **Initialize a Repository**

    ```
    vcs /path/to/repo init
    ```

2. **Add a File to the Staging Area**

    ```
    vcs /path/to/repo add filename.txt
    ```

3. **Commit Changes**

    ```
    vcs /path/to/repo commit "Initial commit"
    ```

4. **Create a New Branch**

    ```
    vcs /path/to/repo branch new-branch
    ```

5. **Switch to an Existing Branch**

    ```
    vcs /path/to/repo checkout new-branch
    ```

6. **Check Repository Status**

    ```
    vcs /path/to/repo status
    ```

## Installation

1. Clone the repository:

    ```
    git clone https://github.com/yourusername/SimpleVCS.git
    ```

2. Navigate to the project directory:

    ```
    cd SimpleVCS
    ```

3. Build the project:

    ```
    dotnet build
    ```

4. Navigate to the build output directory:

    ```
    cd bin/Debug/net<version>
    ```

5. Run the project:

    ```
    dotnet VCS.dll <targetDirectory> <command> [options]
    ```

## Contributing

Contributions are welcome! Please create an issue first to discuss any major changes.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a pull request
