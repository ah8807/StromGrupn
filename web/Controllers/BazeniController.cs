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
    public class BazeniController : Controller
    {
        private readonly SpljocContext _context;

        public BazeniController(SpljocContext context)
        {
            _context = context;
        }

        // GET: Bazeni
        public async Task<IActionResult> Index()
        {
            return View(await _context.Bazeni.ToListAsync());
        }

        // GET: Bazeni/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bazen = await _context.Bazeni
                .FirstOrDefaultAsync(m => m.BazenID == id);
            if (bazen == null)
            {
                return NotFound();
            }

            return View(bazen);
        }

        // GET: Bazeni/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bazeni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BazenID,Ime,Naslov")] Bazen bazen)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bazen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bazen);
        }

        // GET: Bazeni/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bazen = await _context.Bazeni.FindAsync(id);
            if (bazen == null)
            {
                return NotFound();
            }
            return View(bazen);
        }

        // POST: Bazeni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BazenID,Ime,Naslov")] Bazen bazen)
        {
            if (id != bazen.BazenID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bazen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BazenExists(bazen.BazenID))
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
            return View(bazen);
        }

        // GET: Bazeni/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bazen = await _context.Bazeni
                .FirstOrDefaultAsync(m => m.BazenID == id);
            if (bazen == null)
            {
                return NotFound();
            }

            return View(bazen);
        }

        // POST: Bazeni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bazen = await _context.Bazeni.FindAsync(id);
            _context.Bazeni.Remove(bazen);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BazenExists(int id)
        {
            return _context.Bazeni.Any(e => e.BazenID == id);
        }
    }
}
