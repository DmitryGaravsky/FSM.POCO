namespace FSM.POCO.Console {
    class Program {
        static void Main(string[] args) {
            // Create and run our machine
            Source.Create<Machine>().Run();
            System.Console.WriteLine("Bye (press any key)!");
            System.Console.ReadKey();
        }
    }
}