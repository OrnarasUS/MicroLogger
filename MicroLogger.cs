using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

public static class MicroLogger
{
    private static string pathDirLogs;
    private static Queue<string> logRecords = new Queue<string>();
    private static Thread logThread;
    private static int msDelay;

    private static string pathLog => Path.Combine(pathDirLogs, $"{DateTime.Today:yyyyMMdd}.log");

    private static void Log(string prefix, string message) =>
        logRecords.Enqueue($"{DateTime.Now:HH:mm:ss.fff} [{prefix}] -> {message}");
    private static void Begin()
    {
        logThread = new Thread(() =>
        {
            while (true)
            {
                Force();
                Thread.Sleep(msDelay);
            }
        })
        { IsBackground = true };
        logThread.Start();
    }

    /// <summary>
    /// Инициализация журнала
    /// </summary>
    /// <param name="path">Путь директории журналов</param>
    /// <param name="delay">Интервал между записям в журнал</param>
    public static void Init(string path, int delay = 1000)
    {
        pathDirLogs = path;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        msDelay = delay;
        Begin();
    }
    /// <summary>
    /// Принудительная запись в журнал
    /// </summary>
    public static void Force()
    {
        int countRecords = logRecords.Count;
        for (int i = 0; i < countRecords; i++)
        {
            File.AppendAllText(pathLog, $"{logRecords.Dequeue()}\n");
        }
    }
    /// <summary>
    /// Сообщение об исключении в журнал
    /// </summary>
    /// <param name="e"></param>
    public static void Exception(Exception e)
    {
#if DEBUG
        var st = new StackTrace(e, true);
        var fr = st.GetFrame(0);
        var msg = $"({fr.GetFileName()} {fr.GetFileLineNumber()}:{fr.GetFileColumnNumber()}) {e.Message}";
#else
        var msg = e.Message;
#endif
        Log("CRIT", msg);
    }
    /// <summary>
    /// Сообщение в журнал, требующее особого внимания
    /// </summary>
    /// <param name="message"></param>
    public static void Error(string message) => 
        Log("ERRR", message);
    /// <summary>
    /// Сообщение в журнал, требующее внимания
    /// </summary>
    /// <param name="message"></param>
    public static void Warning(string message) => 
        Log("WARN", message);
    /// <summary>
    /// Информационное соощение в журнал
    /// </summary>
    /// <param name="message"></param>
    public static void Info(string message) => 
        Log("INFO", message);
    /// <summary>
    /// Отладочное сообщение в журнал
    /// </summary>
    public static void Debug(string message)
    {
#if DEBUG
        Log("DEBG", message);
#endif
    }
}