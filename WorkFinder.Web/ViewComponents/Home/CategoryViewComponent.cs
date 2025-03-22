using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace WorkFinder.Web.ViewComponents.Home;

public class CategoryViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(int count = 8)
    {
        var categories = new List<CategoryItem>
        {
            new CategoryItem { Id = 1, Name = "Anesthesiologists", Jobs = 45904, Icon = "fas fa-heartbeat" },
            new CategoryItem { Id = 2, Name = "Surgeons", Jobs = 50364, Icon = "fas fa-user-md" },
            new CategoryItem { Id = 3, Name = "Obstetricians-Gynecologists", Jobs = 4339, Icon = "fas fa-baby" },
            new CategoryItem { Id = 4, Name = "Orthodontists", Jobs = 20079, Icon = "fas fa-tooth" },
            new CategoryItem { Id = 5, Name = "Maxillofacial Surgeons", Jobs = 74875, Icon = "fas fa-procedures" },
            new CategoryItem { Id = 6, Name = "Software Developer", Jobs = 43359, Icon = "fas fa-laptop-code" },
            new CategoryItem { Id = 7, Name = "Psychiatrists", Jobs = 18599, Icon = "fas fa-brain" },
            new CategoryItem { Id = 8, Name = "Data Scientist", Jobs = 28200, Icon = "fas fa-chart-bar", IsHighlighted = true },
            new CategoryItem { Id = 9, Name = "Financial Manager", Jobs = 61391, Icon = "fas fa-money-bill-wave" },
            new CategoryItem { Id = 10, Name = "Management Analysis", Jobs = 93046, Icon = "fas fa-chart-line" },
            new CategoryItem { Id = 11, Name = "IT Manager", Jobs = 50963, Icon = "fas fa-server" },
            new CategoryItem { Id = 12, Name = "Operations Research Analysis", Jobs = 16627, Icon = "fas fa-cogs" }
        };

        // Lấy count danh mục đầu tiên từ danh sách
        var topCategories = categories.Take(count).ToList();

        return View(topCategories);
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Jobs { get; set; }
        public bool IsHighlighted { get; set; } = false;
        public string Icon { get; set; }
    }
}