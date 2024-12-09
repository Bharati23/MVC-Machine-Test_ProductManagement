namespace ProductCategoryApp.Models
{
    public class PaginatedProductViewModel
    {
        public IEnumerable<Product> Items { get; set; } = new List<Product>();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
