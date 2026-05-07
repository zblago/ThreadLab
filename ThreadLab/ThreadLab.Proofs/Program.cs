/********* Single Set signal, other threads are waiting *****/
var wait = new AutoResetEvent(false);

var threads = new Thread[5];
for (var i = 0; i < threads.Length - 1; i++)
{
    threads[i] = new Thread((i) =>
    {
        Console.WriteLine($"Thread_{i} is waiting.");
        wait.WaitOne();
        Console.WriteLine($"Thread_{i} received the signal.");
    });
};

for (var i = 0; i < threads.Length - 1; i++) threads[i].Start(i);

Thread.Sleep(1000);

wait.Set();

/**************** Multiple Set() signals (wasted)**********************/
var set = new AutoResetEvent(false);

var threadsSet = new Thread[5];
for (var i = 0; i < threadsSet.Length - 1; i++)
{
    threadsSet[i] = new Thread((i) =>
    {        
        set.Set();
        Console.WriteLine($"Thread_{i} sent the signal.");
    });
};

for (var i = 0; i < threads.Length - 1; i++) threadsSet[i].Start(i);

Thread.Sleep(10000);

set.WaitOne();
Console.WriteLine("Wait 1");

set.WaitOne();
Console.WriteLine("Wait 2");

set.WaitOne();
Console.WriteLine("Wait 3");

set.WaitOne();
Console.WriteLine("Wait 4");

Console.WriteLine("Wait 5");