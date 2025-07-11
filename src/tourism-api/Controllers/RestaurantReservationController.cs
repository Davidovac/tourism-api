using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

    [Route("api/restaurant/reservations")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly RestaurantReservationRepository _reservationRepo;
        private readonly RestaurantRepository _restaurantRepo;
        private readonly UserRepository _userRepo;

        public ReservationController(IConfiguration configuration)
        {
            _reservationRepo = new RestaurantReservationRepository(configuration);
            _restaurantRepo = new RestaurantRepository(configuration);
            _userRepo = new UserRepository(configuration);
        }

        [HttpPost]
        public ActionResult<RestaurantReservation> CreateReservation([FromBody] ReservationRequest request)
        {
            try
            {
                var restaurant = _restaurantRepo.GetById(request.RestaurantId);
                if (restaurant == null)
                {
                    return NotFound($"Restoran sa ID {request.RestaurantId} ne postoji.");
                }

                var user = _userRepo.GetById(request.UserId);
                if (user == null)
                {
                    return NotFound($"Korisnik sa ID {request.UserId} ne postoji.");
                }

                if (request.Date.Date < DateTime.Today)
                {
                    return BadRequest("Datum rezervacije mora biti u buducnosti");
                }

                var (isAvailable, maxAvailable) = _reservationRepo.CheckReservationAvailability(
                    request.RestaurantId,
                    request.Date,
                    request.MealTime,
                    request.NumberOfPeople);

                if (!isAvailable)
                {
                    return BadRequest(new
                    {
                        Message = "Nema dovoljno dostupnih mesta za odabrani restoran i termin.",
                        MaxAvailable = maxAvailable
                    });
                }

                var reservation = new RestaurantReservation
                {
                    RestaurantId = request.RestaurantId,
                    UserId = request.UserId,
                    Date = request.Date,
                    MealTime = request.MealTime,
                    NumberOfPeople = request.NumberOfPeople
                };

                RestaurantReservation createdReservation = _reservationRepo.Create(reservation);
                return Ok(createdReservation);
            }
            catch (Exception ex)
            {
                return Problem("Greska pri kreiranju rezervacije.");
            }
        }

        [HttpDelete("{id}")]
        public ActionResult CancelReservation(int id)
        {
            try
            {
                var reservation = _reservationRepo.GetById(id);
                if (reservation == null)
                {
                    return NotFound($"Rezervacija sa ID {id} ne postoji.");
                }

                DateTime now = DateTime.Now;
                DateTime reservationTime = GetMealTime(reservation.Date, reservation.MealTime);
                TimeSpan timeUntilReservation = reservationTime - now;

                if (reservation.MealTime == "dorucak")
                {
                    if (timeUntilReservation.TotalHours < 12)
                    {
                        return BadRequest("Rezervacije za dorucak mogu da se otkazu najkasnije 12 sati pre pocetka termina.");
                    }
                }
                else
                {
                    if (timeUntilReservation.TotalHours < 4)
                    {
                        return BadRequest("Rezervacije za rucak/veceru mogu se otkazati najkasnije 4 sata pre pocetka termina");
                    }
                }

                bool isDeleted = _reservationRepo.Delete(id);
                if (isDeleted)
                {
                    return NoContent();
                }
                return Problem("Neuspesno otkazivanje rezervacije.");
            }
            catch (Exception ex)
            {
                return Problem("Erorr pri otkazivanju rezervacije.");
            }
        }

        [HttpGet("{restaurantId}")]
        public ActionResult<List<RestaurantReservation>> GetRestaurantReservations(int restaurantId, [FromQuery] DateTime? date = null)
        {
            try
            {
                List<RestaurantReservation> reservations = _reservationRepo.GetByRestaurant(restaurantId, date);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return Problem("Error pri prikupljanu podataka rezervacije.");
            }
        }

        [HttpGet("user/{userId}")]
        public ActionResult<List<RestaurantReservation>> GetUserReservations(int userId)
        {
            try
            {
                List<RestaurantReservation> reservations = _reservationRepo.GetByUser(userId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return Problem("Error pri prikupljanu podataka rezervacije korisnika.");
            }
        }

        private DateTime GetMealTime(DateTime date, string mealTime)
        {
            return mealTime switch
            {
                "dorucak" => date.Date.AddHours(8),
                "rucak" => date.Date.AddHours(13),
                "vecera" => date.Date.AddHours(18),
                _ => throw new ArgumentException("Invalid meal time")
            };
        }
    }

    public class ReservationRequest
    {
        public int RestaurantId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string MealTime { get; set; }
        public int NumberOfPeople { get; set; }
    }