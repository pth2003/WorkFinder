using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkFinder.Web.Models
{
    public class SocialLink : BaseEntity
    {
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public string Platform { get; set; }  // Facebook, LinkedIn, Twitter, Instagram, etc.
        public string Url { get; set; }
    }
}