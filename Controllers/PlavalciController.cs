using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace web.Controllers
{
    [Authorize(Roles = "Administrator,Trener")]
    public class PlavalciController : Controller
    {
        private readonly StromGrupnContext _context;

        public PlavalciController(StromGrupnContext context)
        {
            _context = context;
        }

        // GET: Plavalci
        public async Task<IActionResult> Index()
        {
            var StromGrupnContext = _context.Plavalci.Include(p => p.Skupina);
            return View(await StromGrupnContext.ToListAsync());
        }

        // GET: Plavalci/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plavalec = await _context.Plavalci
                .Include(p => p.Skupina)
                .FirstOrDefaultAsync(m => m.PlavalecID == id);
            if (plavalec == null)
            {
                return NotFound();
            }

            return View(plavalec);
        }

        // GET: Plavalci/Create
        public IActionResult Create()
        {
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID");
            return View();
        }

        // POST: Plavalci/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlavalecID,Ime,Priimek,DatumRojstva,SkupinaID")] Plavalec plavalec)
        {
            if (ModelState.IsValid)
            {
                _context.Add(plavalec);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID", plavalec.SkupinaID);
            return View(plavalec);
        }

        // GET: Plavalci/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plavalec = await _context.Plavalci.FindAsync(id);
            if (plavalec == null)
            {
                return NotFound();
            }
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID", plavalec.SkupinaID);
            return View(plavalec);
        }

        // POST: Plavalci/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlavalecID,Ime,Priimek,DatumRojstva,SkupinaID")] Plavalec plavalec)
        {
            if (id != plavalec.PlavalecID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plavalec);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlavalecExists(plavalec.PlavalecID))
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
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID", plavalec.SkupinaID);
            return View(plavalec);
        }

        // GET: Plavalci/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plavalec = await _context.Plavalci
                .Include(p => p.Skupina)
                .FirstOrDefaultAsync(m => m.PlavalecID == id);
            if (plavalec == null)
            {
                return NotFound();
            }

            return View(plavalec);
        }

        // POST: Plavalci/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plavalec = await _context.Plavalci.FindAsync(id);
            _context.Plavalci.Remove(plavalec);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlavalecExists(int id)
        {
            return _context.Plavalci.Any(e => e.PlavalecID == id);
        }
    }
}
