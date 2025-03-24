using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.ViewComponents.Home;

public class TopCompanyViewComponent : ViewComponent
{
    private readonly ICompanyRepository _companyRepository;

    public TopCompanyViewComponent(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var companies = await _companyRepository.GetTopCompaniesAsync(8);
        return View(companies);
    }
}