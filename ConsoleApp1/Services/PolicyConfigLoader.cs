using System;
using System.IO;
using System.Text.Json;
using CsvValidator.Models;
using CsvValidator.Enums;
using System.Collections.Generic;

namespace CsvValidator.Services
{
    public class PolicyConfigLoader
    {
        /// <summary>
        /// 정책 파일을 읽어 PolicyConfig 객체로 반환
        /// </summary>
        public PolicyConfig? LoadFromFile(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<PolicyConfig>(jsonContent, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"정책 파일을 로드하는 중 오류가 발생했습니다: {ex.Message}");
                return null;
            }
        }

        public PolicyManager CreatePolicyManager(PolicyConfig config)
        {
            var policyManager = new PolicyManager();
            if (config?.Policies == null)
            {
                return policyManager;
            }

            foreach (var policyDef in config.Policies)
            {
                var policy = new Policy(policyDef.FileName);

                // 컬럼 규칙 추가
                if (policyDef.ColumnRules != null)
                {
                    foreach (var ruleDef in policyDef.ColumnRules)
                    {
                        if (Enum.TryParse<ColumnType>(ruleDef.Type, out var columnType))
                        {
                            policy.AddColumnRule(ruleDef.ColumnName, columnType, ruleDef.IsRequired);
                        }
                        else
                        {
                            Console.WriteLine($"경고: 알 수 없는 컬럼 타입이 발견되었습니다: {ruleDef.Type}. String으로 대체합니다.");
                            policy.AddColumnRule(ruleDef.ColumnName, ColumnType.String, ruleDef.IsRequired);
                        }
                    }
                }

                // 커스텀 규칙 추가
                if (policyDef.CustomRules != null)
                {
                    foreach (var ruleDef in policyDef.CustomRules)
                    {
                        Func<object, bool>? validationFunc = CreateValidationFunction(ruleDef.RuleType, ruleDef.Parameter);
                        if (validationFunc != null)
                        {
                            policy.AddCustomRule(ruleDef.ColumnName, validationFunc, ruleDef.ErrorMessage);
                        }
                    }
                }

                policyManager.AddPolicy(policy);
            }

            return policyManager;
        }

        /// <summary>
        /// 검증 함수를 생성. 실패 시 null 반환
        /// </summary>
        private Func<object, bool>? CreateValidationFunction(RuleType ruleType, string parameter)
        {
            switch (ruleType)
            {
                case RuleType.GreaterThan:
                    if (decimal.TryParse(parameter, out decimal gtValue))
                    {
                        return value => decimal.TryParse(value.ToString(), out decimal actualValue) && actualValue > gtValue;
                    }
                    break;

                case RuleType.GreaterThanOrEqual:
                    if (decimal.TryParse(parameter, out decimal gteValue))
                    {
                        return value => decimal.TryParse(value.ToString(), out decimal actualValue) && actualValue >= gteValue;
                    }
                    break;

                case RuleType.LessThan:
                    if (decimal.TryParse(parameter, out decimal ltValue))
                    {
                        return value => decimal.TryParse(value.ToString(), out decimal actualValue) && actualValue < ltValue;
                    }
                    break;

                case RuleType.LessThanOrEqual:
                    if (decimal.TryParse(parameter, out decimal lteValue))
                    {
                        return value => decimal.TryParse(value.ToString(), out decimal actualValue) && actualValue <= lteValue;
                    }
                    break;

                case RuleType.Equal:
                    return value => value.ToString() == parameter;

                case RuleType.NotEqual:
                    return value => value.ToString() != parameter;

                case RuleType.MinLength:
                    if (int.TryParse(parameter, out int minLength))
                    {
                        return value => value.ToString()?.Length >= minLength;
                    }
                    break;

                case RuleType.MaxLength:
                    if (int.TryParse(parameter, out int maxLength))
                    {
                        return value => value.ToString()?.Length <= maxLength;
                    }
                    break;

                case RuleType.Regex:
                    return value => System.Text.RegularExpressions.Regex.IsMatch(value.ToString(), parameter);

                default:
                    Console.WriteLine($"[Error] 알 수 없는 규칙 타입입니다: '{ruleType}' 정책을 확인해주세요.");
                    return null;
            }

            Console.WriteLine($"[Error] 알 수 없는 규칙 타입입니다: '{ruleType}' 정책을 확인해주세요.");
            return null;
        }
    }
}