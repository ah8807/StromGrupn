using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using web.Filters;

namespace web.Controllers_Api
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class PlavalciApiController : ControllerBase
    {
        private readonly StromGrupnContext _context;

        public PlavalciApiController(StromGrupnContext context)
        {
            _context = context;
        }

        // GET: api/PlavalciApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Plavalec>>> GetPlavalci()
        {
            return await _context.Plavalci.ToListAsync();
        }

        // GET: api/PlavalciApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Plavalec>> GetPlavalec(int id)
        {
            var plavalec = await _context.Plavalci.FindAsync(id);

            if (plavalec == null)
            {
                return NotFound();
            }

            return plavalec;
        }

        // PUT: api/PlavalciApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlavalec(int id, Plavalec plavalec)
        {
            if (id != plavalec.PlavalecID)
            {
                return BadRequest();
            }

            _context.Entry(plavalec).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlavalecExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PlavalciApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Plavalec>> PostPlavalec(Plavalec plavalec)
        {
            _context.Plavalci.Add(plavalec);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlavalec", new { id = plavalec.PlavalecID }, plavalec);
        }

        // DELETE: api/PlavalciApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlavalec(int id)
        {
            var plavalec = await _context.Plavalci.FindAsync(id);
            if (plavalec == null)
            {
                return NotFound();
            }

            _context.Plavalci.Remove(plavalec);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlavalecExists(int id)
        {
            return _context.Plavalci.Any(e => e.PlavalecID == id);
        }
    }
}
