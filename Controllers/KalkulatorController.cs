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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace web.Controllers
{
    public class KalkulatorController : Controller
    {
        ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging((loggingBuilder) => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole()
        )
        .BuildServiceProvider();

        private readonly StromGrupnContext _context;

        public KalkulatorController(StromGrupnContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogDebug("Index");

            List<SelectListItem> list = GetData();
            Kalkulator requestViewModel = new Kalkulator { Avti = list };
            return View(requestViewModel);
        }

        // Ob kliku na calculate, se zgodi ta metoda
        public IActionResult Calculate(Kalkulator kalkulator)
        {

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogDebug("Calculate");

            int odgovorCena;
            double odgovorSoncneCeliceProizvodnja;
            double razlikaDenarja;

            double cena1watt = 1.5;
            double porabaVozila;


            return View(kalkulator);
        }

        private static List<SelectListItem> GetData()
        {
            //prep the vcontroller-you can do this from db
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem selectListItemA = new SelectListItem { Text = "Tesla Model 3 Performance", Value = "37.990" };
            SelectListItem selectListItemB = new SelectListItem { Text = "CUPRA Born 110 kW - 45 kWh", Value = "28.000" };
            SelectListItem selectListItemC = new SelectListItem { Text = "Volkswagen ID.3 Pure Performance", Value = "47.000" };
            list.Add(selectListItemA);
            list.Add(selectListItemB);
            list.Add(selectListItemC);
            return list;
        }

    }
}