namespace FastFood.App
{
    using AutoMapper;
    using DataProcessor.Dto.Import;
    using Models;

    public class FastFoodProfile : Profile
    {
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE THIS CLASS
        public FastFoodProfile()
        {
            CreateMap<EmployeeDto, Employee>()
                .ForMember(dest => dest.Position,
                    opt => opt.Ignore());

            CreateMap<ItemDto, Item>()
                .ForMember(dest => dest.Category,
                    opt => opt.Ignore());
        }
    }
}
