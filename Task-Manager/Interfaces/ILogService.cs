namespace Task_Manager.Interfaces;

public interface ILogService
{
    Task Info(string message, string? source = null);
    Task Error(string message, Exception ex, string? source = null);
}