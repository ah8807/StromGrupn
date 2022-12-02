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
            double cenaElektrikeWh = 0.16 / 1000;

            double cenaEnergenta;
            string energent;

            if (kalkulator.izbranEnergent.Equals("0"))
            {
                cenaEnergenta = cenaBenzin;
                energent = "Bencin";
            }
            else
            {
                cenaEnergenta = cenaDiezel;
                energent = "Diezel";
            }

            kalkulator.izbranEnergentIme = energent;

            // Formula za porabo v 1 letu: 
            double avtoTrenutniStrosek = avtoSteviloKilometrov * avtoporabaGoriva1km * cenaEnergenta;
            double avtoPrihodnjiStrosek = avtoSteviloKilometrov * avtoELPoraba * cenaElektrikeWh;

            double porabaWatnihUr1Leto = avtoSteviloKilometrov * avtoELPoraba;

            int kolicinaElektrineNaLetoNaPrikljucnoMoc = 779;

            double mocElektrarneKw = porabaWatnihUr1Leto / kolicinaElektrineNaLetoNaPrikljucnoMoc / 1000;

            double cenaElektrarne = mocElektrarneKw * 1000 / cena1watt;

            double investicija = avtoELCena + cenaElektrarne;

            

            kalkulator.investicija = investicija;
            kalkulator.mocElektrarneKw = mocElektrarneKw;
            kalkulator.avtoPrihodnjiStrosek = avtoPrihodnjiStrosek;
            kalkulator.avtoTrenutniStrosek = avtoTrenutniStrosek;

            logger.LogDebug("avtoSteviloKilometrov: " + avtoSteviloKilometrov + "\n"
                            + "avtoporabaGoriva1km: " + avtoporabaGoriva1km + "\n"
                            + "energent: " + energent + "\n"
                            + "avtoTrenutniStrosek: " + avtoTrenutniStrosek + "\n"
                            + "avtoPrihodnjiStrosek: " + avtoPrihodnjiStrosek + "\n"
                            + "avtoCena: " + avtoELCena + "\n"
                            + "mocElektrarne: " + mocElektrarneKw + "\n"
                            + "cenaElektrarne: " + cenaElektrarne + "\n"
                            + "investicija: " + investicija);

            Chart pieChart = new Chart();
            List<double?> values = new List<double?>();
            values.Add(avtoELCena);
            values.Add(cenaElektrarne);
            GeneratePieChart(pieChart, values);
            ViewData["PieChart"] = pieChart;

            Chart horizontalBarChart = GenerateHorizontalBarChart(energent, avtoTrenutniStrosek, avtoPrihodnjiStrosek);
            ViewData["HorizontalBarChart"] = horizontalBarChart;

            Chart horizontalBarChart2 = GenerateHorizontalBarChart(energent, avtoTrenutniStrosek, avtoPrihodnjiStrosek);
            ViewData["HorizontalBarChart2"] = horizontalBarChart2;

            Chart lineChart;
            if (energent.Equals("Bencin"))
            {
                lineChart = GenerateLineChartBencin();
            }
            else
            {
                lineChart = GenerateLineChartDiezel();
            }

            ViewData["LineChart"] = lineChart;

            return View(kalkulator);
        }

        private static Chart GeneratePieChart(Chart chart, List<double?> values)
        {
            chart.Type = Enums.ChartType.Pie;

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = new List<string>() { "Nakup električnega avtomovila", "Nakup sončnih celic" };

            PieDataset dataset = new PieDataset()
            {
                Label = "Podatki",
                BackgroundColor = new List<ChartColor>() {
                    ChartColor.FromHexString("#FF6384"),
                    ChartColor.FromHexString("#36A2EB")
                },
                HoverBackgroundColor = new List<ChartColor>() {
                    ChartColor.FromHexString("#FF6384"),
                    ChartColor.FromHexString("#36A2EB")
                },
                Data = values
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(dataset);

            chart.Data = data;

            return chart;
        }

        private static Chart GenerateHorizontalBarChart(String gorivo, double avtoTrenutniStrosek, double avtoPrihodnjiStrosek)
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Bar;

            chart.Data = new ChartJSCore.Models.Data()
            {
                Datasets = new List<Dataset>()
                {
                    {
                        new VerticalBarDataset()
                        {
                            Label = gorivo,
                            Data = new List<VerticalBarDataPoint?>()
                            {
                                new VerticalBarDataPoint(avtoTrenutniStrosek, 1)
                            },
                            BackgroundColor = new List<ChartColor>
                            {
                                ChartColor.FromHexString("#FF6384")
                            },
                            BorderWidth = new List<int>() { 2 },
                            IndexAxis = "y"
                        }
                    },
                    {
                        new VerticalBarDataset()
                        {
                            Label = "Elektrika",
                            Data = new List<VerticalBarDataPoint?>()
                            {
                                new VerticalBarDataPoint(avtoPrihodnjiStrosek, 2)
                            },
                            BackgroundColor = new List<ChartColor>
                            {
                                ChartColor.FromHexString("#36A2EB")
                            },
                            BorderWidth = new List<int>() { 2 },
                            IndexAxis = "y"
                        }
                    }
                }
            };

            chart.Options = new Options()
            {
                Responsive = true,
                Plugins = new ChartJSCore.Models.Plugins()
                {
                    Legend = new Legend()
                    {
                        Position = "right"
                    }
                }
            };

            return chart;
        }

        private static Chart GenerateLineChartBencin()
        {
            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Line;
            chart.Options.Scales = new Dictionary<string, Scale>();
            CartesianScale xAxis = new CartesianScale();
            xAxis.Display = true;
            xAxis.Title = new Title
            {
                Text = new List<string> { "Cena" },
                Display = true
            };
            chart.Options.Scales.Add("x", xAxis);

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data
            {
                Labels = new List<string> { "21.6. - 4.7.", "5.7. - 18.7.", "19.7. - 1.8.", "2.8. - 16.8.", "17.8. - 29.8.", "30.8. - 12.9.", "13.9. - 26.9.",
                "27.9. - 10.10.", "11.10. - 24.10.", "25.10. - 7.11.", "8.11. - 21.11.", "22.11. - 5.12." }
            };

            LineDataset dataset = new LineDataset()
            {
                Label = "Bencin",
                Data = new List<double?> { 1.755, 1.723, 1.62, 1.534, 1.49, 1.491, 1.354, 1.464, 1.482, 1.43, 1.414, 1.353 },
                Fill = "true",
                Tension = .01,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(75, 192, 192, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                BorderCapStyle = "butt",
                BorderDash = new List<int>(),
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            LineDataset dataset2 = new LineDataset()
            {
                Label = "Elektrika",
                Data = new List<double?> { 0.11, 0.14, 0.16, 0.21, 0.23, 0.25, 0.21, 0.21, 0.20, 0.18, 0.17, 0.16},
                Fill = "true",
                Tension = .01,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(43, 44, 170, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(43, 44, 170) },
                BorderCapStyle = "butt",
                BorderDash = new List<int>(),
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgba(43, 44, 170, 0.4) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgba(43, 44, 170, 0.4) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(43, 44, 170) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = new List<Dataset>
            {
                dataset, dataset2
            };

            chart.Data = data;

            ZoomOptions zoomOptions = new ZoomOptions
            {
                Zoom = new Zoom
                {
                    Wheel = new Wheel
                    {
                        Enabled = true
                    },
                    Pinch = new Pinch
                    {
                        Enabled = true
                    },
                    Drag = new Drag
                    {
                        Enabled = true,
                        ModifierKey = Enums.ModifierKey.alt
                    }
                },
                Pan = new Pan
                {
                    Enabled = true,
                    Mode = "xy"
                }
            };

            chart.Options.Plugins = new ChartJSCore.Models.Plugins
            {
                PluginDynamic = new Dictionary<string, object> { { "zoom", zoomOptions } }
            };

            return chart;
        }

        private static Chart GenerateLineChartDiezel()
        {
            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Line;
            chart.Options.Scales = new Dictionary<string, Scale>();
            CartesianScale xAxis = new CartesianScale();
            xAxis.Display = true;
            xAxis.Title = new Title
            {
                Text = new List<string> { "Cena" },
                Display = true
            };
            chart.Options.Scales.Add("x", xAxis);

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data
            {
                Labels = new List<string> { "21.6. - 4.7.", "5.7. - 18.7.", "19.7. - 1.8.", "2.8. - 16.8.", "17.8. - 29.8.", "30.8. - 12.9.", "13.9. - 26.9.",
                "27.9. - 10.10.", "11.10. - 24.10.", "25.10. - 7.11.", "8.11. - 21.11.", "22.11. - 5.12." }
            };

            LineDataset dataset = new LineDataset()
            {
                Label = "Diezel",
                Data = new List<double?> { 1.623, 1.718, 1.775, 1.684, 1.603, 1.683, 1.767, 1.641, 1.676, 1.7, 1.808, 1.848 },
                Fill = "true",
                Tension = .01,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(75, 192, 192, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                BorderCapStyle = "butt",
                BorderDash = new List<int>(),
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            LineDataset dataset2 = new LineDataset()
            {
                Label = "Elektrika",
                Data = new List<double?> { 0.11, 0.14, 0.16, 0.21, 0.23, 0.25, 0.21, 0.21, 0.20, 0.18, 0.17, 0.16},
                Fill = "true",
                Tension = .01,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(43, 44, 170, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(43, 44, 170) },
                BorderCapStyle = "butt",
                BorderDash = new List<int>(),
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgba(43, 44, 170, 0.4) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgba(43, 44, 170, 0.4) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(43, 44, 170) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = new List<Dataset>
            {
                dataset, dataset2
            };

            chart.Data = data;

            ZoomOptions zoomOptions = new ZoomOptions
            {
                Zoom = new Zoom
                {
                    Wheel = new Wheel
                    {
                        Enabled = true
                    },
                    Pinch = new Pinch
                    {
                        Enabled = true
                    },
                    Drag = new Drag
                    {
                        Enabled = true,
                        ModifierKey = Enums.ModifierKey.alt
                    }
                },
                Pan = new Pan
                {
                    Enabled = true,
                    Mode = "xy"
                }
            };

            chart.Options.Plugins = new ChartJSCore.Models.Plugins
            {
                PluginDynamic = new Dictionary<string, object> { { "zoom", zoomOptions } }
            };

            return chart;
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