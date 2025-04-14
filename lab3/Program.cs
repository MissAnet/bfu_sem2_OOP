using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Diagnostics;

public enum LogLevel
{
    Trace,      //Трассировка
    Debug,      //Отладка
    Info,       //Стандартные информация о работе системы
    Warning,    //Предупреждение
    Error,      //Ошибка
    Critical    //Фатальные ошибки
}

//Интерфейс для фильтров логов
public interface ILogFilter
{
    bool Match(string text);
}

//Интерфейс для обработчиков логов
public interface ILogHandler
{
    void Handle(string text);
}

//фильтр по вхождению подстроки
public class SimpleLogFilter : ILogFilter
{
    private readonly string _pattern; 

    public SimpleLogFilter(string pattern)
    {
        _pattern = pattern; //подстрака 
    }

    public bool Match(string text)
    {
        return text.Contains(_pattern); //ищет подстроку в тексте лога(регистро зависимый)
    }
}

//фильтр с использованием регулярных выражений
public class ReLogFilter : ILogFilter
{
    private readonly Regex _regex;

    public ReLogFilter(string pattern, RegexOptions options)
    {
        _regex = new Regex(pattern, options); //класс для работы с рег выражением
    }

    public bool Match(string text)
    {
        return _regex.IsMatch(text); //проверка на рег выражение
    }
}

//Фильтр по минимальному уровню логирования
public class LevelLogFilter : ILogFilter
{
    private readonly LogLevel _minLevel;

    public LevelLogFilter(LogLevel minLevel)
    {
        _minLevel = minLevel;
    }

    public bool Match(string text)
    {
        //уровень логирования указан в начале сообщения в формате "[LEVEL] текст"
        var levelStart = text.IndexOf('[') + 1;
        var levelEnd = text.IndexOf(']');

        //проверка на корректность формата
        if (levelStart < 1 || levelEnd < levelStart)
            return false;
        
        //Извлекаем [LEVEL], преоброзовывем в LogLevel и если преобразовалось корректно, то возвращаем true
        //p.s minlevel отвечает за мин. уровень логирования для вывода сообщение, например _minLevel = Warning , все что равно или выше -выведет, а Debug или Trace нет
        var levelStr = text.Substring(levelStart, levelEnd - levelStart);
        if (Enum.TryParse<LogLevel>(levelStr, true, out var level))
        {
            return level >= _minLevel;
        }

        return false;
    }
}

//Обработчик для записи в файл
public class FileHandler : ILogHandler
{
    private readonly string _filePath;

    public FileHandler(string filePath)
    {
        _filePath = filePath;
    }

    public void Handle(string text)
    {
        try
        {
            File.AppendAllText(_filePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {text}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при записи в файл: {ex.Message}");
        }
    }
}

//Обработчик для вывода в консоль
public class ConsoleHandler : ILogHandler
{
    public void Handle(string text)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} {text}");
    }
}

//тестовая версия для отправки по сети
public class SocketHandler : ILogHandler
{
    private readonly string _host;
    private readonly int _port;

    public SocketHandler(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public void Handle(string text)
    {
        try
        {
            using var client = new TcpClient(_host, _port);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SocketHandler error: {ex.Message}");
        }
    }
}

//обработчик для системного лога для работы нужен LogEvent API
/*public class SyslogHandler : ILogHandler
{
    private const string EventLogSource = "MyApplication";//источник
    private const string EventLogName = "Application";//имя лога

    public SyslogHandler()
    {
        //зарегистрирован ли источник
        if (!EventLog.SourceExists(EventLogSource))
        {
            //Если нет- создаём его
            EventLog.CreateEventSource(EventLogSource, EventLogName);
        }
    }

    public void Handle(string text)
    {
        try
        {
            //Запись события в лог
            EventLog.WriteEntry(
                source: EventLogSource,
                message: text,
                type: EventLogEntryType.Information,
                eventID: 1000
            );
        }
        catch (Exception ex)
        {
            // Если запись не удалась
            Console.WriteLine($"[Error writing to Event Log] {ex.Message}");
        }
    }
}*/

public class SyslogHandler : ILogHandler
{
    public void Handle(string text)
    {
        Console.WriteLine($"[Syslog] {text}");
    }
}

//Логирование
public class Logger
{
    private readonly List<ILogFilter> _filters;//список фильтров
    private readonly List<ILogHandler> _handlers;//список обработчиков

    public Logger(IEnumerable<ILogFilter> filters, IEnumerable<ILogHandler> handlers)
    {
        _filters = new List<ILogFilter>(filters);
        _handlers = new List<ILogHandler>(handlers);
    }

    public void Log(string text)
    {
        //Применяем все фильтры
        foreach (var filter in _filters)
        {
            if (!filter.Match(text))
                return; //сообщение не прошло фильтр
        }

        //Передаем сообщение всем обработчикам
        foreach (var handler in _handlers)
        {
            try
            {
                handler.Handle(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in handler {handler.GetType().Name}: {ex.Message}");
            }
        }
    }

    //Удобные методы для логирования с уровнем
    public void Trace(string message) => Log($"[Trace] {message}");
    public void Debug(string message) => Log($"[Debug] {message}");
    public void Info(string message) => Log($"[Info] {message}");
    public void Warning(string message) => Log($"[Warning] {message}");
    public void Error(string message) => Log($"[Error] {message}");
    public void Critical(string message) => Log($"[Critical] {message}");
}

namespace lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            //создаем фильтры
            var filters = new List<ILogFilter>
            {
                new LevelLogFilter(LogLevel.Info),
                new ReLogFilter(@"error|warning", RegexOptions.IgnoreCase) //Только ошибки и предупреждения
            };

            //Создаем обработчики
            var handlers = new List<ILogHandler>
            {
                new ConsoleHandler(),
                new FileHandler("app.log"),
                new SyslogHandler()//по сути тут не нужен тк это тот же вывод в консоль , но в общей системе нужен 
            };

            //Создаем логгер
            var logger = new Logger(filters, handlers);

            logger.Error("Ошибка в модуле");
            logger.Trace("Это сообщение не будет обработано (уровень Trace)");
            logger.Debug("Это сообщение не будет обработано (уровень Debug)");
            logger.Info("Информационное сообщение");
            logger.Warning("Предупреждение: что-то пошло не так");
            logger.Critical("Критическая ошибка! Приложение будет закрыто");

            //Фильтр по подстроке
            var userLogger = new Logger(
                new[] { new SimpleLogFilter("User") },//вместо level и поиска []  ищем по слову
                new[] { new ConsoleHandler() }
            );

            userLogger.Info("User admin logged in");
            userLogger.Info("System started");
        }
    }
}
