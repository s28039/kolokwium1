using kol1_test_3.DTOs;

namespace kol1_test_3.Repositories;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<bool> DoesBookExist(string title);
    Task<bool> DoesAuthorExist(string fName, string sName);
    Task<BookDTO> GetBookWithAuthors(int id);
    
    // Version with implicit transaction
    Task<BookDTO> AddNewBookWithAuthors(NewBookDTO newBookDTO);
}