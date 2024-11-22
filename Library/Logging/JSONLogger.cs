using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Logger
{
    private readonly string _filePath;

    public Logger(string filePath)
    {
        _filePath = filePath;
    }

    public void Log(string message)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Message = message
        };

        var logEntries = new List<LogEntry>();

        if (File.Exists(_filePath))
        {
            var existingLog = File.ReadAllText(_filePath);
            logEntries = JsonConvert.DeserializeObject<List<LogEntry>>(existingLog) ?? new List<LogEntry>();
        }

        logEntries.Add(logEntry);

        var json = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
        File.WriteAllText(_filePath, json);
    }

    private class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}
