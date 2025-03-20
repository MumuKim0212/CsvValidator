using System.Collections.Generic;
using CsvValidator.Enums;

namespace CsvValidator.Models
{
    public class PolicyConfig
    {
        public List<PolicyDefinition> Policies { get; set; } = [];
    }

    public class PolicyDefinition
    {
        public string FileName { get; set; } = string.Empty;
        public List<ColumnRuleDefinition> ColumnRules { get; set; } = [];
        public List<CustomRuleDefinition> CustomRules { get; set; } = [];
    }

    public class ColumnRuleDefinition
    {
        public string ColumnName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
    }

    public class CustomRuleDefinition
    {
        public string ColumnName { get; set; } = string.Empty;
        public RuleType RuleType { get; set; }
        public string Parameter { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
    public class ColumnRule
    {
        public ColumnType Type { get; set; }
        public bool IsRequired { get; set; }
    }

    public class CustomRule
    {
        public string ColumnName { get; set; } = string.Empty;
        public Func<object, bool>? Validation { get; set; } = null;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}