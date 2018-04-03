using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;
using WebApplication3.Models.ApiViewModels;

namespace WebApplication3.AutoMapper
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserAddBindingModel>().ReverseMap();
            CreateMap<Service, ServiceBindingModel>().ReverseMap();
            CreateMap<Customer, UserAddBindingModel>().ReverseMap();
            CreateMap<UserInfoBindingModel, UserAddBindingModel>().ReverseMap();
            CreateMap<ApplicationUser, UserInfoBindingModel>().ReverseMap();
            CreateMap<Customer, CustomerInfoBindingModel>().ReverseMap();
            CreateMap<Customer, CustomerStatsModel>().ReverseMap();
            CreateMap<UserInfoBindingModel, ApplicationUser>().ReverseMap();
            CreateMap<AddVisitBindingModel, Visit>().ReverseMap();
            CreateMap<Visit, AddVisitBindingModel>().ReverseMap();
            CreateMap<ServiceBindingModel, Service>().ReverseMap();
            CreateMap<RegisterBindingModel, ApplicationUser>().ReverseMap();
            CreateMap<VisitTerm, VisitTermInfoBindingModel>().ReverseMap();
            CreateMap<Visit, VisitInfoBindingModel>().ReverseMap();
            CreateMap<Customer, CustomerSetDiscountModel>().ReverseMap();
            CreateMap<Visit, UserVisitInfoBindingModel>().ReverseMap();
            CreateMap<UpdateVisitBindingModel, Visit>().ReverseMap();
            CreateMap<CustomerFullStatsModel, CustomerStatsApiModel>().ReverseMap();
        }
    }
}