namespace FinancialManagement.Api.DTOs.Category;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}