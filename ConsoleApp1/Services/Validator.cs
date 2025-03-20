using System;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvValidator.Models;

namespace CsvValidator.Services
{
    public class Validator
    {
        public ValidationResult Validate(string filePath, Policy policy)
        {
            var result = new ValidationResult(Path.GetFileName(filePath));

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    // 헤더 읽기
                    csv.Read();
                    csv.ReadHeader();
                    var headers = csv.HeaderRecord;

                    // 필수 칼럼이 모두 있는지 확인
                    foreach (var columnRule in policy.GetColumnRules())
                    {
                        if (headers?.Contains(columnRule.Key) == false)
                        {
                            result.AddError($"필수 칼럼이 없습니다: {columnRule.Key}");
                        }
                    }

                    // 헤더 다음부터 각 행 검증
                    int rowIndex = 1;
                    while (csv.Read())
                    {
                        rowIndex++;

                        // 칼럼 규칙 검증
                        foreach (var columnRule in policy.GetColumnRules())
                        {
                            string columnName = columnRule.Key;

                            // 칼럼이 존재하지 않으면 건너뛰기 (필수 칼럼은 이미 헤더 검증에서 오류 보고)
                            if (headers?.Contains(columnName) == false)
                            {
                                continue;
                            }

                            string? value = csv.GetField(columnName);
                            if (columnRule.Value.IsRequired && string.IsNullOrWhiteSpace(value))
                            {
                                result.AddError($"행 {rowIndex}, 칼럼 {columnName}: 필수 값이 비어 있습니다.");
                                continue;
                            }

                            // 값이 비어있어도 필수가 아니면 검증하지 않음
                            if (string.IsNullOrWhiteSpace(value) && !columnRule.Value.IsRequired)
                            {
                                continue;
                            }

                            // 데이터 타입 검증
                            bool typeValid = true;
                            switch (columnRule.Value.Type)
                            {
                                case Enums.ColumnType.Numeric:
                                    typeValid = decimal.TryParse(value, out _);
                                    break;
                                case Enums.ColumnType.Date:
                                    typeValid = DateTime.TryParse(value, out _);
                                    break;
                                case Enums.ColumnType.Boolean:
                                    typeValid = bool.TryParse(value, out _);
                                    break;
                                    // String 타입은 항상 유효
                            }

                            if (!typeValid)
                            {
                                result.AddError($"행 {rowIndex}, 칼럼 {columnName}: 값 {value}이 예상 타입 {columnRule.Value.Type}과 일치하지 않습니다.");
                            }
                        }

                        // 커스텀 룰 검증
                        foreach (var customRule in policy.GetCustomRules())
                        {
                            if (headers?.Contains(customRule.ColumnName) == false)
                            {
                                continue;
                            }

                            string? value = csv.GetField(customRule.ColumnName);
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                continue;
                            }

                            try
                            {
                                if (!customRule.Validation(value))
                                {
                                    result.AddError($"행 {rowIndex}, 칼럼 {customRule.ColumnName}: {customRule.ErrorMessage}");
                                }
                            }
                            catch (Exception ex)
                            {
                                result.AddError($"행 {rowIndex}, 칼럼 {customRule.ColumnName}: 규칙 검증 중 오류 발생: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddError($"파일 검증 중 오류 발생: {ex.Message}");
            }

            return result;
        }
    }
}