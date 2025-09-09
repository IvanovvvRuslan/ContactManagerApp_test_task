using ContactManagerApp.Data;
using ContactManagerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactManagerApp.Repository;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<PagedResult<Contact>> GetPagedAsync(PaginationRequest request);
    Task<Contact?> GetByIdAsync(int id);
    Task<Contact?> GetByIdTrackedAsync(int id);
    Task CreateAsync(Contact contact);
    Task Delete (Contact contact);
    Task SaveChangesAsync();
    
}

public class ContactRepository: IContactRepository
{
    private readonly ApplicationDbContext _context;

    public ContactRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        return await _context.Contacts.ToListAsync();
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

    public async Task<Contact?> GetByIdAsync(int id)
    {
        var contact = await _context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(contact => contact.Id == id);
        
        return contact;
    }

    public async Task<Contact?> GetByIdTrackedAsync(int id)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(contact => contact.Id == id);
        
        return contact;
    }

    public async Task CreateAsync(Contact contact)
    {
        await _context.Contacts.AddAsync(contact);
    }

    public Task Delete(Contact contact)
    {
        _context.Contacts.Remove(contact);
        
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}