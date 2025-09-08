using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ContactManagerApp.Models;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    [BindNever]
    [JsonIgnore]
    public int Skip => (PageNumber - 1) * PageSize;
}