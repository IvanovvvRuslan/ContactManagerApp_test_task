namespace ContactManagerApp.Models;

public class Contact
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public bool IsMarried { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public decimal Salary { get; set; }
}