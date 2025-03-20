using System;
using System.Collections.Generic;
using CsvValidator.Enums;

namespace CsvValidator.Models
{
    public class Policy
    {
        public string FileName { get; private set; }
        private Dictionary<string, ColumnRule> ColumnRules;
        private List<CustomRule> CustomRules;

        public Policy(string fileName)
        {
            FileName = fileName;
            ColumnRules = new Dictionary<string, ColumnRule>();
            CustomRules = new List<CustomRule>();
        }

        public void AddColumnRule(string columnName, ColumnType type, bool isRequired)
        {
            ColumnRules[columnName] = new ColumnRule { Type = type, IsRequired = isRequired };
        }

        public void AddCustomRule(string columnName, Func<object, bool> validation, string errorMessage)
        {
            CustomRules.Add(new CustomRule
            {
                ColumnName = columnName,
                Validation = validation,
                ErrorMessage = errorMessage
            });
        }

        public Dictionary<string, ColumnRule> GetColumnRules()
        {
            return ColumnRules;
        }

        public List<CustomRule> GetCustomRules()
        {
            return CustomRules;
        }
    }
}