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
    public class IzvedbeApiController : ControllerBase
    {
        private readonly SpljocContext _context;

        public IzvedbeApiController(SpljocContext context)
        {
            _context = context;
        }

        // GET: api/IzvedbeApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Izvedba>>> GetIzvedbe()
        {
            return await _context.Izvedbe.ToListAsync();
        }

        // GET: api/IzvedbeApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Izvedba>> GetIzvedba(int id)
        {
            var izvedba = await _context.Izvedbe.FindAsync(id);

            if (izvedba == null)
            {
                return NotFound();
            }

            return izvedba;
        }

        // PUT: api/IzvedbeApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIzvedba(int id, Izvedba izvedba)
        {
            if (id != izvedba.IzvedbaID)
            {
                return BadRequest();
            }

            _context.Entry(izvedba).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IzvedbaExists(id))
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

        // POST: api/IzvedbeApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Izvedba>> PostIzvedba(Izvedba izvedba)
        {
            _context.Izvedbe.Add(izvedba);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIzvedba", new { id = izvedba.IzvedbaID }, izvedba);
        }

        // DELETE: api/IzvedbeApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIzvedba(int id)
        {
            var izvedba = await _context.Izvedbe.FindAsync(id);
            if (izvedba == null)
            {
                return NotFound();
            }

            _context.Izvedbe.Remove(izvedba);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IzvedbaExists(int id)
        {
            return _context.Izvedbe.Any(e => e.IzvedbaID == id);
        }
    }
}
