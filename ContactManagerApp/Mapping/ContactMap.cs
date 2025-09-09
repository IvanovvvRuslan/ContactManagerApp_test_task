using ContactManagerApp.DTO;
using CsvHelper.Configuration;

namespace ContactManagerApp.Mapping;

public class ContactMap: ClassMap<ContactDto>
{
    public ContactMap()
    {
        Map(m => m.Id).Ignore();
        Map(m => m.Name);
        Map(m => m.BirthDate);
        Map(m => m.IsMarried);
        Map(m => m.PhoneNumber);
        Map(m => m.Salary);
    }
}