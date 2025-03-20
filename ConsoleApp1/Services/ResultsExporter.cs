using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvValidator.Models;

namespace CsvValidator.Services
{
    public class ResultsExporter
    {
        private readonly bool _exportResults;
        private readonly string _exportPath;
        private readonly LoggerService _logger;

        public ResultsExporter(AppSettings settings, LoggerService logger)
        {
            _exportResults = settings.ExportResults;
            _exportPath = settings.ResultsExportPath;
            _logger = logger;
        }

        public void ExportResults(List<ValidationResult> results)
        {
            if (_exportResults == false)
            {
                return;
            }

            try
            {
                // 디렉토리 확인 및 생성
                string? exportDirectory = Path.GetDirectoryName(_exportPath);
                if (string.IsNullOrEmpty(exportDirectory) == false && Directory.Exists(exportDirectory) == false)
                {
                    Directory.CreateDirectory(exportDirectory);
                }

                // CSV 헤더 및 데이터 생성
                var sb = new StringBuilder();
                sb.AppendLine("파일명,검증 결과,오류 개수,오류 상세");

                foreach (var result in results)
                {
                    string errorDetails = result.Errors.Any() ? $"\"{string.Join("; ", result.Errors).Replace("\"", "\"\"")}\"" : "";

                    sb.AppendLine($"{result.FileName},{(result.IsValid ? "유효함" : "유효하지 않음")},{result.Errors.Count},{errorDetails}");
                }

                // 파일 저장
                File.WriteAllText(_exportPath, sb.ToString());
                _logger.LogInfo($"검증 결과가 성공적으로 저장됐습니다: {_exportPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"결과를 저장하는 중 오류가 발생했습니다: {ex.Message}");
            }
        }
    }
}