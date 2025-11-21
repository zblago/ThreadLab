using ThreadLab;

Console.Write("Number of threads: ");
var numberOfThreads = int.Parse(Console.ReadLine()!);

Console.Write("Batch size (how many times you want to increment per batch): ");
Console.WriteLine();
var numberOfIncrementsPerThread = int.Parse(Console.ReadLine()!);

Incrementer.Run(numberOfThreads, numberOfIncrementsPerThread);

Console.ReadLine();