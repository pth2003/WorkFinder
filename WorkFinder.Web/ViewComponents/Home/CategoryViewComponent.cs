using Microsoft.AspNetCore.Mvc;

namespace WorkFinder.Web.ViewComponents.Home;

public class CategoryViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var categories = new List<CategoryItem>
        {
            new CategoryItem { Name = "Anesthesiologists", Jobs = 45904 },
            new CategoryItem { Name = "Surgeons", Jobs = 50364 },
            new CategoryItem { Name = "Obstetricians-Gynecologists", Jobs = 4339 },
            new CategoryItem { Name = "Orthodontists", Jobs = 20079 },
            new CategoryItem { Name = "Maxillofacial Surgeons", Jobs = 74875 },
            new CategoryItem { Name = "Software Developer", Jobs = 43359 },
            new CategoryItem { Name = "Psychiatrists", Jobs = 18599 },
            new CategoryItem { Name = "Data Scientist", Jobs = 28200, IsHighlighted = true },
            new CategoryItem { Name = "Financial Manager", Jobs = 61391 },
            new CategoryItem { Name = "Management Analysis", Jobs = 93046 },
            new CategoryItem { Name = "IT Manager", Jobs = 50963 },
            new CategoryItem { Name = "Operations Research Analysis", Jobs = 16627 }
        };
        return View(categories);

    }

    public class CategoryItem
    {
        public string Name { get; set; }
        public int Jobs { get; set; }
        public bool IsHighlighted { get; set; } = false;
    }
}