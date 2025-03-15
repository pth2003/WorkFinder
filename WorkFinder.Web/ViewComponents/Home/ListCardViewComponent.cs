using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models.ViewModels;

namespace WorkFinder.Web.ViewComponents.Home;

public class ListCardViewComponent : ViewComponent
{
    private readonly WorkFinderContext _context;
    private static readonly Dictionary<string, string> CategoryIcons = new()
    {
        ["design"] = "fas fa-paint-brush",
        ["development"] = "fas fa-code",
        ["marketing"] = "fas fa-bullhorn",
        ["business"] = "fas fa-briefcase"
    };
    public ListCardViewComponent(WorkFinderContext context)
    {
        _context = context;
    }
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Jobs)
            .Select(c => new CategoryCardViewModel
            {
                Category = c,
                Icon = GetIconForCategory(c.Name) // Helper method
            })
            .ToListAsync();

        return View(categories);
    }
    private static string GetIconForCategory(string categoryName)
    {
        return CategoryIcons.GetValueOrDefault(categoryName.ToLower(), "fas fa-folder");
    }
}