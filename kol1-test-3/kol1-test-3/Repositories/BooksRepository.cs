using kol1_test_3.DTOs;
using Microsoft.Data.SqlClient;

namespace kol1_test_3.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;
    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM books WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesBookExist(string title)
    {
        var query = "SELECT 1 FROM books WHERE title = @Title";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@Title", title);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesAuthorExist(string fName, string sName)
    {
        var query = "SELECT 1 FROM authors WHERE first_name = @FName AND last_name = @SName";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@FName", fName);
        command.Parameters.AddWithValue("@SName", sName);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<BookDTO> GetBookWithAuthors(int id)
    {
        var query = @"SELECT
						    books.PK AS BookID,
						    books.title AS BookTitle,
						    authors.first_name as AuthorFName,
						    authors.last_name as AuthorSName
						FROM books
						JOIN books_authors ON books_authors.FK_book = books.PK
						JOIN authors ON authors.PK = books_authors.FK_author
						WHERE books.PK = @ID";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@ID", id);
	    
	    await connection.OpenAsync();

	    var reader = await command.ExecuteReaderAsync();

	    var bookIdOrdinal = reader.GetOrdinal("BookID");
	    var bookTitleOrdinal = reader.GetOrdinal("BookTitle");
	    var authorFNameOrdinal = reader.GetOrdinal("AuthorFName");
	    var authorSNameOrdinal = reader.GetOrdinal("AuthorSName");

	    BookDTO bookDTO = null;

	    while (await reader.ReadAsync())
	    {
		    if (bookDTO is not null)
		    {
			    bookDTO.Authors.Add(new Author()
			    {
				    FirstName = reader.GetString(authorFNameOrdinal),
				    LastName = reader.GetString(authorSNameOrdinal)
			    });
		    }
		    else
		    {
			    bookDTO = new BookDTO()
			    {
				    Pk = reader.GetInt32(bookIdOrdinal),
				    Title = reader.GetString(bookTitleOrdinal),
				    Authors = new List<Author>()
				    {
					    new Author()
					    {
						    FirstName = reader.GetString(authorFNameOrdinal),
						    LastName = reader.GetString(authorSNameOrdinal)
					    }
				    }
			    };
		    }
	    }

	    if (bookDTO is null) throw new Exception();
        
        return bookDTO;
    }

    public async Task<BookDTO> AddNewBookWithAuthors(NewBookDTO newBookDTO)
    {
	    var insert = @"INSERT INTO books VALUES(@Title);
					   SELECT @@IDENTITY AS ID;";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();
	    
	    command.Connection = connection;
	    command.CommandText = insert;
	    
	    command.Parameters.AddWithValue("@Title", newBookDTO.Title);
	    
	    await connection.OpenAsync();

	    var transaction = await connection.BeginTransactionAsync();
	    command.Transaction = transaction as SqlTransaction;

	    try
	    {
		    var id = await command.ExecuteScalarAsync();
    
		    foreach (var author in newBookDTO.Authors)
		    {
			    command.Parameters.Clear();

			    command.CommandText = @"Select Pk from authors where first_name = @AuthorFName and last_name = @AuthorLName;";
			    command.Parameters.AddWithValue("@AuthorFName", author.FirstName);
			    command.Parameters.AddWithValue("@AuthorLName", author.LastName);
			    var authId = await command.ExecuteScalarAsync();
			    
			    command.Parameters.Clear();
			    command.CommandText = "INSERT INTO books_authors VALUES(@ID, @AuthId)";
			    command.Parameters.AddWithValue("@AuthId", authId);
			    command.Parameters.AddWithValue("@ID", id);

			    await command.ExecuteNonQueryAsync();
		    }

		    await transaction.CommitAsync();

		    return new BookDTO()
		    {
			    Pk = Convert.ToInt32(id),
				Title = newBookDTO.Title,
				Authors = newBookDTO.Authors as List<Author>
		    };
	    }
	    catch (Exception)
	    {
		    await transaction.RollbackAsync();
		    throw;
	    }
    }
}