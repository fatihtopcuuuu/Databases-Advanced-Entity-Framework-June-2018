namespace FastFood.App
{
    using AutoMapper;
    using DataProcessor.Dtos.Import;
    using Models;

    public class FastFoodProfile : Profile
	{
		// Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE THIS CLASS
		public FastFoodProfile()
		{
            CreateMap<EmployeeDto, Employee>()
                .ForMember(
                    dest=> dest.Name,
                    opt=> opt.MapFrom(src=> src.Name))
                .ForMember(
                    dest=> dest.Age,
                    opt=> opt.MapFrom(src=> src.Age))
                .ForMember(
                    dest=> dest.Position,
                    opt=> opt.Ignore())
                .ForAllOtherMembers(opt=> opt.Ignore());
		    CreateMap<ItemDto, Item>()
		        .ForMember(
		            dest => dest.Name,
		            opt => opt.MapFrom(src => src.Name))
		        .ForMember(
		            dest=> dest.Price,
		            opt=> opt.MapFrom(src=> src.Price))
		        .ForMember(
		            dest=> dest.Category,
		            opt=> opt.Ignore());
		}
	}
}
