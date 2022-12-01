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
            int avtoELCena = Convert.ToInt32(kalkulator.izbranAvto.Split(";")[0]);
            int avtoELPoraba = Convert.ToInt32(kalkulator.izbranAvto.Split(";")[1]);

            // Prevozenih kilometrov
            double avtoSteviloKilometrov = kalkulator.kilometerPovprecje;
            // Poraba goriva
            double avtoporabaGoriva1km = kalkulator.porabaPovprecje / 100;

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
            double avtoTrenutniStrosek = avtoSteviloKilometrov * avtoporabaGoriva1km * energent;
            double avtoPrihodnjiStrosek = avtoSteviloKilometrov * avtoELPoraba * cenaElektrikeWh;

            double porabaWatnihUr1Leto = avtoSteviloKilometrov * avtoELPoraba;

            int kolicinaElektrineNaLetoNaPrikljucnoMoc = 779;

            double mocElektrarneKw = porabaWatnihUr1Leto/kolicinaElektrineNaLetoNaPrikljucnoMoc/1000;

            double cenaElektrarne = mocElektrarneKw * 1000 / cena1watt;

            double investicija = avtoELCena + cenaElektrarne;

            logger.LogDebug("avtoSteviloKilometrov: " + avtoSteviloKilometrov + "\n"
                            + "avtoporabaGoriva1km: " + avtoporabaGoriva1km + "\n"
                            + "energent: " + energent + "\n"
                            + "avtoTrenutniStrosek: " + avtoTrenutniStrosek + "\n"
                            + "avtoPrihodnjiStrosek: " + avtoPrihodnjiStrosek + "\n"
                            + "avtoCena: " + avtoELCena + "\n"
                            + "mocElektrarne: " + mocElektrarneKw + "\n"
                            + "cenaElektrarne: " + cenaElektrarne + "\n"
                            + "investicija: " + investicija);

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