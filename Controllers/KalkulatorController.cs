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

            List<SelectListItem> listAvti = GetAvtoData();
            List<SelectListItem> listEnergenti = GetEnergentiData();
            Kalkulator requestViewModel = new Kalkulator { Avti = listAvti, Energenti = listEnergenti };
            return View(requestViewModel);
        }

        // Ob kliku na calculate, se zgodi ta metoda
        public IActionResult Calculate(Kalkulator kalkulator)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogDebug("Calculate");

            // Avto (poraba in cena)
            int avtoCena = Convert.ToInt32(kalkulator.izbranAvto.Split(";")[0]);
            int avtoPoraba = Convert.ToInt32(kalkulator.izbranAvto.Split(";")[1]);

            // Prevozenih kilometrov
            double steviloKilometrov = kalkulator.kilometerPovprecje;
            // Poraba goriva
            double porabaGoriva1km = kalkulator.porabaPovprecje / 100;


            double odgovorCena;
            double odgovorSoncneCeliceProizvodnja;
            double razlikaDenarja;

            double cena1watt = 1.5;
            double cenaDiezel = 1.65;
            double cenaBenzin = 1.55;
            double cenaElektrikeWh = 0.16/1000;

            double energent;

            if (kalkulator.izbranEnergent.Equals("0"))
            {
                energent = cenaBenzin;
            }
            else
            {
                energent = cenaDiezel;
            }

            // Formula za porabo v 1 letu: 
            double avtoTrenutniStrosek = steviloKilometrov * porabaGoriva1km * energent;
            double avtoPrihodnjiStrosek = steviloKilometrov * avtoPoraba * cenaElektrikeWh;

            logger.LogDebug("avtoTrenutniStrosek: " + avtoTrenutniStrosek + "\n"
                            + "avtoPrihodnjiStrosek: " + avtoPrihodnjiStrosek + "\n"
                            + "avtoCena: " + avtoCena);

            return View(kalkulator);
        }

        private static List<SelectListItem> GetAvtoData()
        {
            //prep the vcontroller-you can do this from db
            List<SelectListItem> listAvti = new List<SelectListItem>();
            SelectListItem selectListItemA = new SelectListItem { Text = "Tesla Model 3 Performance", Value = "66465;163" };
            SelectListItem selectListItemB = new SelectListItem { Text = "CUPRA Born 110 kW - 45 kWh", Value = "32700;164" };
            SelectListItem selectListItemC = new SelectListItem { Text = "Volkswagen ID.3 Pure Performance", Value = "37990;164" };
            listAvti.Add(selectListItemA);
            listAvti.Add(selectListItemB);
            listAvti.Add(selectListItemC);

            return listAvti;
        }

        private static List<SelectListItem> GetEnergentiData()
        {
            List<SelectListItem> listEnergenti = new List<SelectListItem>();
            SelectListItem selectListItem1 = new SelectListItem { Text = "Bencin", Value = "0" };
            SelectListItem selectListItem2 = new SelectListItem { Text = "Diezel", Value = "1" };
            listEnergenti.Add(selectListItem1);
            listEnergenti.Add(selectListItem2);

            return listEnergenti;
        }

    }
}