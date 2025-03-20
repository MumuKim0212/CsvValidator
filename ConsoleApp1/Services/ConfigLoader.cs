using System;
using System.IO;
using System.Text.Json;
using CsvValidator.Models;

namespace CsvValidator.Services
{
    public class ConfigLoader
    {
        private const string DEFAULT_CONFIG_PATH = "appsettings.json";

        /// <summary>
        /// appsettings.json 파일을 읽어 AppConfig 객체로 변환
        /// </summary>
        public AppConfig LoadConfig(string? configPath = null)
        {
            string filePath = configPath ?? DEFAULT_CONFIG_PATH;

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"설정 파일을 찾을 수 없습니다: {filePath}. 기본 설정을 사용합니다.");
                    return CreateDefaultConfig();
                }

                string jsonContent = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var config = JsonSerializer.Deserialize<AppConfig>(jsonContent, options);
                return config ?? CreateDefaultConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"설정 파일을 로드하는 중 오류가 발생했습니다: {ex.Message}");
                Console.WriteLine("기본 설정을 사용합니다.");
                return CreateDefaultConfig();
            }
        }

        private AppConfig CreateDefaultConfig()
        {
            // 실패하면 기본 설정으로 진행
            return new AppConfig
            {
                AppSettings = new AppSettings
                {
                    PolicyFilePath = "samplePolicy.json",
                    IncludeSubfolders = true,
                    LogToFile = false,
                    LogFilePath = "sampleLogs\\sample_logs.txt",
                    ExportResults = false,
                    ResultsExportPath = "sampleResults\\sample_results.csv"
                }
            };
        }
    }
}