using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Controllers
{
    public class UciteljiiController : Controller
    {
        private readonly SpljocContext _context;

        public UciteljiiController(SpljocContext context)
        {
            _context = context;
        }

        // GET: Uciteljii
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ucitelji.ToListAsync());
        }

        // GET: Uciteljii/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ucitelj = await _context.Ucitelji
                .FirstOrDefaultAsync(m => m.UciteljID == id);
            if (ucitelj == null)
            {
                return NotFound();
            }

            return View(ucitelj);
        }

        // GET: Uciteljii/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Uciteljii/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UciteljID,Ime,Priimek,DatumRojstva,UrnaPostavka")] Ucitelj ucitelj)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ucitelj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ucitelj);
        }

        // GET: Uciteljii/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ucitelj = await _context.Ucitelji.FindAsync(id);
            if (ucitelj == null)
            {
                return NotFound();
            }
            return View(ucitelj);
        }

        // POST: Uciteljii/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UciteljID,Ime,Priimek,DatumRojstva,UrnaPostavka")] Ucitelj ucitelj)
        {
            if (id != ucitelj.UciteljID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ucitelj);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UciteljExists(ucitelj.UciteljID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ucitelj);
        }

        // GET: Uciteljii/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ucitelj = await _context.Ucitelji
                .FirstOrDefaultAsync(m => m.UciteljID == id);
            if (ucitelj == null)
            {
                return NotFound();
            }

            return View(ucitelj);
        }

        // POST: Uciteljii/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ucitelj = await _context.Ucitelji.FindAsync(id);
            _context.Ucitelji.Remove(ucitelj);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UciteljExists(int id)
        {
            return _context.Ucitelji.Any(e => e.UciteljID == id);
        }
    }
}
