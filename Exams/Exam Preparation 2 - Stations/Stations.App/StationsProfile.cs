using AutoMapper;
using Stations.DataProcessor.Dto.Import;
using Stations.Models;

namespace Stations.App
{
    public class StationsProfile : Profile
    {
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE THIS CLASS
        public StationsProfile()
        {
            CreateMap<StationDto, Station>();
            CreateMap<SeatingClassDto, SeatingClass>();
            CreateMap<TrainDto, Train>()
                .ForMember(
                    dest => dest.TrainSeats,
                    opt => opt.Ignore()
                );
            CreateMap<TripDto, Trip>()
                .ForMember(
                    dest=> dest.Status,
                    opt=> opt.MapFrom(src=> src.Status))
                .ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<CardDto, CustomerCard>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(src => src.Age))
                .ForMember(
                    dest => dest.Type,
                    opt => opt.MapFrom(src => src.CardType));
        }
    }
}
