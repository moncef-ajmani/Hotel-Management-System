using Authentication.Data;
using Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles="Client")]
    public class HotelsController : ControllerBase
    {
        private static AppDbContext _context;

        public HotelsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var hotels = await _context.Hotels.ToListAsync();
            return Ok(hotels);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == id);

            if (hotel == null)
            {
                return BadRequest("Invalid Id");
            }

            return Ok(hotel);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Hotel hotel) 
        {
            await _context.Hotels.AddAsync(hotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get",hotel.Id,hotel);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch(int id,string name)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == id);

            if (hotel == null)
            {
                return BadRequest("Invalid Id");
            }

            hotel.Name = name;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == id);

            if (hotel == null)
            {
                return BadRequest("Invalid Id");
            }

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
