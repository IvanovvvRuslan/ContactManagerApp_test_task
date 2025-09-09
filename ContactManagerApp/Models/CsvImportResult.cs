using System.Net.Security;

namespace ContactManagerApp.Models;

public class CsvImportResult
{
    public bool Success => !Errors.Any();
    public List<string> Errors { get; set; } = new();
}