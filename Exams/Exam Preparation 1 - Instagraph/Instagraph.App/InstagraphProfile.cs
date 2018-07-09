using AutoMapper;

namespace Instagraph.App
{
    using DataProcessor.Dtos.Export;
    using DataProcessor.Dtos.Import;
    using Models;
    using System.Linq;

    public class InstagraphProfile : Profile
    {
        public InstagraphProfile()
        {
            CreateMap<PictureDto, Picture>();
            CreateMap<UserDto, User>()
                .ForMember(
                    dest => dest.ProfilePicture,
                    opt => opt.Ignore());
            CreateMap<FollowerDto, UserFollower>()
                .ForMember(
                    dest => dest.User,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.Follower,
                    opt => opt.Ignore());
            CreateMap<PostDto, Post>()
                .ForMember(
                    dest => dest.User,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.Picture,
                    opt => opt.Ignore());
            CreateMap<CommentDto, Comment>()
                .ForMember(
                    dest => dest.Content,
                    opt => opt.MapFrom(src => src.Content))
                .ForAllOtherMembers(opt=> opt.Ignore());
            CreateMap<CommentPostDto, Comment>()
                .ForMember(
                    dest=> dest.PostId,
                    opt=> opt.MapFrom(src=> src.Id));
            CreateMap<Post, UncommentedPostDto>()
                .ForMember(
                    dest=> dest.User,
                    opt=> opt.MapFrom(src=> src.User.Username))
                .ForMember(
                    dest=> dest.Picture,
                    opt=> opt.MapFrom(src=> src.Picture.Path));
            CreateMap<User, PopularUserDto>()
                .ForMember(dto => dto.Followers, 
                    f => f.MapFrom(u => u.Followers.Count));
            CreateMap<User, UserExportDto>()
                .ForMember(dto => dto.Username,
                    f => f.MapFrom(u=> u.Username))
                .ForMember(
                    dest=> dest.MostComments
                    ,opt=> opt.MapFrom(src=> src.Posts.Count == 0 ? 0 : src.Posts.Max(p=> p.Comments.Count)));
        }
    }
}
