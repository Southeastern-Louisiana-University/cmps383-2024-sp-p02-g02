using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Features.Hotels;
using Microsoft.AspNetCore.Authorization;

namespace Selu383.SP24.Api.HotelsController;

[Route("api/hotels")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly DbSet<Hotel> hotels;
    private readonly DataContext dataContext;

    public HotelsController(DataContext dataContext)
    {
        this.dataContext = dataContext;
        hotels = dataContext.Set<Hotel>()   ;
    }

    [HttpGet]
    public IQueryable<HotelDto> GetAllHotels()
    {
        return GetHotelDtos(hotels);
    }

    [HttpGet]
    [Route("{id}")]
    public ActionResult<HotelDto> GetHotelById(int id)
    {
        var result = GetHotelDtos(hotels.Where(x => x.Id == id)).FirstOrDefault();
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<HotelDto> CreateHotel([FromBody] HotelDto dto)
    {
        if (IsInvalidForCreateHotel(dto))
        {
            return BadRequest("Invalid input for creating a hotel.");
        }

        var hotel = new Hotel
        {
            Name = dto.Name,
            Address = dto.Address,
            ManagerId = dto.ManagerId,
        };

        hotels.Add(hotel);
        dataContext.SaveChanges();

        dto.Id = hotel.Id;

        // Return the created DTO with its location
        var locationUri = new Uri($"/api/hotels/{dto.Id}", UriKind.Relative);
        return Created(locationUri, dto);
    }

    private bool IsInvalidForCreateHotel(HotelDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name) || dto.Name.Length > 120 || string.IsNullOrEmpty(dto.Address))
        {
            return true;
        }

        if (dto.ManagerId.HasValue && dto.ManagerId <= 0)
        {
            return true;
        }

        return false;
    }



    [HttpPut("{id}")]
    public ActionResult<HotelDto> UpdateHotel(int id, [FromBody] HotelDto dto)
    {
        var hotel = hotels.FirstOrDefault(x => x.Id == id);

        if (hotel == null)
        {
            return NotFound();
        }


        if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && hotel.ManagerId != dto.ManagerId)
        {
            return BadRequest("You do not have permission to update ManagerId.");
        }

        if (IsInvalidUpdateForUpdate(dto))
        {
            return BadRequest("Invalid input for updating a hotel.");
        }

        hotel.Name = dto.Name;
        hotel.Address = dto.Address;

        // Only admins can update the ManagerId
        if (User.IsInRole("Admin"))
        {
            hotel.ManagerId = dto.ManagerId;
        }

        dataContext.SaveChanges();

        dto.Id = hotel.Id;

        return Ok(dto);
    }

    private bool IsInvalidUpdateForUpdate(HotelDto dto) // Renamed the method here
    {
        if (string.IsNullOrEmpty(dto.Name) || dto.Name.Length > 120 || string.IsNullOrEmpty(dto.Address))
        {
            return true;
        }

        if (dto.ManagerId.HasValue && dto.ManagerId <= 0)
        {
            return true;
        }

        return false;
    }


    private bool IsInvalidUpdate(HotelDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name) || dto.Name.Length > 120 || string.IsNullOrEmpty(dto.Address))
        {
            return true;
        }

        if (dto.ManagerId.HasValue && dto.ManagerId <= 0)
        {
            return true;
        }

        return false;
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteHotel(int id)
    {
        var hotel = hotels.FirstOrDefault(x => x.Id == id);

        if (hotel == null)
        {
            return NotFound();
        }

        // Only admins can delete hotels
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        hotels.Remove(hotel);
        dataContext.SaveChanges();

        return Ok();
    }


    private static bool IsInvalid(HotelDto dto)
    {
        return string.IsNullOrWhiteSpace(dto.Name) ||
               dto.Name.Length > 120 ||
               string.IsNullOrWhiteSpace(dto.Address);
    }

    private static IQueryable<HotelDto> GetHotelDtos(IQueryable<Hotel> hotels)
    {
        return hotels
            .Select(x => new HotelDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
            });
    }
}