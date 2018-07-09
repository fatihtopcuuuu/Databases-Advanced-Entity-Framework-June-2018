namespace Instagraph.DataProcessor
{
    using AutoMapper;
    using Data;
    using Dtos.Import;
    using Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        public static string ImportPictures(InstagraphContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedPictures = JsonConvert.DeserializeObject<PictureDto[]>(jsonString);

            var validPicutres = new List<Picture>();
            foreach (var pictureDto in deserializedPictures)
            {
                if (!IsValid(pictureDto))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                sb.AppendLine($"Successfully imported Picture {pictureDto.Path}.");
                var picture = Mapper.Map<Picture>(pictureDto);
                validPicutres.Add(picture);
            }

            context.Pictures.AddRange(validPicutres);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportUsers(InstagraphContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedUsers = JsonConvert.DeserializeObject<UserDto[]>(jsonString);

            var validUsers = new List<User>();
            foreach (var userDto in deserializedUsers)
            {
                var profilePicutre = context.Pictures.SingleOrDefault(p => p.Path == userDto.ProfilePicture);

                if (!IsValid(userDto) || profilePicutre == null)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                sb.AppendLine($"Successfully imported User {userDto.Username}.");
                var user = Mapper.Map<User>(userDto);
                user.ProfilePicture = profilePicutre;
                validUsers.Add(user);
            }

            context.Users.AddRange(validUsers);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportFollowers(InstagraphContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedFollowers = JsonConvert.DeserializeObject<FollowerDto[]>(jsonString);

            var validFollowers = new List<UserFollower>();
            foreach (var followerDto in deserializedFollowers)
            {
                var user = context.Users.SingleOrDefault(u => u.Username == followerDto.User);
                var followerUser = context.Users.SingleOrDefault(u => u.Username == followerDto.Follower);

                if (!IsValid(followerDto) || user == null || followerUser == null)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                bool alreadyFollowed = validFollowers.Any(f => f.User == user && f.Follower == followerUser);
                if (alreadyFollowed)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                sb.AppendLine($"Successfully imported Follower {followerDto.Follower} to User {followerDto.User}.");
                var follower = Mapper.Map<UserFollower>(followerDto);
                follower.User = user;
                follower.Follower = followerUser;
                validFollowers.Add(follower);
            }
            context.UsersFollowers.AddRange(validFollowers);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportPosts(InstagraphContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(PostDto[]), new XmlRootAttribute("posts"));
            var deserializedPosts = (PostDto[])serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

            var validPosts = new List<Post>();
            foreach (var postDto in deserializedPosts)
            {
                var user = context.Users.SingleOrDefault(u => u.Username == postDto.User);
                var picture = context.Pictures.SingleOrDefault(p => p.Path == postDto.Picture);

                if (!IsValid(postDto) || (user == null || picture == null))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                sb.AppendLine($"Successfully imported Post {postDto.Caption}.");
                var post = Mapper.Map<Post>(postDto);
                post.User = user;
                post.Picture = picture;

                validPosts.Add(post);
            }
            context.Posts.AddRange(validPosts);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportComments(InstagraphContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(CommentDto[]), new XmlRootAttribute("comments"));
            var deserializedComments = (CommentDto[])serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

            var validComments = new List<Comment>();
            foreach (var commentDto in deserializedComments)
            {
                var user = context.Users.SingleOrDefault(u => u.Username == commentDto.User);

                if (!IsValid(commentDto) || user == null)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }
                bool isParsed = int.TryParse(commentDto.PostId?.Id, out var parsedId);

                if (!isParsed)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                var postId = context.Posts.SingleOrDefault(p => p.Id == parsedId)?.Id;
                if (postId == null)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                sb.AppendLine($"Successfully imported Comment {commentDto.Content}.");
                var comment = Mapper.Map<Comment>(commentDto);
                comment.User = user;
                comment.PostId = postId.Value;

                validComments.Add(comment);
            }
            context.Comments.AddRange(validComments);
            context.SaveChanges();

            return sb.ToString();
        }

        private static bool IsValid(object obj)
        {
            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(obj, context, results, true);

            return isValid;
        }
    }
}
