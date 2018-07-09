using Instagraph.Data;

namespace Instagraph.DataProcessor
{
    using AutoMapper.QueryableExtensions;
    using Dtos.Export;
    using Newtonsoft.Json;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportUncommentedPosts(InstagraphContext context)
        {
            var posts = context
                .Posts
                .Where(p => p.Comments.Count == 0)
                .ProjectTo<UncommentedPostDto>()
                .OrderBy(p => p.Id)
                .ToList();

            var json = JsonConvert.SerializeObject(posts, Formatting.Indented);
            return json;
        }

        public static string ExportPopularUsers(InstagraphContext context)
        {
            var users = context
                .Users
                .Where(u => u.Posts
                    .Any(p => p.Comments
                        .Any(c => u.Followers
                            .Any(f => f.FollowerId == c.UserId))))
                .OrderBy(u => u.Id)
                .ProjectTo<PopularUserDto>()
                .ToList();

            var jsonString = JsonConvert.SerializeObject(users, Formatting.Indented);

            return jsonString;
        }

        public static string ExportCommentsOnPosts(InstagraphContext context)
        {
            var sb = new StringBuilder();

            var users = context
                .Users
                .ProjectTo<UserExportDto>()
                .OrderByDescending(u => u.MostComments)
                .ThenBy(u => u.Username)
                .ToArray();

            var serializer = new XmlSerializer(typeof(UserExportDto[]), new XmlRootAttribute("users"));

            serializer.Serialize(new StringWriter(sb),
             users,
            new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));

            return sb.ToString();

            //var xDoc = new XDocument(new XElement("users"));

            //foreach (var user in users)
            //{
            //    var userElement = new XElement("user");

            //    userElement.Add(new XElement("Username", user.Username));
            //    userElement.Add(new XElement("MostComments", user.MostComments));

            //    xDoc.Root.Add(userElement);
            //}

            //var usersXml = xDoc.ToString();

            //return usersXml;
        }
    }
}
