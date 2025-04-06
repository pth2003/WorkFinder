using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.DTOs.Candidate;

namespace WorkFinder.Web.Models.ViewModels
{
    public class SettingViewModel
    {
        public ResumeSettingDto resumeSettingDto { get; set; }
        public UserSettingDto userSettingDto { get; set; }
    }
}