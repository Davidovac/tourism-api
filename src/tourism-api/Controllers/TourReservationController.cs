using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/reservations")]
[ApiController]
public class TourReservationController : ControllerBase
{
    private readonly TourReservationRepository _reservationRepo;
    private readonly UserRepository _userRepo;
    private readonly TourRepository _tourRepo;

    public TourReservationController(IConfiguration configuration)
    {
        _reservationRepo = new TourReservationRepository(configuration);
        _userRepo = new UserRepository(configuration);
        _tourRepo = new TourRepository(configuration);
    }

    [HttpGet]
    public ActionResult GetByUser([FromQuery] int userId)
    {
        try
        {
            List<TourReservation> reservations = _reservationRepo.GetByUser(userId);
            if (reservations == null || reservations.Count == 0)
            {
                return NotFound("No reservations found for this guide.");
            }
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching reservations: " + ex.Message);
        }
    }

    [HttpPost]
    public ActionResult Create([FromBody] TourReservation newReservation)
    {
        try
        {
            TourReservation? retrievedReservation = null;
            User user = _userRepo.GetById(newReservation.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {newReservation.UserId} not found.");
            }
            Tour tour = _tourRepo.GetById(newReservation.TourId);
            if (tour == null)
            {
                return NotFound($"Tour with ID {newReservation.TourId} not found.");
            }
            newReservation.Tour = tour;

            int? capacityLeft = _reservationRepo.GetCapacityLeft(newReservation.TourId);
            if (capacityLeft == null)
            {
                retrievedReservation = _reservationRepo.Create(newReservation);
            }
            else if (capacityLeft <= 0)
            {
                return Conflict("No capacity left for this tour.");
            }
            else
            {
                if (newReservation.GuestsCount > capacityLeft)
                {
                    return Conflict($"Only {capacityLeft} spots left for this tour.");
                }
                if (_reservationRepo.DidReserveAlready(newReservation.UserId, newReservation.TourId))
                {
                    retrievedReservation = _reservationRepo.Update(newReservation);
                }
                else
                {
                    retrievedReservation = _reservationRepo.Create(newReservation);
                }
            }

                return Ok(retrievedReservation);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while creating the tour.");
        }
    }


    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        try
        {
            bool isDeleted = _reservationRepo.Delete(id);
            if (isDeleted)
            {
                return NoContent();
            }
            return NotFound($"Reservation with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while deleting the reservation.");
        }
    }
}
