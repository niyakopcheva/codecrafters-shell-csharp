class Program
{
    static void Main()
    {
        List<string> builtin = new(["exit", "echo", "type"]);
        while (true)
        {
            Console.Write("$ ");
            string command = Console.ReadLine();

            if (command == "exit") return;

            string firstCommand = command.Split(" ")[0];

            if (firstCommand == "echo")
            {
                string[] args = command.Substring(command.IndexOf(' ') + 1).Split(" ");
                Console.WriteLine(String.Join(" ", args));
                continue;
            }

            if (firstCommand == "type")
            {
                string arg = command.Substring(command.IndexOf(' ') + 1);
                if (builtin.Contains(arg))
                    Console.WriteLine($"{arg} is a shell builtin");
                else
                    Console.WriteLine($"{arg}: not found");
                continue;
            }

            Console.WriteLine($"{command}: command not found");
        }
    }
}
