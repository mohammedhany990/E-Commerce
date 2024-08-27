using AutoMapper;
using Demo.DAL.Models;
using Demo.PL.Models;

namespace Demo.PL.MappingProfile
{
    public class MappingProfiles : Profile
    {

        public MappingProfiles()
        {
            CreateMap<CategoryViewModel, Category>().ReverseMap();
            CreateMap<ProductViewModel, Product>().ReverseMap();
            CreateMap<Company, CompanyViewModel>().ReverseMap();
        }
    }
}
