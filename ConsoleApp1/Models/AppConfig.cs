namespace CsvValidator.Models
{
    public class AppConfig
    {
        public AppSettings AppSettings { get; set; } = new();
    }

    public class AppSettings
    {
        public string DefaultScanFolder { get; set; } = Environment.CurrentDirectory;
        public string PolicyFilePath { get; set; } = string.Empty;
        public bool IncludeSubfolders { get; set; }
        public bool LogToFile { get; set; }
        public string LogFilePath { get; set; } = string.Empty;
        public bool ExportResults { get; set; }
        public string ResultsExportPath { get; set; } = string.Empty;
    }
}