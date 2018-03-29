namespace FSM.POCO.Console {
    class Program {
        static void Main(string[] args) {
            // Run our machine
            new Machine().Run();
            System.Console.WriteLine("Bye (press any key)!");
            System.Console.ReadKey();
        }
    }
}