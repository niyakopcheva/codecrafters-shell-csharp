using System.Diagnostics;
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
    string[] arguments = command.Substring(firstCommand.Length)
    .Trim()
    .Split(" ");

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
        System.Console.WriteLine("Invalid command");
        return;
    }

    string target = arguments[0];
    //aboslute paths
    // if (target[0] == '/')
    // {
    if (target.StartsWith("/"))
        target = target.Replace("/", "\\").Substring(1);

    if (Directory.Exists(target))
        Directory.SetCurrentDirectory(target);
    else
        System.Console.WriteLine($"cd: {target}: No such file or directory");
    // }
    // else
    // {
    //     if (target.StartsWith("./"))
    //         target = target.Replace(".", Directory.GetCurrentDirectory());
    //     if (target.StartsWith("../"))
    //     {
    //         var matchCount = Regex.Matches(target, @"\.\.\/").Count();
    //         for (int i = 0; i < matchCount; i++)
    //         {
    //             string current = Directory.GetCurrentDirectory();
    //             var parent = Directory.GetParent(current);
    //             if (Directory.Exists(parent.ToString()))
    //                 Directory.SetCurrentDirectory(parent.ToString());
    //         }

    //     }

    //     if (OperatingSystem.IsWindows())
    //         target = target.Replace(@"/", @"\");

    //     if (Directory.Exists(target))
    //         Directory.SetCurrentDirectory(target);
    // }
}