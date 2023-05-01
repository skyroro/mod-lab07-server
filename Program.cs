using System.Security.Cryptography;
using System;
using System.IO;

public class procEventArgs : EventArgs
{
    public int id { get; set; }
}

struct PoolRecord
{
    public Thread thread; // объект потока
    public bool in_use; // флаг занятости
}

class Server
{
    PoolRecord[] pool;
    object threadLock = new object();

    public int poolSize; //Размер пула потоков
    public int requestCount = 0; //Количество полученных запросов
    public int processedCount = 0; //Количество обработанных запросов
    public int rejectedCount = 0; //Количество отклоненных запросов
    public int delayTime;

    public Server(int size, int time) // Конструктор с указанием размера пула потоков
    {
        pool = new PoolRecord[size];
        delayTime = time;
        poolSize = size;
    }

    public void proc(object sender, procEventArgs e)
    {
        lock (threadLock)
        {
            requestCount++;
            for (int i = 0; i < poolSize; i++)
            {
                if (!pool[i].in_use)
                {
                    pool[i].in_use = true;
                    pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                    pool[i].thread.Start(e.id);
                    processedCount++;
                    return;
                }
            }
            rejectedCount++;
        }
    }

    public void Answer(object arg)
    {
        int id = (int)arg;
        Thread.Sleep(delayTime);
        for (int i = 0; i < poolSize; i++)
        {
            if (pool[i].thread == Thread.CurrentThread)
            {
                pool[i].in_use = false;
            }
        }
    }
}

class Client
{
    public event EventHandler<procEventArgs> request;
    private Server server;

    public Client(Server server)
    {
        this.server = server;
        this.request += server.proc;
    }

    protected virtual void OnProc(procEventArgs e)
    {
        EventHandler<procEventArgs> handler = request;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    public void Send(int id)
    {
        procEventArgs args = new procEventArgs();
        args.id = id;
        OnProc(args);
    }
}

class Program
{
    static long Factorial(long n)
    {
        if (n == 0) return 1;
        else return n * Factorial(n - 1);
    }

    static void PrintConsole(int requestCount, int processedCount, int rejectedCount, int la, int mu, int poolSize)
    {
        double p = la / mu;
        double temp = 0;
        for (long i = 0; i <= poolSize; i++)
            temp = temp + Math.Pow(p, i) / Factorial(i);

        double P0 = 1 / temp;
        double Pn = Math.Pow(p, poolSize) * P0 / Factorial(poolSize);
        double Q = 1 - Pn;
        double A = la * Q;
        double k = A / mu;

        Console.WriteLine("Показатели по формулам: \n");

        Console.WriteLine("Всего заявок:" + requestCount);
        Console.WriteLine("Обработано заявок:" + processedCount);
        Console.WriteLine("Отклонено заявок:" + rejectedCount);
        Console.WriteLine("Интенсивность потока заявок (запросов): " + la);
        Console.WriteLine("Интенсивность μ потока обслуживания: " + mu);

        Console.WriteLine("Количество потоков: " + poolSize);
        Console.WriteLine("Вероятность простоя системы: " + P0);
        Console.WriteLine("Вероятность отказа системы: " + Pn);
        Console.WriteLine("Относительная пропускная способность: " + Q);
        Console.WriteLine("Абсолютная пропускная способность: " + A);
        Console.WriteLine("Среднее число занятых каналов: " + k);

        la = requestCount / 1;
        mu = processedCount / poolSize;

        p = la / mu;
        temp = 0;
        for (long i = 0; i <= poolSize; i++)
            temp = temp + Math.Pow(p, i) / Factorial(i);

        P0 = 1 / temp;
        Pn = Math.Pow(p, poolSize) * P0 / Factorial(poolSize);
        Q = 1 - Pn;
        A = la * Q;
        k = A / mu;

        Console.WriteLine("\nРезультаты обработки статистики: \n");

        Console.WriteLine("Всего заявок:" + requestCount);
        Console.WriteLine("Обработано заявок:" + processedCount);
        Console.WriteLine("Отклонено заявок:" + rejectedCount);
        Console.WriteLine("Интенсивность потока заявок (запросов): " + la);
        Console.WriteLine("Интенсивность μ потока обслуживания: " + mu);

        Console.WriteLine("Количество потоков: " + poolSize);
        Console.WriteLine("Вероятность простоя системы: " + P0);
        Console.WriteLine("Вероятность отказа системы: " + Pn);
        Console.WriteLine("Относительная пропускная способность: " + Q);
        Console.WriteLine("Абсолютная пропускная способность: " + A);
        Console.WriteLine("Среднее число занятых каналов: " + k);
    }

    static void PrintFile(int requestCount, int processedCount, int rejectedCount, int la, int mu, int poolSize)
    {
        double p = la / mu;
        double temp = 0;
        for (long i = 0; i <= poolSize; i++)
            temp = temp + Math.Pow(p, i) / Factorial(i);

        double P0 = 1 / temp;
        double Pn = Math.Pow(p, poolSize) * P0 / Factorial(poolSize);
        double Q = 1 - Pn;
        double A = la * Q;
        double k = A / mu;

        string path = "results.txt";
        if (!File.Exists(path))
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("Показатели по формулам: \n");

                sw.WriteLine("Всего заявок:" + requestCount);
                sw.WriteLine("Обработано заявок:" + processedCount);
                sw.WriteLine("Отклонено заявок:" + rejectedCount);
                sw.WriteLine("Интенсивность потока заявок (запросов): " + la);
                sw.WriteLine("Интенсивность μ потока обслуживания: " + mu);

                sw.WriteLine("Количество потоков: " + poolSize);
                sw.WriteLine("Вероятность простоя системы: " + P0);
                sw.WriteLine("Вероятность отказа системы: " + Pn);
                sw.WriteLine("Относительная пропускная способность: " + Q);
                sw.WriteLine("Абсолютная пропускная способность: " + A);
                sw.WriteLine("Среднее число занятых каналов: " + k);

                la = requestCount / 1;
                mu = processedCount / poolSize;

                p = la / mu;
                temp = 0;
                for (long i = 0; i <= poolSize; i++)
                    temp = temp + Math.Pow(p, i) / Factorial(i);

                P0 = 1 / temp;
                Pn = Math.Pow(p, poolSize) * P0 / Factorial(poolSize);
                Q = 1 - Pn;
                A = la * Q;
                k = A / mu;

                sw.WriteLine("\nРезультаты обработки статистики: \n");

                sw.WriteLine("Всего заявок:" + requestCount);
                sw.WriteLine("Обработано заявок:" + processedCount);
                sw.WriteLine("Отклонено заявок:" + rejectedCount);
                sw.WriteLine("Интенсивность потока заявок (запросов): " + la);
                sw.WriteLine("Интенсивность μ потока обслуживания: " + mu);

                sw.WriteLine("Количество потоков: " + poolSize);
                sw.WriteLine("Вероятность простоя системы: " + P0);
                sw.WriteLine("Вероятность отказа системы: " + Pn);
                sw.WriteLine("Относительная пропускная способность: " + Q);
                sw.WriteLine("Абсолютная пропускная способность: " + A);
                sw.WriteLine("Среднее число занятых каналов: " + k);
            }
        }
    }

    static void Main(string[] args)
    {
        int la = 10; //среднее число заявок, поступающих из потока за единицу времени.
        int mu = 2; //Интенсивность μ потока обслуживаний

        int delayTime = 100;
        int poolSize = 5;
        int n = 10;

        Server server = new Server(poolSize, delayTime); // Создаем сервер с пулом потоков размером 10      
        Client client = new Client(server); // Создаем клиента и передаем ему ссылку на сервер

        for (int id = 1; id <= n; id++) //имитация работы
        {
            client.Send(id);
            Thread.Sleep(delayTime);
        }

        PrintConsole(server.requestCount, server.processedCount, server.rejectedCount, la, mu, poolSize);
        PrintFile(server.requestCount, server.processedCount, server.rejectedCount, la, mu, poolSize);
    }
}