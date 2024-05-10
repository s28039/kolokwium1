namespace kol1_test_3.DTOs;

public class BookDTO
{
    public int Pk { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Author> Authors { get; set; } = null!;
}

public class Author
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}