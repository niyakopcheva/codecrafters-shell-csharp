using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

List<string> builtin = new(["exit", "echo", "type", "pwd", "cd"]);
Dictionary<string, Action<string[]>> commands = new()
{
    { "echo", echo },
    { "exit", exit },
    { "type", type },
    { "pwd",  pwd  },
    { "cd",   cd   }
};

while (true)
{
    Console.Write("$ ");

    string command = Console.ReadLine() ?? "";
    if (command == "") continue;

    string firstCommand = command.Split(" ")[0];
    string[] arguments = getArguments(command, firstCommand);

    if (commands.ContainsKey(firstCommand))
    {
        commands[firstCommand].Invoke(arguments);
    }
    else
    {
        var program = findExe(firstCommand);
        if (program == "")
        {
            System.Console.WriteLine($"{firstCommand}: command not found");
            continue;
        }

        if (arguments == null)
        {
            var process = Process.Start(program);
            process?.WaitForExit();
        }
        else
        {
            Process.Start(program, arguments)?.WaitForExit();
        }

    }
}

static void echo(string[] arguments)
{
    System.Console.WriteLine(String.Join(" ", arguments));
}

static void exit(string[] arguments)
{
    System.Environment.Exit(0);
}

void type(string[] arguments)
{
    string cmd = arguments[0];
    if (builtin.Contains(cmd))
        Console.WriteLine($"{cmd} is a shell builtin");
    else
    {
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (path == null) return;

        string[] directories = path.Split(Path.PathSeparator);
        foreach (string dir in directories)
        {
            if (!Directory.Exists(dir)) continue;

            var files = Directory.GetFiles(dir);
            var exactFilePath = files.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f).Equals(cmd, StringComparison.OrdinalIgnoreCase));
            // var names = files.Select(f => Path.GetFileNameWithoutExtension(f));

            if (exactFilePath != null)
            {
                if (isExecutable(exactFilePath))
                {
                    System.Console.WriteLine($"{cmd} is {exactFilePath}");
                    return;
                }
            }
        }

        System.Console.WriteLine($"{cmd}: not found");
    }
}

bool isExecutable(string path)
{
    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
    {
        UnixFileMode mode = File.GetUnixFileMode(path);
        if ((mode & UnixFileMode.UserExecute) != 0 ||
        (mode & UnixFileMode.GroupExecute) != 0 ||
        (mode & UnixFileMode.OtherExecute) != 0)
            return true;
        return false;
    }

    //for windows
    return true;
}

string findExe(string program)
{
    string? path = Environment.GetEnvironmentVariable("PATH");

    string[] directories = path.Split(Path.PathSeparator);
    foreach (string dir in directories)
    {
        if (!Directory.Exists(dir)) continue;

        var files = Directory.GetFiles(dir);
        var exactFilePath = files.FirstOrDefault(f =>
        Path.GetFileNameWithoutExtension(f).Equals(program, StringComparison.OrdinalIgnoreCase));

        if (exactFilePath != null)
        {
            if (isExecutable(exactFilePath))
                return program;
        }
    }
    return "";
}

void pwd(string[] arguments)
{
    if (arguments.Count() != 1 || arguments[0] != "")
    {
        System.Console.WriteLine("Invalid command");
        return;
    }
    string path = Directory.GetCurrentDirectory();
    if (path[0] != '/') path = path.Insert(0, "/");
    System.Console.WriteLine(path);
}

void cd(string[] arguments)
{
    if (arguments.Count() != 1 || arguments[0] == "")
    {
        Console.WriteLine($"cd: {String.Join(" ", arguments)}: No such file or directory");
        return;
    }

    string target = arguments[0];
    if (target[0] == '/' && target[2] == ':')
        target = target.Substring(1);

    //home env var
    if (target == "~")
    {
        target = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Directory.SetCurrentDirectory(target);
        return;
    }

    try
    {
        string fullPath = Path.GetFullPath(target);

        if (Directory.Exists(fullPath))
        {
            Directory.SetCurrentDirectory(fullPath);
        }
        else
        {
            Console.WriteLine($"cd: {target}: No such file or directory");
        }
    }
    catch (Exception)
    {
        Console.WriteLine($"cd: {target}: No such file or directory");
    }
}


string[] getArguments(string input, string firstCommand)
{
    input = input.Replace("''", "").Substring(firstCommand.Length);
    var matches = Regex.Matches(input, @"'([^']*)'|(\S+)");
    string[] arguments = matches
    .Select(m => m.ToString().Trim('\''))
    .ToArray();

    return arguments;
}