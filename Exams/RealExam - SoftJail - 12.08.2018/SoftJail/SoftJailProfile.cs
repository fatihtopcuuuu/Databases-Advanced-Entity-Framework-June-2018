namespace SoftJail
{
    using System;
    using System.Globalization;
    using AutoMapper;
    using Data.Models;
    using DataProcessor.ImportDto.Department;
    using DataProcessor.ImportDto.Prisoner;

    public class SoftJailProfile : Profile
    {
        private const string DateTimeFormat = "dd/MM/yyyy";
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE THIS CLASS
        public SoftJailProfile()
        {
            CreateMap<DepartmentDto, Department>();

            CreateMap<DepartmentCellDto, Cell>();

            CreateMap<PrisonerDto, Prisoner>()
                .ForMember(dest => dest.IncarcerationDate,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ReleaseDate,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IncarcerationDate,
                    opt => opt.MapFrom(src => DateTime.ParseExact(src.IncarcerationDate, DateTimeFormat,
                         CultureInfo.InvariantCulture)));

            CreateMap<PrisonerMailDto, Mail>();
        }
    }
}
