namespace PetClinic.App
{
    using AutoMapper;
    using DataProcessor.Dtos.Import;
    using Models;
    using System;
    using System.Globalization;

    public class PetClinicProfile : Profile
    {
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE THIS CLASS
        public PetClinicProfile()
        {
            CreateMap<AnimalDto, Animal>();

            CreateMap<PassportDto, Passport>()
                .ForMember(p => p.RegistrationDate,
                    rd => rd.MapFrom(dto =>
                         DateTime.ParseExact(dto.RegistrationDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)));

            CreateMap<VetDto, Vet>();
        }
    }
}
