using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;

namespace WorkFinder.Web.ViewComponents.Home;

public class TopCompanyViewComponent : ViewComponent
{
    private readonly WorkFinderContext _context;
    
    public TopCompanyViewComponent(WorkFinderContext context)
    {
        _context = context;
    }
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var companies = await _context.Companies
            .Take(8)
            .ToListAsync();
        return View(companies);
    }
}