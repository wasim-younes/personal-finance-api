namespace thepiapi.Models.DTOs
{
    public class CategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public string? Type { get; set; }
    }
}