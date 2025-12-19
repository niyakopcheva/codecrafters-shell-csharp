using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

List<string> builtin = new(["exit", "echo", "type"]);
Dictionary<string, Action<string[]>> commands = new()
{
    { "echo", echo },
    { "exit", exit },
    { "type", type }
};

bool processStarted = false;

while (true)
{
    if (!processStarted)
        Console.Write("$ ");
    else
        processStarted = false;
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
            Process.Start(program);
        else
            Process.Start(program, arguments);
        processStarted = true;
    }
    // else
    // {
    //     System.Console.WriteLine($"{firstCommand}: command not found");
    // }
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