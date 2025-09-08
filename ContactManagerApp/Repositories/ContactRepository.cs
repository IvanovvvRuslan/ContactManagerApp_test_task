using ContactManagerApp.Data;
using ContactManagerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactManagerApp.Repository;

public interface IContactRepository
{
    Task<PagedResult<Contact>> GetPagedAsync(PaginationRequest request);
    Task<Contact?> GetByIdTracked(int id);
    Task CreateAsync(Contact contact);
    
    Task SaveChangesAsync();
    
}

public class ContactRepository: IContactRepository
{
    private readonly ApplicationDbContext _context;

    public ContactRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Contact>> GetPagedAsync(PaginationRequest request)
    {
        var totalCount = await _context.Contacts.CountAsync();

        var items = await _context.Contacts
            .AsNoTracking()
            .OrderBy(contact => contact.Name)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<Contact>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<Contact?> GetByIdTracked(int id)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(contact => contact.Id == id);
        
        return contact;
    }

    public async Task CreateAsync(Contact contact)
    {
        await _context.Contacts.AddAsync(contact);
    }

    public async Task SaveChangesAsync()
    {
        _context.SaveChangesAsync();
    }
}