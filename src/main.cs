class Program
{
    static void Main()
    {
        // TODO: Uncomment the code below to pass the first stage
        Console.Write("$ ");
        while (true)
        {
            string command = Console.ReadLine();
            if (command == null || command != null)
                Console.WriteLine($"{command}: command not found");
        }
    }
}
