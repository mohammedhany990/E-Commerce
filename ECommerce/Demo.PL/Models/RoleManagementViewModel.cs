using Demo.DAL.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Demo.PL.Models
{
    public class RoleManagementViewModel
    {
        public AppUser AppUser { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }
        public IEnumerable<SelectListItem> CompanyList { get; set; }
    }
}
