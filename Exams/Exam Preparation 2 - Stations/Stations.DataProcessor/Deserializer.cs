namespace Stations.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using AutoMapper;
    using Data;
    using Dto.Import;
    using Models;
    using Models.Enums;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public static class Deserializer
    {
        private const string FailureMessage = "Invalid data format.";
        private const string SuccessMessage = "Record {0} successfully imported.";

        public static string ImportStations(StationsDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedStations = JsonConvert.DeserializeObject<StationDto[]>(jsonString);

            var validStations = new List<Station>();
            foreach (var stationDto in deserializedStations)
            {
                if (!IsValid(stationDto) || validStations.Any(s => s.Name == stationDto.Name))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var station = Mapper.Map<Station>(stationDto);

                if (stationDto.Town == null)
                {
                    station.Town = stationDto.Name;
                }

                validStations.Add(station);
                sb.AppendLine(string.Format(SuccessMessage, stationDto.Name));
            }

            context.Stations.AddRange(validStations);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportClasses(StationsDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedClasses = JsonConvert.DeserializeObject<SeatingClassDto[]>(jsonString);

            var validClasses = new List<SeatingClass>();
            foreach (var classDto in deserializedClasses)
            {
                if (!IsValid(classDto) ||
                    validClasses.Any(sc => sc.Name == classDto.Name ||
                                           sc.Abbreviation == classDto.Abbreviation))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var seatingClass = Mapper.Map<SeatingClass>(classDto);

                validClasses.Add(seatingClass);
                sb.AppendLine(string.Format(SuccessMessage, classDto.Name));
            }

            context.SeatingClasses.AddRange(validClasses);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportTrains(StationsDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedTrains = JsonConvert.DeserializeObject<TrainDto[]>(jsonString, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var validTrains = new List<Train>();
            foreach (var trainDto in deserializedTrains)
            {
                if (!IsValid(trainDto) || validTrains.Any(t => t.TrainNumber == trainDto.TrainNumber))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var train = new Train
                {
                    TrainNumber = trainDto.TrainNumber,
                    TrainSeats = new List<TrainSeat>(),
                };

                if (trainDto.Type != null)
                {
                    var type = Enum.Parse<TrainType>(trainDto.Type);

                    train.Type = type;
                }
                else
                {
                    train.Type = TrainType.HighSpeed;
                }

                bool isSeatingClassValid = true;
                foreach (var seatingClass in trainDto.Seats)
                {
                    var sc = context
                        .SeatingClasses
                        .SingleOrDefault(
                            s => s.Name == seatingClass.Name && s.Abbreviation == seatingClass.Abbreviation);

                    if (sc == null || !seatingClass.Quantity.HasValue || seatingClass.Quantity < 0)
                    {
                        isSeatingClassValid = false;
                        break;
                    }

                    train.TrainSeats.Add(new TrainSeat
                    {
                        SeatingClass = sc,
                        Quantity = seatingClass.Quantity.Value
                    });
                }

                if (!isSeatingClassValid)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                validTrains.Add(train);
                sb.AppendLine(string.Format(SuccessMessage, trainDto.TrainNumber));
            }

            context.Trains.AddRange(validTrains);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportTrips(StationsDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedTrips = JsonConvert.DeserializeObject<TripDto[]>(jsonString);

            var validTrips = new List<Trip>();
            foreach (var tripDto in deserializedTrips)
            {
                if (!IsValid(tripDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var departureTime = DateTime.ParseExact(tripDto.DepartureTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                var arrivalTime = DateTime.ParseExact(tripDto.ArrivalTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                var train = context
                    .Trains
                    .SingleOrDefault(t => t.TrainNumber == tripDto.Train);
                var originStation = context
                    .Stations
                    .SingleOrDefault(os => os.Name == tripDto.OriginStation);
                var destinationStation = context
                    .Stations
                    .SingleOrDefault(ds => ds.Name == tripDto.DestinationStation);

                if (departureTime >= arrivalTime || train == null ||
                    originStation == null || destinationStation == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var trip = new Trip
                {
                    Train = train,
                    OriginStation = originStation,
                    DestinationStation = destinationStation,
                    DepartureTime = departureTime,
                    ArrivalTime = arrivalTime,
                };

                if (tripDto.Status != null)
                {
                    var status = Enum.Parse<TripStatus>(tripDto.Status);
                    trip.Status = status;
                }

                if (tripDto.TimeDifference != null)
                {
                    var timeDifference = TimeSpan.ParseExact(tripDto.TimeDifference, @"hh\:mm", CultureInfo.InvariantCulture);
                    trip.TimeDifference = timeDifference;
                }

                validTrips.Add(trip);
                sb.AppendLine($"Trip from {tripDto.OriginStation} to {tripDto.DestinationStation} imported.");
            }

            context.Trips.AddRange(validTrips);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportCards(StationsDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(CardDto[]), new XmlRootAttribute("Cards"));
            var deserializedCards = (CardDto[])serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

            var validCards = new List<CustomerCard>();
            foreach (var cardDto in deserializedCards)
            {
                if (!IsValid(cardDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var card = Mapper.Map<CustomerCard>(cardDto);

                if (cardDto.CardType != null)
                {
                    var type = Enum.Parse<CardType>(cardDto.CardType);
                    card.Type = type;
                }

                validCards.Add(card);
                sb.AppendLine(string.Format(SuccessMessage, cardDto.Name));
            }

            context.Cards.AddRange(validCards);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportTickets(StationsDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(TicketDto[]), new XmlRootAttribute("Tickets"));
            var deserializedTickets = (TicketDto[])serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

            var validTickets = new List<Ticket>();
            foreach (var ticketDto in deserializedTickets)
            {
                if (!IsValid(ticketDto) || !IsValid(ticketDto.Trip) || (ticketDto.Card != null && !IsValid(ticketDto.Card)))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var departureTime = DateTime.ParseExact(ticketDto.Trip.DepartureTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                var trip = context
                    .Trips
                    .SingleOrDefault(t =>
                        t.OriginStation.Name == ticketDto.Trip.OriginStation &&
                        t.DestinationStation.Name == ticketDto.Trip.DestinationStation &&
                        t.DepartureTime == departureTime);

                var seatClassAbbreviation = ticketDto.Seat.Substring(0, 2);
                var seatingClass = context
                    .SeatingClasses
                    .SingleOrDefault(sc => sc.Abbreviation == seatClassAbbreviation);

                var trainSeatingClass = trip?
                    .Train
                    .TrainSeats
                    .SingleOrDefault(ts => ts.SeatingClass == seatingClass);

                if (trip == null || seatingClass == null || trainSeatingClass == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var seatNumber = int.Parse(ticketDto.Seat.Substring(2));
                var trainSeatsQuantity = trainSeatingClass.Quantity;

                if (seatNumber <= 0 || seatNumber > trainSeatsQuantity)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var ticket = new Ticket
                {
                    Price = decimal.Parse(ticketDto.Price),
                    Trip = trip,
                    SeatingPlace = ticketDto.Seat,
                };

                if (ticketDto.Card != null)
                {
                    var card = context
                        .Cards
                        .SingleOrDefault(c => c.Name == ticketDto.Card.Name);
                    if (card == null)
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }

                    ticket.CustomerCard = card;
                }

                validTickets.Add(ticket);
                sb.AppendLine($"Ticket from {ticketDto.Trip.OriginStation} to {ticketDto.Trip.DestinationStation} departing at {ticketDto.Trip.DepartureTime} imported.");
            }

            context.Tickets.AddRange(validTickets);
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