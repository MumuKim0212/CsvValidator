using System.Collections.Generic;

namespace CsvValidator.Models
{
    public class ValidationResult
    {
        public string FileName { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }

        public ValidationResult(string fileName)
        {
            FileName = fileName;
            IsValid = true;
            Errors = new List<string>();
        }

        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }
    }
}