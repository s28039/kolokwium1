namespace kol1_test_3.DTOs;

public class NewBookDTO
{
    public string Title { get; set; } = string.Empty;
    public IEnumerable<Author> Authors { get; set; } = new List<Author>();
}