using System;
using System.IO;
using CsvValidator.Models;

namespace CsvValidator.Services
{
    public class LoggerService
    {
        private readonly bool _logToFile;
        private readonly string _logFilePath;

        public LoggerService(AppSettings settings)
        {
            _logToFile = settings.LogToFile;
            _logFilePath = settings.LogFilePath;

            if (_logToFile)
            {
                // 로그 디렉토리 확인 및 생성
                string? logDirectory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 로그 파일 초기화
                File.AppendAllText(_logFilePath, $"=== CSV 검증 로그 시작: {DateTime.Now} ===\r\n");
            }
        }

        public void Log(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            Console.WriteLine(message);

            // 파일에 로깅이 설정됐을때만 파일에도 기록
            if (_logToFile)
            {
                try
                {
                    File.AppendAllText(_logFilePath, logEntry + "\r\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"로그 파일에 기록하는 중 오류가 발생했습니다: {ex.Message}");
                }
            }
        }

        public void LogError(string message)
        {
            Log($"[Error] {message}");
        }

        public void LogWarning(string message)
        {
            Log($"[Warning] {message}");
        }

        public void LogInfo(string message)
        {
            Log($"[Info] {message}");
        }
    }
}