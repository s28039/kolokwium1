using kol1_test_3.DTOs;
using kol1_test_3.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace kol1_test_3.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;
    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }
    
    [HttpGet("{id}/authors")]
    public async Task<IActionResult> GetBookWithAuthors(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
            return NotFound($"Book with given ID - {id} doesn't exist");

        var book = await _booksRepository.GetBookWithAuthors(id);
            
        return Ok(book);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBook(NewBookDTO newBookDTO)
    {
        if (await _booksRepository.DoesBookExist(newBookDTO.Title))
            return NotFound($"Book with given title - {newBookDTO.Title} exists");

        foreach (var author in newBookDTO.Authors)
        {
            if (!await _booksRepository.DoesAuthorExist(author.FirstName, author.LastName))
                return NotFound($"Author with given first name and last name - {author.FirstName} {author.LastName} doesn't exist");
        }

        BookDTO book = await _booksRepository.AddNewBookWithAuthors(newBookDTO);

        return Created(Request.Path.Value ?? "api/books", book);
    }
}