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
    public class BazeniApiController : ControllerBase
    {
        private readonly SpljocContext _context;

        public BazeniApiController(SpljocContext context)
        {
            _context = context;
        }

        // GET: api/BazeniApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bazen>>> GetBazeni()
        {
            return await _context.Bazeni.ToListAsync();
        }

        // GET: api/BazeniApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bazen>> GetBazen(int id)
        {
            var bazen = await _context.Bazeni.FindAsync(id);

            if (bazen == null)
            {
                return NotFound();
            }

            return bazen;
        }

        // PUT: api/BazeniApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBazen(int id, Bazen bazen)
        {
            if (id != bazen.BazenID)
            {
                return BadRequest();
            }

            _context.Entry(bazen).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BazenExists(id))
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

        // POST: api/BazeniApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Bazen>> PostBazen(Bazen bazen)
        {
            _context.Bazeni.Add(bazen);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBazen", new { id = bazen.BazenID }, bazen);
        }

        // DELETE: api/BazeniApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBazen(int id)
        {
            var bazen = await _context.Bazeni.FindAsync(id);
            if (bazen == null)
            {
                return NotFound();
            }

            _context.Bazeni.Remove(bazen);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BazenExists(int id)
        {
            return _context.Bazeni.Any(e => e.BazenID == id);
        }
    }
}
