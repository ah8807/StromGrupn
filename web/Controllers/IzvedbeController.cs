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
    public class IzvedbeController : Controller
    {
        private readonly SpljocContext _context;

        public IzvedbeController(SpljocContext context)
        {
            _context = context;
        }

        // GET: Izvedbe
        public async Task<IActionResult> Index()
        {
            var spljocContext = _context.Izvedbe.Include(i => i.NadomestniUcitelj).Include(i => i.Skupina);
            return View(await spljocContext.ToListAsync());
        }

        // GET: Izvedbe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var izvedba = await _context.Izvedbe
                .Include(i => i.NadomestniUcitelj)
                .Include(i => i.Skupina)
                .FirstOrDefaultAsync(m => m.IzvedbaID == id);
            if (izvedba == null)
            {
                return NotFound();
            }

            return View(izvedba);
        }

        // GET: Izvedbe/Create
        public IActionResult Create()
        {
            ViewData["NadomestniUciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID");
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID");
            return View();
        }

        // POST: Izvedbe/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IzvedbaID,DatumUra,SkupinaID,NadomestniUciteljID")] Izvedba izvedba)
        {
            if (ModelState.IsValid)
            {
                _context.Add(izvedba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NadomestniUciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID", izvedba.NadomestniUciteljID);
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID", izvedba.SkupinaID);
            return View(izvedba);
        }

        // GET: Izvedbe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var izvedba = await _context.Izvedbe.FindAsync(id);
            if (izvedba == null)
            {
                return NotFound();
            }
            ViewData["NadomestniUciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID", izvedba.NadomestniUciteljID);
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID", izvedba.SkupinaID);
            return View(izvedba);
        }

        // POST: Izvedbe/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IzvedbaID,DatumUra,SkupinaID,NadomestniUciteljID")] Izvedba izvedba)
        {
            if (id != izvedba.IzvedbaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(izvedba);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IzvedbaExists(izvedba.IzvedbaID))
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
            ViewData["NadomestniUciteljID"] = new SelectList(_context.Ucitelji, "UciteljID", "UciteljID", izvedba.NadomestniUciteljID);
            ViewData["SkupinaID"] = new SelectList(_context.Skupine, "SkupinaID", "SkupinaID", izvedba.SkupinaID);
            return View(izvedba);
        }

        // GET: Izvedbe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var izvedba = await _context.Izvedbe
                .Include(i => i.NadomestniUcitelj)
                .Include(i => i.Skupina)
                .FirstOrDefaultAsync(m => m.IzvedbaID == id);
            if (izvedba == null)
            {
                return NotFound();
            }

            return View(izvedba);
        }

        // POST: Izvedbe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var izvedba = await _context.Izvedbe.FindAsync(id);
            _context.Izvedbe.Remove(izvedba);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IzvedbaExists(int id)
        {
            return _context.Izvedbe.Any(e => e.IzvedbaID == id);
        }
    }
}
