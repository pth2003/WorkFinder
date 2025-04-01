using WorkFinder.Web.Areas.Employer.Models;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Areas.Employer.Services
{
    public interface ICompanySetupService
    {
        // Lưu thông tin từ bước 1 (basic)
        Task SaveBasicInfoAsync(CompanySetupBasicViewModel model);

        // Lưu thông tin từ bước 2 (organization)
        Task SaveOrganizationInfoAsync(CompanySetupOrganizationViewModel model);

        // Lưu thông tin từ bước 3 (social)
        Task SaveSocialInfoAsync(CompanySetupSocialViewModel model);

        // Lưu thông tin từ bước 4 (contact) và hoàn tất quá trình setup
        Task<Company> CompleteSetupAsync(CompanySetupContactViewModel model);

        // Lấy thông tin đã lưu trữ trong session
        CompanySetupBasicViewModel GetBasicInfo();
        CompanySetupOrganizationViewModel GetOrganizationInfo();
        CompanySetupSocialViewModel GetSocialInfo();
        CompanySetupContactViewModel GetContactInfo();

        // Kiểm tra xem người dùng đã hoàn thành các bước trước đó chưa
        bool HasCompletedBasicInfo();
        bool HasCompletedOrganizationInfo();
        bool HasCompletedSocialInfo();

        // Initialize setup service with existing company data for profile editing
        Task InitializeWithExistingCompanyAsync(Company company);
    }
}