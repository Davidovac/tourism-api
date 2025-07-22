using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/tours")]
[ApiController]
public class TourController : ControllerBase
{
    private readonly TourRepository _tourRepo;
    private readonly UserRepository _userRepo;
    private readonly TourKeyPointRepository _tourKPRepo;

    public TourController(IConfiguration configuration)
    {
        _tourRepo = new TourRepository(configuration);
        _userRepo = new UserRepository(configuration);
        _tourKPRepo = new TourKeyPointRepository(configuration);
    }

    [HttpGet]
    public ActionResult GetPaged([FromQuery] int guideId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Name", [FromQuery] string orderDirection = "ASC")
    {
        if (guideId > 0)
        {
            return Ok(_tourRepo.GetByGuide(guideId));
        }

        // Validacija za orderBy i orderDirection
        List<string> validOrderByColumns = new List<string> { "Name", "Description", "DateTime", "MaxGuests" }; // Lista dozvoljenih kolona za sortiranje
        if (!validOrderByColumns.Contains(orderBy))
        {
            orderBy = "Name"; // Default vrednost
        }

        List<string> validOrderDirections = new List<string> { "ASC", "DESC" }; // Lista dozvoljenih smerova
        if (!validOrderDirections.Contains(orderDirection))
        {
            orderDirection = "ASC"; // Default vrednost
        }

        try
        {
            List<Tour> tours = _tourRepo.GetPaged(page, pageSize, orderBy, orderDirection);
            int totalCount = _tourRepo.CountAll();
            Object result = new
            {
                Data = tours,
                TotalCount = totalCount
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching tours.");
        }
    }

    [HttpGet("{id}")]
    public ActionResult<Tour> GetById(int id)
    {
        try
        {
            Tour tour = _tourRepo.GetById(id);
            if (tour == null)
            {
                return NotFound($"Tour with ID {id} not found.");
            }
            return Ok(tour);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching the tour.");
        }
    }

    [HttpPost]
    public ActionResult<Tour> Create([FromBody] Tour newTour)
    {
        if (!newTour.IsValid())
        {
            return BadRequest("Invalid tour data.");
        }
        try
        {
            User user = _userRepo.GetById(newTour.GuideId);
            if (user == null)
            {
                return NotFound($"User with ID {newTour.GuideId} not found.");
            }

            Tour createdTour = _tourRepo.Create(newTour);
            if (newTour.KeyPoints != null && newTour.KeyPoints.Count != 0)
            {
                createdTour.KeyPoints = newTour.KeyPoints;
                _tourKPRepo.AddKeyPointToTour(createdTour);
            }
            
            return Ok(createdTour);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while creating the tour.");
        }
    }

    [HttpPut("{id}")]
    public ActionResult<Tour> Update(int id, [FromBody] Tour tour)
    {
        if (!tour.IsValid())
        {
            return BadRequest("Invalid tour data.");
        }

        try
        {
            tour.Id = id;
            Tour updatedTour = _tourRepo.Update(tour);
            if (updatedTour == null)
            {
                return NotFound();
            }
            if (tour.KeyPoints != null || tour.KeyPoints.Count != 0)
            {
                updatedTour.KeyPoints = tour.KeyPoints;
                _tourKPRepo.ReplaceKeyPointsInTour(updatedTour);
            }

            return Ok(updatedTour);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while updating the tour.");
        }
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        try
        {
            bool isDeleted = _tourRepo.Delete(id);
            if (isDeleted)
            {
                return NoContent();
            }
            return NotFound($"Tour with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while deleting the tour.");
        }
    }

    [HttpPost("{tourId}/ratings")]
    public ActionResult AddRating(int tourId, [FromBody] TourRating rating)
    {
        try
        {
            if (rating.Rating < 1 || rating.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }
                
            Tour tour = _tourRepo.GetById(tourId);
            if (tour == null)
            {
                return NotFound($"Tour with ID {tourId} not found.");
            }
                
            User user = _userRepo.GetById(rating.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {rating.UserId} not found.");
            }
                
            rating.TourId = tourId;
            rating.CreatedAt = DateTime.UtcNow;

            bool ind = _tourRepo.AddRating(rating);

            if (ind)
            {
                return Conflict("User already rated this tour.");
            }

            return Ok("Rating added.");
        }
        catch (Exception)
        {
            return Problem("An error occurred while adding the rating.");
        }
    }

    [HttpGet("stats")]
    public ActionResult GetTourStats([FromQuery] DateTime? from, DateTime? to, int guideId = 0)
    {
        try
        {
            User? guide = _userRepo.GetById(guideId);
            if (guide == null)
            {
                return NotFound($"Guide with ID {guideId} not found.");
            }
            ToursStatsData stats = _tourRepo.GetTourStatsByGuide(guideId, from, to);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching tour statistics.");
        }
    }
}
