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
    public class SkupineApiController : ControllerBase
    {
        private readonly SpljocContext _context;

        public SkupineApiController(SpljocContext context)
        {
            _context = context;
        }

        // GET: api/SkupineApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Skupina>>> GetSkupine()
        {
            return await _context.Skupine.ToListAsync();
        }

        // GET: api/SkupineApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Skupina>> GetSkupina(int id)
        {
            var skupina = await _context.Skupine.FindAsync(id);

            if (skupina == null)
            {
                return NotFound();
            }

            return skupina;
        }

        // PUT: api/SkupineApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSkupina(int id, Skupina skupina)
        {
            if (id != skupina.SkupinaID)
            {
                return BadRequest();
            }

            _context.Entry(skupina).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkupinaExists(id))
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

        // POST: api/SkupineApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Skupina>> PostSkupina(Skupina skupina)
        {
            _context.Skupine.Add(skupina);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSkupina", new { id = skupina.SkupinaID }, skupina);
        }

        // DELETE: api/SkupineApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkupina(int id)
        {
            var skupina = await _context.Skupine.FindAsync(id);
            if (skupina == null)
            {
                return NotFound();
            }

            _context.Skupine.Remove(skupina);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SkupinaExists(int id)
        {
            return _context.Skupine.Any(e => e.SkupinaID == id);
        }
    }
}
