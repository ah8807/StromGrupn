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
    public class UciteljiApiController : ControllerBase
    {
        private readonly StromGrupnContext _context;

        public UciteljiApiController(StromGrupnContext context)
        {
            _context = context;
        }

        // GET: api/UciteljiApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ucitelj>>> GetUcitelji()
        {
            return await _context.Ucitelji.ToListAsync();
        }

        // GET: api/UciteljiApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ucitelj>> GetUcitelj(int id)
        {
            var ucitelj = await _context.Ucitelji.FindAsync(id);

            if (ucitelj == null)
            {
                return NotFound();
            }

            return ucitelj;
        }

        // PUT: api/UciteljiApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUcitelj(int id, Ucitelj ucitelj)
        {
            if (id != ucitelj.UciteljID)
            {
                return BadRequest();
            }

            _context.Entry(ucitelj).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UciteljExists(id))
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

        // POST: api/UciteljiApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Ucitelj>> PostUcitelj(Ucitelj ucitelj)
        {
            _context.Ucitelji.Add(ucitelj);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUcitelj", new { id = ucitelj.UciteljID }, ucitelj);
        }

        // DELETE: api/UciteljiApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUcitelj(int id)
        {
            var ucitelj = await _context.Ucitelji.FindAsync(id);
            if (ucitelj == null)
            {
                return NotFound();
            }

            _context.Ucitelji.Remove(ucitelj);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UciteljExists(int id)
        {
            return _context.Ucitelji.Any(e => e.UciteljID == id);
        }
    }
}
