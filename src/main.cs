class Program
{
    static void Main()
    {
        // TODO: Uncomment the code below to pass the first stage
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


            Console.WriteLine($"{command}: command not found");
        }
    }
}
