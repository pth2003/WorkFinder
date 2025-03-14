using Microsoft.AspNetCore.Mvc;

namespace WorkFinder.Web.ViewComponents.Home;

public class SearchViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}