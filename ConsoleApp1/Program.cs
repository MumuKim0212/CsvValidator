using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvValidator.Services;
using CsvValidator.Models;

namespace CsvValidator
{
    class Program
    {
        private static LoggerService? _logger;
        private static AppSettings? _appSettings;

        static void Main(string[] args)
        {
            Console.WriteLine("CSV 파일 정책 검증용 프로그램입니다.");
            Console.WriteLine("==================================");

            // 설정 및 로거 초기화
            InitializeConfig();
            if (_appSettings == null || _logger == null)
            {
                return;
            }

            // 정책 로드
            var policyManager = LoadPolicy();
            if (policyManager == null)
            {
                return;
            }

            // 검사 폴더 경로 확인
            var scanFolder = GetScanFolderPath();
            if (scanFolder == null)
            {
                return;
            }

            // CSV 파일 검증
            var results = ValidateFiles(scanFolder, policyManager);

            // 결과 출력 및 내보내기
            WriteResult(results);
            ExportResult(results);

            _logger?.LogInfo("프로그램을 종료하려면 아무 키나 누르세요");
            Console.ReadKey();
        }

        private static void InitializeConfig()
        {
            // appsettings에서 설정 로드
            var configLoader = new ConfigLoader();
            var appConfig = configLoader.LoadConfig();

            _appSettings = appConfig.AppSettings;
            _logger = new LoggerService(_appSettings);
        }

        private static PolicyManager? LoadPolicy()
        {
            // 설정된 경로에 정책파일이 없으면 사용자에게 경로 입력 요청
            string? policyFilePath = _appSettings?.PolicyFilePath;
            if (!File.Exists(policyFilePath))
            {
                _logger?.LogWarning($"정책 파일({policyFilePath})이 없습니다.");
                _logger?.LogInfo("정책 파일의 전체 경로를 입력하세요 (취소하려면 빈 값 입력): ");

                string? customPath = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(customPath))
                {
                    _logger?.LogInfo("프로그램을 종료합니다.");
                    return null;
                }

                policyFilePath = customPath;
                if (!File.Exists(policyFilePath))
                {
                    _logger?.LogError($"정책 파일({policyFilePath})을 찾을 수 없습니다. 프로그램을 종료합니다.");
                    return null;
                }
            }

            // 정책파일 로드
            var policyConfigLoader = new PolicyConfigLoader();
            var policyConfig = policyConfigLoader.LoadFromFile(policyFilePath);
            if (policyConfig == null)
            {
                _logger?.LogError("정책 파일을 로드할 수 없습니다. 프로그램을 종료합니다.");
                return null;
            }

            var policyManager = policyConfigLoader.CreatePolicyManager(policyConfig);
            _logger?.LogInfo($"{policyConfig.Policies.Count}개의 정책을 로드했습니다.");

            return policyManager;
        }

        private static string? GetScanFolderPath()
        {
            string? scanFolder = _appSettings?.DefaultScanFolder;
            Console.WriteLine($"검사할 루트 폴더 경로를 입력하세요 (기본값: {scanFolder}): ");
            string? inputFolder = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(inputFolder))
            {
                scanFolder = inputFolder;
            }

            if (!Directory.Exists(scanFolder))
            {
                _logger?.LogError($"유효하지 않은 폴더 경로입니다: {scanFolder}");
                return null;
            }

            return scanFolder;
        }

        private static List<ValidationResult> ValidateFiles(string scanFolder, PolicyManager policyManager)
        {
            // 폴더 내 모든 CSV 파일 찾기
            SearchOption searchOption = _appSettings?.IncludeSubfolders == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var csvFiles = Directory.GetFiles(scanFolder, "*.csv", searchOption);
            _logger?.LogInfo($"{csvFiles.Length}개의 CSV 파일을 찾았습니다.");

            var validator = new Validator();
            var results = new List<ValidationResult>();

            // 각 파일 검증
            foreach (var file in csvFiles)
            {
                var result = ValidateFile(file, policyManager, validator);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        private static ValidationResult? ValidateFile(string file, PolicyManager policyManager, Validator validator)
        {
            var fileName = Path.GetFileName(file);
            _logger?.LogInfo($"검증 중: {fileName}");

            // 파일 이름에 맞는 정책 가져오기
            Policy? policy = policyManager.GetPolicy(fileName);
            if (policy == null)
            {
                _logger?.LogWarning($"{fileName}에 대한 정책이 정의되지 않았습니다. 검증을 건너뜁니다.");
                return null;
            }

            // 파일 검증
            ValidationResult result = validator.Validate(file, policy);

            _logger?.LogInfo($"결과: {(result.IsValid ? "유효함" : "유효하지 않음")}");
            foreach (var error in result.Errors)
            {
                _logger?.LogError(error);
            }

            return result;
        }

        private static void WriteResult(List<ValidationResult> results)
        {
            int validCount = results.Count(r => r.IsValid);
            _logger?.LogInfo("\n검증 결과 요약:");
            _logger?.LogInfo($"총 검사 파일: {results.Count}");
            _logger?.LogInfo($"유효한 파일: {validCount}");
            _logger?.LogInfo($"유효하지 않은 파일: {results.Count - validCount}");
        }

        private static void ExportResult(List<ValidationResult> results)
        {
            var resultsExporter = new ResultsExporter(_appSettings, _logger);
            resultsExporter.ExportResults(results);
        }
    }
}