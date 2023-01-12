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
            List<SelectListItem> listCrpalke = GetCrpalkeData();

            Kalkulator requestViewModel = new Kalkulator { Avti = listAvti, Energenti = listEnergenti, Crpalke = listCrpalke };
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

            int crpalkaCena = Convert.ToInt32(kalkulator.izbranaCrpalka.Split(";")[0]);
            int crpalkaPorabaNaDanvWh = Convert.ToInt32(kalkulator.izbranaCrpalka.Split(";")[1]);
            double crpalkaPorabaNaLeto = crpalkaPorabaNaDanvWh * 365 * kalkulator.izbranaPovrsina;
            double cena_starega_avta = kalkulator.izbranStrosekAvtaObNakupu;

            // Formula za porabo v 1 letu: 
            double avtoTrenutniStrosek = avtoSteviloKilometrov * avtoporabaGoriva1km * cenaEnergenta;
            double avtoPrihodnjiStrosek = avtoSteviloKilometrov * avtoELPoraba * cenaElektrikeWh;

            double porabaWatnihUr1Leto = avtoSteviloKilometrov * avtoELPoraba;

            int kolicinaElektrineNaLetoNaPrikljucnoMoc = 779;

            double mocElektrarneKw = porabaWatnihUr1Leto / kolicinaElektrineNaLetoNaPrikljucnoMoc / 1000;

            double cenaElektrarne = mocElektrarneKw * 1000 / cena1watt;

            double investicija = avtoELCena + cenaElektrarne + crpalkaCena;

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
                            + "investicija: " + investicija + "\n"
                            + "crpalkaPorabaNaLeto: " + crpalkaPorabaNaLeto + "\n"
                            + "cena_starega_avta: " + cena_starega_avta + "\n");


            List<List<double?>> consumptionByMonth = generateConsumptionByMonth(energent, cenaElektrarne, avtoELCena, cena_starega_avta, avtoSteviloKilometrov,
                avtoporabaGoriva1km);


            Chart lineChartByMonth = GenerateLineChartConsByMonth(consumptionByMonth);
            ViewData["LineChartByMonth"] = lineChartByMonth;


            Chart pieChart = new Chart();
            List<double?> values = new List<double?>();
            values.Add(avtoELCena);
            values.Add(cenaElektrarne);
            values.Add(crpalkaCena);
            GeneratePieChart(pieChart, values);
            ViewData["PieChart"] = pieChart;

            Chart horizontalBarChart = GenerateHorizontalBarChart(energent, "Elektrika", avtoTrenutniStrosek, avtoPrihodnjiStrosek);
            ViewData["HorizontalBarChart"] = horizontalBarChart;

            Chart horizontalBarChart2 = GenerateHorizontalBarChart(energent, "Elektrika", avtoTrenutniStrosek, avtoPrihodnjiStrosek);
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

            Chart horizontalBarChartPorabaElektrike = GenerateHorizontalBarChart("Toplotna črpalka", "Električni avtomobil", crpalkaPorabaNaLeto, porabaWatnihUr1Leto);
            ViewData["HorizontalBarChartCrpalkaElektrika"] = horizontalBarChartPorabaElektrike;

            return View(kalkulator);
        }

        private static Chart GeneratePieChart(Chart chart, List<double?> values)
        {
            chart.Type = Enums.ChartType.Pie;

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = new List<string>() { "Nakup električnega avtomovila", "Nakup sončnih celic", "Nakup in montaža toplotne črpalke" };

            PieDataset dataset = new PieDataset()
            {
                Label = "Podatki",
                BackgroundColor = new List<ChartColor>() {
                    ChartColor.FromHexString("#FF6384"),
                    ChartColor.FromHexString("#36A2EB"),
                    ChartColor.FromHexString("#7CFC00")
                },
                HoverBackgroundColor = new List<ChartColor>() {
                    ChartColor.FromHexString("#FF6384"),
                    ChartColor.FromHexString("#36A2EB"),
                    ChartColor.FromHexString("#7CFC00")
                },
                Data = values
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(dataset);

            chart.Data = data;

            return chart;
        }

        private static Chart GenerateHorizontalBarChart(String label1, String label2, double value1, double value2)
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
                            Label = label1,
                            Data = new List<VerticalBarDataPoint?>()
                            {
                                new VerticalBarDataPoint(value1, 1)
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
                            Label = label2,
                            Data = new List<VerticalBarDataPoint?>()
                            {
                                new VerticalBarDataPoint(value2, 2)
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

        private static Chart GenerateLineChartConsByMonth(List<List<double?>> data)
        {
            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Line;
            chart.Options.Scales = new Dictionary<string, Scale>();
            CartesianScale xAxis = new CartesianScale();
            xAxis.Display = true;
            xAxis.Title = new Title
            {
                Text = new List<string> { "stroški glede na mesec" },
                Display = true
            };
            chart.Options.Scales.Add("x", xAxis);
            List<string> labels = new List<string>();
            int u = 0;
            foreach (double x in data[0]){
                labels.Add(u+"");
                u++;
            }
            ChartJSCore.Models.Data x_axis_lables = new ChartJSCore.Models.Data
            {
                Labels = labels
            };

            LineDataset dataset = new LineDataset()
            {
                Label = "Fosilno gorivo",
                Data = data[1],
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
                Data = data[0],
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

            x_axis_lables.Datasets = new List<Dataset>
            {
                dataset, dataset2
            };

            chart.Data = x_axis_lables;

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
                Data = new List<double?> { 0.11, 0.14, 0.16, 0.21, 0.23, 0.25, 0.21, 0.21, 0.20, 0.18, 0.17, 0.16 },
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
                Data = new List<double?> { 0.11, 0.14, 0.16, 0.21, 0.23, 0.25, 0.21, 0.21, 0.20, 0.18, 0.17, 0.16 },
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

        private static List<SelectListItem> GetCrpalkeData()
        {
            List<SelectListItem> listCrpalke = new List<SelectListItem>();
            SelectListItem selectListItem1 = new SelectListItem { Text = "Toplotna črpalka VODA - voda", Value = "11300;55" };
            SelectListItem selectListItem2 = new SelectListItem { Text = "Toplotna črpalka ZEMLJA - voda", Value = "11700;75" };
            SelectListItem selectListItem3 = new SelectListItem { Text = "Toplotna črpalka ZRAK - voda", Value = "7250;90" };
            listCrpalke.Add(selectListItem1);
            listCrpalke.Add(selectListItem2);
            listCrpalke.Add(selectListItem3);

            return listCrpalke;
        }

        public new List<List<double?>> generateConsumptionByMonth(string energent, double cenaElektrarne, double cenaElAvta, double cenaAvta, double km_na_leto, double liter_na_100_km)
        {
            List<double?> valuesElektrika = new List<double?>();
            List<double?> valuesEnergent = new List<double?>();
            double bencin =
                (1.755 + 1.723 + 1.62 + 1.534 + 1.49 + 1.491 + 1.354 + 1.464 + 1.482 + 1.43 + 1.414 + 1.353) / 12;
            double dizel =
                (1.623 + 1.718 + 1.775 + 1.684 + 1.603 + 1.683 + 1.767 + 1.641 + 1.676 + 1.7 + 1.808 + 1.848) / 12;
            double elektrika =
                (0.11 + 0.14 + 0.16 + 0.21 + 0.23 + 0.25 + 0.21 + 0.21 + 0.20 + 0.18 + 0.17 + 0.16) / 12;
            double sumElektrika = cenaElAvta+cenaElektrarne;
            double sumEnergent = cenaAvta;

            double energent_cena;
            if (energent.Equals("Dizel"))
                energent_cena = dizel;
            else
                energent_cena = bencin;

            while (sumElektrika > sumEnergent)
            {
                sumEnergent += (km_na_leto / 12 * liter_na_100_km) * energent_cena;
                valuesEnergent.Add(sumEnergent);
                //sumElektrika += (km_na_leto / 12 * liter_na_100_km / 100)*elektrika;
                valuesElektrika.Add(sumElektrika);
            }

            for (int i = 0; i <= 10; i++)
            {
                sumEnergent += (km_na_leto / 12 * liter_na_100_km) * energent_cena;
                valuesEnergent.Add(sumEnergent);
                valuesElektrika.Add(sumElektrika);
            }

            return new List<List<double?>> { valuesElektrika, valuesEnergent };
        }

        private static List<SelectListItem> GetAvtoData()
        {
            //prep the vcontroller-you can do this from db
            List<SelectListItem> listAvti = new List<SelectListItem>();

            listAvti.Add(new SelectListItem { Text = "Lightyear 0 297.000", Value = "297.000;109" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 6 Standard Range 2WD 43.900", Value = "43.900;150" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model 3 52.965", Value = "52.965;151" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model 3 Long Range Dual Motor 62.465", Value = "62.465;155" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 6 Long Range 2WD 54.000", Value = "54.000;156" });
            listAvti.Add(new SelectListItem { Text = "XPENG P5 48.000", Value = "48.000;156" });
            listAvti.Add(new SelectListItem { Text = "Hyundai Kona Electric 39 kWh 36.400", Value = "36.400;157" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Pure 100.000", Value = "100.000;157" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Hatchback 24 kWh 30.990", Value = "30.990;158" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e 3+1 24 kWh 32.290", Value = "32.290;158" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Cabrio 24 kWh 35.490", Value = "35.490;158" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-208 35.350", Value = "35.350;158" });
            listAvti.Add(new SelectListItem { Text = "Opel Corsa-e 33.895", Value = "33.895;158" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Hatchback 42 kWh 34.990", Value = "34.990;159" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e 3+1 42 kWh 36.990", Value = "36.990;159" });
            listAvti.Add(new SelectListItem { Text = "Renault Megane E-Tech EV40 130hp 42.000", Value = "42.000;160" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Touring 120.000", Value = "120.000;160" });
            listAvti.Add(new SelectListItem { Text = "Mini Cooper SE 35.700", Value = "35.700;161" });
            listAvti.Add(new SelectListItem { Text = "Dacia Spring Electric 22.550", Value = "22.550;161" });
            listAvti.Add(new SelectListItem { Text = "Hyundai Kona Electric 64 kWh 42.900", Value = "42.900;162" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Cabrio 42 kWh 37.990", Value = "37.990;162" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model 3 Performance 66.465", Value = "66.465;163" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 110 kW - 45 kWh 32.700", Value = "32.700;164" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pure Performance 37.990", Value = "37.990;164" });
            listAvti.Add(new SelectListItem { Text = "Renault Twingo Electric 28.000", Value = "28.000;164" });
            listAvti.Add(new SelectListItem { Text = "Renault Megane E-Tech EV60 130hp 47.000", Value = "47.000;164" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-308 44.000", Value = "44.000;165" });
            listAvti.Add(new SelectListItem { Text = "Renault Zoe ZE50 R110 36.840", Value = "36.840;165" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 150 kW - 58 kWh 39.370", Value = "39.370;166" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro 35.460", Value = "35.460;166" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro Performance 38.060", Value = "38.060;166" });
            listAvti.Add(new SelectListItem { Text = "Abarth 500e Scorpionissima 42.000", Value = "42.000;166" });
            listAvti.Add(new SelectListItem { Text = "Nissan Leaf 33.400", Value = "33.400;166" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model S Dual Motor 105.000", Value = "105.000;167" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model Y 56.965", Value = "56.965;167" });
            listAvti.Add(new SelectListItem { Text = "Renault Megane E-Tech EV60 220hp 46.600", Value = "46.600;167" });
            listAvti.Add(new SelectListItem { Text = "Smart EQ fortwo coupe 21.940", Value = "21.940;167" });
            listAvti.Add(new SelectListItem { Text = "BMW i4 eDrive35 56.500", Value = "56.500;168" });
            listAvti.Add(new SelectListItem { Text = "Honda e Advance 39.900", Value = "39.900;168" });
            listAvti.Add(new SelectListItem { Text = "Renault Zoe ZE50 R135 37.840", Value = "37.840;168" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 170 kW - 58 kWh 40.750", Value = "40.750;168" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 6 Long Range AWD 61.100", Value = "61.100;168" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Grand Touring 175.000", Value = "175.000;168" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 450+ 109.551", Value = "109.551;168" });
            listAvti.Add(new SelectListItem { Text = "MG MG4 Electric 64 kWh 35.990", Value = "35.990;169" });
            listAvti.Add(new SelectListItem { Text = "MG MG4 Electric 51 kWh 31.990", Value = "31.990;169" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Dream Edition R 218.000", Value = "218.000;170" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-C4 36.040", Value = "36.040;170" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-C4 X 38.000", Value = "38.000;170" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-308 SW 45.000", Value = "45.000;170" });
            listAvti.Add(new SelectListItem { Text = "Kia e-Soul 39.2 kWh 40.290", Value = "40.290;170" });
            listAvti.Add(new SelectListItem { Text = "Kia Niro EV 47.590", Value = "47.590;171" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 170 kW - 77 kWh 46.450", Value = "46.450;171" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro S - 4 Seats 43.560", Value = "43.560;171" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro S - 5 Seats 43.720", Value = "43.720;171" });
            listAvti.Add(new SelectListItem { Text = "Aiways U6 47.588", Value = "47.588;171" });
            listAvti.Add(new SelectListItem { Text = "BMW i4 eDrive40 59.200", Value = "59.200;172" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model Y Long Range Dual Motor 59.965", Value = "59.965;172" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 350+ 70.626", Value = "70.626;173" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 300 66.402", Value = "66.402;173" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 350 70.210", Value = "70.210;173" });
            listAvti.Add(new SelectListItem { Text = "Kia e-Soul 64 kWh 46.950", Value = "46.950;173" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV 80 47.000", Value = "47.000;173" });
            listAvti.Add(new SelectListItem { Text = "DS 3 Crossback E-Tense 40.540", Value = "40.540;173" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV 60 42.000", Value = "42.000;173" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 88.399", Value = "88.399;173" });
            listAvti.Add(new SelectListItem { Text = "JAC iEV7s 29.900", Value = "29.900;173" });
            listAvti.Add(new SelectListItem { Text = "Nissan Leaf e+ 41.100", Value = "41.100;174" });
            listAvti.Add(new SelectListItem { Text = "ORA Funky Cat First Edition 36.971", Value = "36.971;174" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 450 4MATIC 113.359", Value = "113.359;175" });
            listAvti.Add(new SelectListItem { Text = "MG MG5 Electric Long Range 38.490", Value = "38.490;175" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV 60 42.100", Value = "42.100;176" });
            listAvti.Add(new SelectListItem { Text = "Smart EQ fortwo cabrio 25.200", Value = "25.200;176" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model S Plaid 138.970", Value = "138.970;176" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Plus 94.123", Value = "94.123;176" });
            listAvti.Add(new SelectListItem { Text = "NIO ET7 75 kWh 82.850", Value = "82.850;176" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 35 46.700", Value = "46.700;176" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-2008 SUV 37.900", Value = "37.900;176" });
            listAvti.Add(new SelectListItem { Text = "Opel Mokka-e 37.650", Value = "37.650;176" });
            listAvti.Add(new SelectListItem { Text = "Mazda MX-30 35.990", Value = "35.990;176" });
            listAvti.Add(new SelectListItem { Text = "MG MG5 Electric Standard Range 35.490", Value = "35.490;177" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 Standard Range 2WD 46.990", Value = "46.990;177" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 500 4MATIC 125.378", Value = "125.378;178" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 580 4MATIC 141.705", Value = "141.705;178" });
            listAvti.Add(new SelectListItem { Text = "NIO ET7 100 kWh 91.850", Value = "91.850;178" });
            listAvti.Add(new SelectListItem { Text = "NIO ET5 75 kWh 62.850", Value = "62.850;178" });
            listAvti.Add(new SelectListItem { Text = "BYD HAN 70.805", Value = "70.805;178" });
            listAvti.Add(new SelectListItem { Text = "Polestar 2 Long Range Single Motor 50.995", Value = "50.995;179" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.5 Pro 47.935", Value = "47.935;179" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.5 Pro Performance 48.970", Value = "48.970;179" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV 80x 49.000", Value = "49.000;179" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S 108.867", Value = "108.867;180" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Dream Edition P 218.000", Value = "218.000;180" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 40 53.900", Value = "53.900;180" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 Long Range 2WD 50.990", Value = "50.990;180" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model Y Performance 68.465", Value = "68.465;181" });
            listAvti.Add(new SelectListItem { Text = "Sono Sion 29.899", Value = "29.899;181" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 350 4MATIC 73.363", Value = "73.363;181" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 500 4MATIC 87.286", Value = "87.286;181" });
            listAvti.Add(new SelectListItem { Text = "MG ZS EV Standard Range 33.990", Value = "33.990;181" });
            listAvti.Add(new SelectListItem { Text = "NIO ET5 100 kWh 71.850", Value = "71.850;182" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Plus 114.389", Value = "114.389;182" });
            listAvti.Add(new SelectListItem { Text = "BMW iX1 xDrive30 55.000", Value = "55.000;182" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pure Performance 38.450", Value = "38.450;182" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 35 44.700", Value = "44.700;182" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pure 36.950", Value = "36.950;182" });
            listAvti.Add(new SelectListItem { Text = "Smart #1 41.490", Value = "41.490;183" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 5 Standard Range 2WD 43.900", Value = "43.900;183" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV 80 46.800", Value = "46.800;183" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 Long Range AWD 54.980", Value = "54.980;185" });
            listAvti.Add(new SelectListItem { Text = "BMW i4 M50 70.800", Value = "70.800;186" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan GTS 134.214", Value = "134.214;186" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE AMG 43 4MATIC 103.827", Value = "103.827;187" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Sport Turismo 89.351", Value = "89.351;187" });
            listAvti.Add(new SelectListItem { Text = "MG ZS EV Long Range 37.990", Value = "37.990;187" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 250 50.777", Value = "50.777;187" });
            listAvti.Add(new SelectListItem { Text = "Genesis G80 Electrified Luxury 69.200", Value = "69.200;188" });
            listAvti.Add(new SelectListItem { Text = "BYD ATTO 3 44.625", Value = "44.625;188" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pro Performance 46.335", Value = "46.335;188" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pro 48.190", Value = "48.190;188" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 250+ 52.205", Value = "52.205;188" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya 63kWh 47.490", Value = "47.490;188" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Plus Sport Turismo 95.074", Value = "95.074;188" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE AMG 53 4MATIC+ 109.777", Value = "109.777;189" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 40 51.900", Value = "51.900;189" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV60 Premium 54,342", Value = "54,342;190" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 5 Long Range 2WD 47.900", Value = "47.900;190" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 300 4MATIC 53.746", Value = "53.746;190" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 350 4MATIC 56.424", Value = "56.424;190" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.5 GTX 56.455", Value = "56.455;190" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV RS 61.960", Value = "61.960;190" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pro 4MOTION 49.020", Value = "49.020;190" });
            listAvti.Add(new SelectListItem { Text = "Toyota bZ4X FWD 47.490", Value = "47.490;190" });
            listAvti.Add(new SelectListItem { Text = "Aiways U5 39.563", Value = "39.563;190" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS AMG 53 4MATIC+ 155.009", Value = "155.009;191" });
            listAvti.Add(new SelectListItem { Text = "Smart #1 Brabus 48.990", Value = "48.990;191" });
            listAvti.Add(new SelectListItem { Text = "Lexus UX 300e 47.550", Value = "47.550;191" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 45 quattro 55.900", Value = "55.900;192" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 50 quattro 59.900", Value = "59.900;192" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Sport Turismo 109.819", Value = "109.819;192" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 5 Long Range AWD 59.200", Value = "59.200;192" });
            listAvti.Add(new SelectListItem { Text = "BMW iX3 67.300", Value = "67.300;192" });
            listAvti.Add(new SelectListItem { Text = "Polestar 2 Long Range Dual Motor 54.995", Value = "54.995;192" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV 80x 49.050", Value = "49.050;193" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 GTX 53.255", Value = "53.255;193" });
            listAvti.Add(new SelectListItem { Text = "Seres 3 37.995", Value = "37.995;193" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 250 52.550", Value = "52.550;193" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV 350+ 85.000", Value = "85.000;193" });
            listAvti.Add(new SelectListItem { Text = "Toyota bZ4X AWD 57.390", Value = "57.390;193" });
            listAvti.Add(new SelectListItem { Text = "SsangYong Korando e-Motion 40.490", Value = "40.490;193" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 250+ 53.978", Value = "53.978;193" });
            listAvti.Add(new SelectListItem { Text = "MG Marvel R 46.990", Value = "46.990;194" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Plus Sport Turismo 115.341", Value = "115.341;195" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV60 Sport 56.370", Value = "56.370;195" });
            listAvti.Add(new SelectListItem { Text = "Polestar 2 Long Range Performance 61.495", Value = "61.495;195" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya 87kWh 63.490", Value = "63.490;196" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean Extreme 69.950", Value = "69.950;196" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean One 69.950", Value = "69.950;196" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan GTS Sport Turismo 135.166", Value = "135.166;197" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Cross Turismo 114.222", Value = "114.222;197" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4 Cross Turismo 98.514", Value = "98.514;197" });
            listAvti.Add(new SelectListItem { Text = "MG Marvel R Performance 50.990", Value = "50.990;197" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E SR RWD 62.900", Value = "62.900;197" });
            listAvti.Add(new SelectListItem { Text = "BMW iX xDrive40 77.300", Value = "77.300;197" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean Sport 41.560", Value = "41.560;197" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean Ultra 57.000", Value = "57.000;198" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 300 4MATIC 55.519", Value = "55.519;199" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 350 4MATIC 58.197", Value = "58.197;199" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 50 quattro 57.900", Value = "57.900;199" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 45 quattro 53.990", Value = "53.990;199" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV 350 4MATIC 90.000", Value = "90.000;199" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV 500 4MATIC 115.000", Value = "115.000;199" });
            listAvti.Add(new SelectListItem { Text = "BMW i7 xDrive60 135.900", Value = "135.900;199" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV RS 60.000", Value = "60.000;200" });
            listAvti.Add(new SelectListItem { Text = "Volvo XC40 Recharge Pure Electric 45.080", Value = "45.080;200" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 GT 69.990", Value = "69.990;200" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E ER RWD 71.200", Value = "71.200;200" });
            listAvti.Add(new SelectListItem { Text = "Subaru Solterra AWD 57.490", Value = "57.490;201" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo 156.348", Value = "156.348;202" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron Sportback 55 quattro 87.550", Value = "87.550;202" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model X Dual Motor 110.000", Value = "110.000;202" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron Sportback 50 quattro 76.650", Value = "76.650;202" });
            listAvti.Add(new SelectListItem { Text = "Audi e-tron GT quattro 106.050", Value = "106.050;202" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV60 Sport Plus 71.010", Value = "71.010;203" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E SR AWD 69.200", Value = "69.200;203" });
            listAvti.Add(new SelectListItem { Text = "Volvo C40 Recharge Pure Electric 48.850", Value = "48.850;203" });
            listAvti.Add(new SelectListItem { Text = "Lexus RZ 450e 60.000", Value = "60.000;204" });
            listAvti.Add(new SelectListItem { Text = "Lotus Eletre 95.990", Value = "95.990;206" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo Sport Turismo 157.300", Value = "157.300;207" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E ER AWD 77.200", Value = "77.200;207" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya e-4ORCE 87kWh 66.490", Value = "66.490;207" });
            listAvti.Add(new SelectListItem { Text = "NIO EL7 75 kWh 86.850", Value = "86.850;207" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV AMG 43 4MATIC 120.000", Value = "120.000;208" });
            listAvti.Add(new SelectListItem { Text = "BMW iX xDrive50 100.100", Value = "100.100;208" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo S 189.668", Value = "189.668;209" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo Cross Turismo 157.776", Value = "157.776;209" });
            listAvti.Add(new SelectListItem { Text = "NIO EL7 100 kWh 95.850", Value = "95.850;209" });
            listAvti.Add(new SelectListItem { Text = "Audi e-tron GT RS 146.050", Value = "146.050;210" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model X Plaid 141.970", Value = "141.970;211" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV70 Electrified Sport 67.300", Value = "67.300;211" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo S Sport Turismo 190.620", Value = "190.620;212" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV AMG 53 4MATIC+ 135.000", Value = "135.000;213" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E GT 86.200", Value = "86.200;214" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron 55 quattro 85.300", Value = "85.300;214" });
            listAvti.Add(new SelectListItem { Text = "Volvo C40 Recharge Twin Pure Electric 57.650", Value = "57.650;214" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron 50 quattro 74.400", Value = "74.400;214" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQC 400 4MATIC 66.069", Value = "66.069;216" });
            listAvti.Add(new SelectListItem { Text = "BMW iX M60 136.100", Value = "136.100;217" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo S Cross Turismo 191.096", Value = "191.096;217" });
            listAvti.Add(new SelectListItem { Text = "Polestar 3 Long Range Dual motor 88.600", Value = "88.600;218" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS SUV 450+ 110.658", Value = "110.658;219" });
            listAvti.Add(new SelectListItem { Text = "Toyota Proace City Verso Electric L1 50 kWh 37.800", Value = "37.800;220" });
            listAvti.Add(new SelectListItem { Text = "Rolls-Royce Spectre 400.000", Value = "400.000;220" });
            listAvti.Add(new SelectListItem { Text = "Volvo XC40 Recharge Twin Pure Electric 56.150", Value = "56.150;221" });
            listAvti.Add(new SelectListItem { Text = "Jaguar I-Pace EV400 76.815", Value = "76.815;223" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS SUV 450 4MATIC 114.446", Value = "114.446;224" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS SUV 580 4MATIC 135.291", Value = "135.291;224" });
            listAvti.Add(new SelectListItem { Text = "Opel Combo-e Life 50 kWh 43.050", Value = "43.050;225" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Berlingo M 50 kWh 37.790", Value = "37.790;225" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Rifter Standard 50 kWh 41.240", Value = "41.240;225" });
            listAvti.Add(new SelectListItem { Text = "Toyota Proace City Verso Electric L2 50 kWh 40.150", Value = "40.150;225" });
            listAvti.Add(new SelectListItem { Text = "Audi SQ8 e-tron Sportback 115.000", Value = "115.000;226" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya e-4ORCE 87kWh Performance 66.000", Value = "66.000;226" });
            listAvti.Add(new SelectListItem { Text = "Polestar 3 Long Range Performance 95.200", Value = "95.200;228" });
            listAvti.Add(new SelectListItem { Text = "Opel Combo-e Life XL 50 kWh 44.750", Value = "44.750;231" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Berlingo XL 50 kWh 43.640", Value = "43.640;231" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Rifter Long 50 kWh 42.440", Value = "42.440;231" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID. Buzz Pro 64.581", Value = "64.581;233" });
            listAvti.Add(new SelectListItem { Text = "Volvo EX90 Twin Motor 105.550", Value = "105.550;235" });
            listAvti.Add(new SelectListItem { Text = "Lotus Eletre R 150.990", Value = "150.990;238" });
            listAvti.Add(new SelectListItem { Text = "Audi SQ8 e-tron 110.000", Value = "110.000;238" });
            listAvti.Add(new SelectListItem { Text = "BYD TANG 71.400", Value = "71.400;240" });
            listAvti.Add(new SelectListItem { Text = "Volvo EX90 Twin Motor Performance 110.650", Value = "110.650;243" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi M 50 kWh 51.940", Value = "51.940;250" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer M 50 kWh 53.640", Value = "53.640;250" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi M 50 kWh 50.992", Value = "50.992;250" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life M 50 kWh 63.250", Value = "63.250;250" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Standard 50 kWh 51.940", Value = "51.940;250" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Standard 50 kWh 53.640", Value = "53.640;250" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L2 50 kWh 55.990", Value = "55.990;250" });
            listAvti.Add(new SelectListItem { Text = "Hongqi E-HS9 84 kWh 83.590", Value = "83.590;251" });
            listAvti.Add(new SelectListItem { Text = "Hongqi E-HS9 99 kWh 94.590", Value = "94.590;254" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi M 75 kWh 57.940", Value = "57.940;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer M 75 kWh 59.640", Value = "59.640;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi M 75 kWh 56.942", Value = "56.942;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life M 75 kWh 69.250", Value = "69.250;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Standard 75 kWh 57.940", Value = "57.940;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Standard 75 kWh 59.640", Value = "59.640;257" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L2 75 kWh 61.990", Value = "61.990;257" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Verso M 75 kWh 64.530", Value = "64.530;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi XL 50 kWh 52.730", Value = "52.730;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer XL 50 kWh 54.430", Value = "54.430;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi L 50 kWh 51.825", Value = "51.825;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life L 50 kWh 64.075", Value = "64.075;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Long 50 kWh 52.730", Value = "52.730;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Long 50 kWh 54.430", Value = "54.430;257" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle M 50 kWh 58.845", Value = "58.845;257" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle L 50 kWh 68.545", Value = "68.545;257" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L3 50 kWh 56.990", Value = "56.990;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi XL 75 kWh 58.730", Value = "58.730;262" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer XL 75 kWh 60.430", Value = "60.430;262" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi L 75 kWh 57.775", Value = "57.775;262" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life L 75 kWh 70.075", Value = "70.075;262" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Long 75 kWh 58.730", Value = "58.730;262" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Long 75 kWh 60.430", Value = "60.430;262" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle M 75 kWh 60.045", Value = "60.045;262" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle L 75 kWh 69.745", Value = "69.745;262" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L3 75 kWh 62.990", Value = "62.990;262" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Verso L 75 kWh 65.385", Value = "65.385;262" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Long 60 kWh 60.678", Value = "60.678;286" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Long 90 kWh 64.248", Value = "64.248;290" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 250 Long 68.056", Value = "68.056;293" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 250 Extra-Long 68.949", Value = "68.949;293" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Extra-Long 60 kWh 61.571", Value = "61.571;293" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 300 Extra-Long 72.519", Value = "72.519;295" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Extra-Long 90 kWh 65.140", Value = "65.140;295" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 300 Long 71.626", Value = "71.626;295" });

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