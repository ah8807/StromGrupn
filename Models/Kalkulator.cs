using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.Models
{
    public class Kalkulator
    {

        public Kalkulator()
        {

        }

        // Vprasanja
        public List<SelectListItem> Avti { get; set; }
        public string izbranAvto { get; set; }

        public List<SelectListItem> Energenti { get; set; }
        
        public string izbranEnergent { get; set; }

        public double kilometerPovprecje { get; set; }

        public double porabaPovprecje { get; set; }

        public string naslov { get; set; }

        public Boolean soncneCeliceVprasanje { get; set; }

        public double proizvodnjaKilovatnihUr { get; set; }

        // Odgovor
        public double odgovorCena { get; set; }

        public double odgovorSoncneCeliceProizvodnja { get; set; }

        public double razlikaDenarja { get; set; }
    }
}