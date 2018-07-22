namespace Stations.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Dto.Export;
    using Models.Enums;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;


    public class Serializer
    {
        public static string ExportDelayedTrains(StationsDbContext context, string dateAsString)
        {
            var date = DateTime.ParseExact(dateAsString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var trains = context
                .Trips
                .Where(t => t.Status == TripStatus.Delayed && t.DepartureTime <= date)
                .GroupBy(t => t.Train.TrainNumber)
                .Select(t => new
                {
                    TrainNumber = t.Key,
                    DelayedTimes = t.Count(),
                    MaxDelayedTime = t.Max(d => d.TimeDifference.Value)
                })
                .OrderByDescending(t => t.DelayedTimes)
                .ThenByDescending(t => t.MaxDelayedTime)
                .ThenBy(t => t.TrainNumber)
                .Select(t => new
                {
                    t.TrainNumber,
                    t.DelayedTimes,
                    MaxDelayedTime = t.MaxDelayedTime.ToString()
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(trains, Formatting.Indented);

            return json;
        }

        public static string ExportCardsTicket(StationsDbContext context, string cardType)
        {
            var sb = new StringBuilder();

            var type = Enum.Parse<CardType>(cardType);

            var cards = context
                .Cards
                .Where(c => c.Type == type && c.BoughtTickets.Any())
                .Select(c => new CardDto
                {
                    Name = c.Name,
                    Type = c.Type.ToString(),
                    Tickets = c.BoughtTickets.Select(t => new TicketDto
                    {
                        OriginStation = t.Trip.OriginStation.Name,
                        DestinationStation = t.Trip.DestinationStation.Name,
                        DepartureTime = t.Trip.DepartureTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    })
                    .ToArray()
                })
                .OrderBy(c => c.Name)
                .ToArray();

            var serializer = new XmlSerializer(typeof(CardDto[]), new XmlRootAttribute("Cards"));
            serializer.Serialize(new StringWriter(sb),
                cards,
                new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty, }));

            return sb.ToString();
        }
    }
}