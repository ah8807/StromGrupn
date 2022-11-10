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
    [Authorize(Roles ="Administrator,Trener")]
    public class SkupineController : Controller
    {
        private readonly SpljocContext _context;

        public SkupineController(SpljocContext context)
        {
            _context = context;
        }

        // GET: Skupine
        public async Task<IActionResult> Index()
        {
            var spljocContext = _context.Skupine.Include(s => s.Bazen).Include(s => s.Ucitelj);
            return View(await spljocContext.ToListAsync());
        }

        // GET: Skupine/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skupina = await _context.Skupine
                .Include(s => s.Bazen)
                .Include(s => s.Ucitelj)
                .FirstOrDefaultAsync(m => m.SkupinaID == id);
            if (skupina == null)
            {
                return NotFound();
            }

            return View(skupina);
        }

        // GET: Skupine/Create
        public IActionResult Create()
        {
            ViewData["BazenID"] = new SelectList(_context.Bazeni, "BazenID", "BazenID");
            ViewData["UciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID");
            return View();
        }

        // POST: Skupine/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkupinaID,UciteljID,BazenID,ProgaID,Ura")] Skupina skupina)
        {
            if (ModelState.IsValid)
            {
                _context.Add(skupina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BazenID"] = new SelectList(_context.Bazeni, "BazenID", "BazenID", skupina.BazenID);
            ViewData["UciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID", skupina.UciteljID);
            return View(skupina);
        }

        // GET: Skupine/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skupina = await _context.Skupine.FindAsync(id);
            if (skupina == null)
            {
                return NotFound();
            }
            ViewData["BazenID"] = new SelectList(_context.Bazeni, "BazenID", "BazenID", skupina.BazenID);
            ViewData["UciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID", skupina.UciteljID);
            return View(skupina);
        }

        // POST: Skupine/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SkupinaID,UciteljID,BazenID,ProgaID,Ura")] Skupina skupina)
        {
            if (id != skupina.SkupinaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skupina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkupinaExists(skupina.SkupinaID))
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
            ViewData["BazenID"] = new SelectList(_context.Bazeni, "BazenID", "BazenID", skupina.BazenID);
            ViewData["UciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID", skupina.UciteljID);
            return View(skupina);
        }

        // GET: Skupine/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skupina = await _context.Skupine
                .Include(s => s.Bazen)
                .Include(s => s.Ucitelj)
                .FirstOrDefaultAsync(m => m.SkupinaID == id);
            if (skupina == null)
            {
                return NotFound();
            }

            return View(skupina);
        }

        // POST: Skupine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skupina = await _context.Skupine.FindAsync(id);
            _context.Skupine.Remove(skupina);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SkupinaExists(int id)
        {
            return _context.Skupine.Any(e => e.SkupinaID == id);
        }
    }
}
