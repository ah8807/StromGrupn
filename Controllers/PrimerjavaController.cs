using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using web.Data;
using web.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChartJSCore.Models;
using ChartJSCore.Helpers;
using ChartJSCore.Plugins.Zoom;

namespace web.Controllers;

public class PrimerjavaController : Controller
{
       private readonly StromGrupnContext _context;

        public PrimerjavaController(StromGrupnContext context)
        {
            _context = context;
        }
   public IActionResult Index(){
    return View();
   }
}