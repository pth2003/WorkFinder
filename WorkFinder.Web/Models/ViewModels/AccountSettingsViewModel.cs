using System.Collections.Generic;

namespace WorkFinder.Web.Models.ViewModels;

public class AccountSettingsViewModel
{
    public ProfileViewModel Profile { get; set; } = new ProfileViewModel();
    public Resume Resume { get; set; }

    // Additional properties for other settings categories
    public string Title { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string PersonalWebsite { get; set; } = string.Empty;
}