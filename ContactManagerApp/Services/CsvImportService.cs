using System.Globalization;
using ContactManagerApp.DTO;
using ContactManagerApp.Mapping;
using ContactManagerApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;

namespace ContactManagerApp.Services;

public interface ICsvImportService
{
    Task<CsvImportResult> ImportContactsFromCsvAsync(IFormFile file);
}

public class CsvImportService:  ICsvImportService
{
    private readonly IContactService _contactService;
    private readonly ILogger<CsvImportService> _logger;
    private readonly IValidator<ContactDto> _validator;

    public CsvImportService(IContactService contactService,  ILogger<CsvImportService> logger,IValidator<ContactDto> validator)
    {
        _contactService = contactService;
        _logger = logger;
        _validator = validator;
    }
    
    public async Task<CsvImportResult> ImportContactsFromCsvAsync(IFormFile file)
    {
        _logger.LogInformation("Starting CSV import, file name: {FileName}", file.FileName);
        
        var result = new CsvImportResult();

        if (file == null || file.Length == 0)
        {
            result.Errors.Add("File is empty.");
            return result;
        }
        
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
        };

        using var csv = new CsvReader(reader, csvConfig);
        csv.Context.RegisterClassMap<ContactMap>();

       List<ContactDto> records;
        try
        {
            records = csv.GetRecords<ContactDto>().ToList();
        }
        catch (CsvHelperException ex)
        {
            result.Errors.Add($"CSV parsing error: {ex.Message}");
            _logger.LogError(ex, "CSV parsing failed.");
            return result;
        }

        foreach (var contactDto in records)
        {
            var validationResult = await _validator.ValidateAsync(contactDto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                
                _logger.LogWarning("Row {RowNumber} validation errors: {Errors}", records.IndexOf(contactDto) + 1, errors);
                
                var rowNum = records.IndexOf(contactDto) + 2;
                
                result.Errors.Add($"Row {rowNum} validation errors: {errors}");
                
                continue;
            }
            
            await _contactService.CreateAsync(contactDto);
        }
        
        return result;
    }
}