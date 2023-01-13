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

            double porabaWatnihUr1Leto = (avtoSteviloKilometrov * avtoELPoraba) + crpalkaPorabaNaLeto;

            double strosekCrpalkePorabaNaLetovWh = crpalkaPorabaNaLeto * cenaElektrikeWh;

            kalkulator.strosekCrpalkePorabaNaLetovWh = strosekCrpalkePorabaNaLetovWh;

            kalkulator.porabaWatnihUr1Leto = porabaWatnihUr1Leto;

            int kolicinaElektrineNaLetoNaPrikljucnoMoc = 779;

            double mocElektrarneKw = porabaWatnihUr1Leto / kolicinaElektrineNaLetoNaPrikljucnoMoc / 1000;

            double cenaElektrarne = mocElektrarneKw * 1000 / cena1watt;

            double investicija = avtoELCena + cenaElektrarne + crpalkaCena;

            double trenutniStrosek = avtoTrenutniStrosek + kalkulator.strosekTrenutnegaOgrevanja;

            double prihodnjiStrosek = avtoPrihodnjiStrosek + strosekCrpalkePorabaNaLetovWh;

            kalkulator.prihranek = trenutniStrosek - prihodnjiStrosek;

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
                            + "cena_starega_avta: " + cena_starega_avta + "\n"
                            + "strosekCrpalkePorabaNaLetovWh: " + strosekCrpalkePorabaNaLetovWh + "\n"
                            + "porabaWatnihUr1Leto: " + porabaWatnihUr1Leto + "\n");


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

            Chart horizontalBarChartstrosekCrpalkeInKlasicnoOgrevanje = GenerateHorizontalBarChart("Toplotna črpalka", "Trenutno ogrevanje", strosekCrpalkePorabaNaLetovWh, kalkulator.strosekTrenutnegaOgrevanja);
            ViewData["horizontalBarChartstrosekCrpalkeInKlasicnoOgrevanje"] = horizontalBarChartstrosekCrpalkeInKlasicnoOgrevanje;

            Chart horizontalBarChartRazlikaLetnihStroskov = GenerateHorizontalBarChart("TČ + EV", "Trenuten", prihodnjiStrosek, trenutniStrosek);
            ViewData["horizontalBarChartRazlikaLetnihStroskov"] = horizontalBarChartRazlikaLetnihStroskov;

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
            foreach (double x in data[0])
            {
                labels.Add(u + "");
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
            double sumElektrika = cenaElAvta + cenaElektrarne;
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

            listAvti.Add(new SelectListItem { Text = "Lightyear 0 Cena: 297.000", Value = "297000;109" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 6 Standard Range 2WD Cena: 43.900", Value = "43900;150" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model 3 Cena: 52.965", Value = "52965;151" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model 3 Long Range Dual Motor Cena: 62.465", Value = "62465;155" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 6 Long Range 2WD Cena: 54.000", Value = "54000;156" });
            listAvti.Add(new SelectListItem { Text = "XPENG P5 Cena: 48.000", Value = "48000;156" });
            listAvti.Add(new SelectListItem { Text = "Hyundai Kona Electric 39 kWh Cena: 36.400", Value = "36400;157" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Pure Cena: 100.000", Value = "100000;157" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Hatchback 24 kWh Cena: 30.990", Value = "30990;158" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e 3+1 24 kWh Cena: 32.290", Value = "32290;158" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Cabrio 24 kWh Cena: 35.490", Value = "35490;158" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-208 Cena: 35.350", Value = "35350;158" });
            listAvti.Add(new SelectListItem { Text = "Opel Corsa-e Cena: 33.895", Value = "33895;158" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Hatchback 42 kWh Cena: 34.990", Value = "34990;159" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e 3+1 42 kWh Cena: 36.990", Value = "36990;159" });
            listAvti.Add(new SelectListItem { Text = "Renault Megane E-Tech EV40 130hp Cena: 42.000", Value = "42000;160" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Touring Cena: 120.000", Value = "120000;160" });
            listAvti.Add(new SelectListItem { Text = "Mini Cooper SE Cena: 35.700", Value = "35700;161" });
            listAvti.Add(new SelectListItem { Text = "Dacia Spring Electric Cena: 22.550", Value = "22550;161" });
            listAvti.Add(new SelectListItem { Text = "Hyundai Kona Electric 64 kWh Cena: 42.900", Value = "42900;162" });
            listAvti.Add(new SelectListItem { Text = "Fiat 500e Cabrio 42 kWh Cena: 37.990", Value = "37990;162" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model 3 Performance Cena: 66.465", Value = "66465;163" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 110 kW - 45 kWh Cena: 32.700", Value = "32700;164" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pure Performance Cena: 37.990", Value = "37990;164" });
            listAvti.Add(new SelectListItem { Text = "Renault Twingo Electric Cena: 28.000", Value = "28000;164" });
            listAvti.Add(new SelectListItem { Text = "Renault Megane E-Tech EV60 130hp Cena: 47.000", Value = "47000;164" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-308 Cena: 44.000", Value = "44000;165" });
            listAvti.Add(new SelectListItem { Text = "Renault Zoe ZE50 R110 Cena: 36.840", Value = "36840;165" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 150 kW - 58 kWh Cena: 39.370", Value = "39370;166" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro Cena: 35.460", Value = "35460;166" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro Performance Cena: 38.060", Value = "38060;166" });
            listAvti.Add(new SelectListItem { Text = "Abarth 500e Scorpionissima Cena: 42.000", Value = "42000;166" });
            listAvti.Add(new SelectListItem { Text = "Nissan Leaf Cena: 33.400", Value = "33400;166" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model S Dual Motor Cena: 105.000", Value = "105000;167" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model Y Cena: 56.965", Value = "56965;167" });
            listAvti.Add(new SelectListItem { Text = "Renault Megane E-Tech EV60 220hp Cena: 46.600", Value = "46600;167" });
            listAvti.Add(new SelectListItem { Text = "Smart EQ fortwo coupe Cena: 21.940", Value = "21940;167" });
            listAvti.Add(new SelectListItem { Text = "BMW i4 eDrive35 Cena: 56.500", Value = "56500;168" });
            listAvti.Add(new SelectListItem { Text = "Honda e Advance Cena: 39.900", Value = "39900;168" });
            listAvti.Add(new SelectListItem { Text = "Renault Zoe ZE50 R135 Cena: 37.840", Value = "37840;168" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 170 kW - 58 kWh Cena: 40.750", Value = "40750;168" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 6 Long Range AWD Cena: 61.100", Value = "61100;168" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Grand Touring Cena: 175.000", Value = "175000;168" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 450+ Cena: 109.551", Value = "109551;168" });
            listAvti.Add(new SelectListItem { Text = "MG MG4 Electric 64 kWh Cena: 35.990", Value = "35990;169" });
            listAvti.Add(new SelectListItem { Text = "MG MG4 Electric 51 kWh Cena: 31.990", Value = "31990;169" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Dream Edition R Cena: 218.000", Value = "218000;170" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-C4 Cena: 36.040", Value = "36040;170" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-C4 X Cena: 38.000", Value = "38000;170" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-308 SW Cena: 45.000", Value = "45000;170" });
            listAvti.Add(new SelectListItem { Text = "Kia e-Soul 39.2 kWh Cena: 40.290", Value = "40290;170" });
            listAvti.Add(new SelectListItem { Text = "Kia Niro EV Cena: 47.590", Value = "47590;171" });
            listAvti.Add(new SelectListItem { Text = "CUPRA Born 170 kW - 77 kWh Cena: 46.450", Value = "46450;171" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro S - 4 Seats Cena: 43.560", Value = "43560;171" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.3 Pro S - 5 Seats Cena: 43.720", Value = "43720;171" });
            listAvti.Add(new SelectListItem { Text = "Aiways U6 Cena: 47.588", Value = "47588;171" });
            listAvti.Add(new SelectListItem { Text = "BMW i4 eDrive40 Cena: 59.200", Value = "59200;172" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model Y Long Range Dual Motor Cena: 59.965", Value = "59965;172" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 350+ Cena: 70.626", Value = "70626;173" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 300 Cena: 66.402", Value = "66402;173" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 350 Cena: 70.210", Value = "70210;173" });
            listAvti.Add(new SelectListItem { Text = "Kia e-Soul 64 kWh Cena: 46.950", Value = "46950;173" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV 80 Cena: 47.000", Value = "47000;173" });
            listAvti.Add(new SelectListItem { Text = "DS 3 Crossback E-Tense Cena: 40.540", Value = "40540;173" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV 60 Cena: 42.000", Value = "42000;173" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Cena: 88.399", Value = "88399;173" });
            listAvti.Add(new SelectListItem { Text = "JAC iEV7s Cena: 29.900", Value = "29900;173" });
            listAvti.Add(new SelectListItem { Text = "Nissan Leaf e+ Cena: 41.100", Value = "41100;174" });
            listAvti.Add(new SelectListItem { Text = "ORA Funky Cat First Edition Cena: 36.971", Value = "36971;174" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 450 4MATIC Cena: 113.359", Value = "113359;175" });
            listAvti.Add(new SelectListItem { Text = "MG MG5 Electric Long Range Cena: 38.490", Value = "38490;175" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV 60 Cena: 42.100", Value = "42100;176" });
            listAvti.Add(new SelectListItem { Text = "Smart EQ fortwo cabrio Cena: 25.200", Value = "25200;176" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model S Plaid Cena: 138.970", Value = "138970;176" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Plus Cena: 94.123", Value = "94123;176" });
            listAvti.Add(new SelectListItem { Text = "NIO ET7 75 kWh Cena: 82.850", Value = "82850;176" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 35 Cena: 46.700", Value = "46700;176" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-2008 SUV Cena: 37.900", Value = "37900;176" });
            listAvti.Add(new SelectListItem { Text = "Opel Mokka-e Cena: 37.650", Value = "37650;176" });
            listAvti.Add(new SelectListItem { Text = "Mazda MX-30 Cena: 35.990", Value = "35990;176" });
            listAvti.Add(new SelectListItem { Text = "MG MG5 Electric Standard Range Cena: 35.490", Value = "35490;177" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 Standard Range 2WD Cena: 46.990", Value = "46990;177" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 500 4MATIC Cena: 125.378", Value = "125378;178" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS 580 4MATIC Cena: 141.705", Value = "141705;178" });
            listAvti.Add(new SelectListItem { Text = "NIO ET7 100 kWh Cena: 91.850", Value = "91850;178" });
            listAvti.Add(new SelectListItem { Text = "NIO ET5 75 kWh Cena: 62.850", Value = "62850;178" });
            listAvti.Add(new SelectListItem { Text = "BYD HAN Cena: 70.805", Value = "70805;178" });
            listAvti.Add(new SelectListItem { Text = "Polestar 2 Long Range Single Motor Cena: 50.995", Value = "50995;179" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.5 Pro Cena: 47.935", Value = "47935;179" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.5 Pro Performance Cena: 48.970", Value = "48970;179" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV 80x Cena: 49.000", Value = "49000;179" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Cena: 108.867", Value = "108867;180" });
            listAvti.Add(new SelectListItem { Text = "Lucid Air Dream Edition P Cena: 218.000", Value = "218000;180" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 40 Cena: 53.900", Value = "53900;180" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 Long Range 2WD Cena: 50.990", Value = "50990;180" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model Y Performance Cena: 68.465", Value = "68465;181" });
            listAvti.Add(new SelectListItem { Text = "Sono Sion Cena: 29.899", Value = "29899;181" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 350 4MATIC Cena: 73.363", Value = "73363;181" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE 500 4MATIC Cena: 87.286", Value = "87286;181" });
            listAvti.Add(new SelectListItem { Text = "MG ZS EV Standard Range Cena: 33.990", Value = "33990;181" });
            listAvti.Add(new SelectListItem { Text = "NIO ET5 100 kWh Cena: 71.850", Value = "71850;182" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Plus Cena: 114.389", Value = "114389;182" });
            listAvti.Add(new SelectListItem { Text = "BMW iX1 xDrive30 Cena: 55.000", Value = "55000;182" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pure Performance Cena: 38.450", Value = "38450;182" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 35 Cena: 44.700", Value = "44700;182" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pure Cena: 36.950", Value = "36950;182" });
            listAvti.Add(new SelectListItem { Text = "Smart #1 Cena: 41.490", Value = "41490;183" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 5 Standard Range 2WD Cena: 43.900", Value = "43900;183" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV 80 Cena: 46.800", Value = "46800;183" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 Long Range AWD Cena: 54.980", Value = "54980;185" });
            listAvti.Add(new SelectListItem { Text = "BMW i4 M50 Cena: 70.800", Value = "70800;186" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan GTS Cena: 134.214", Value = "134214;186" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE AMG 43 4MATIC Cena: 103.827", Value = "103827;187" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Sport Turismo Cena: 89.351", Value = "89351;187" });
            listAvti.Add(new SelectListItem { Text = "MG ZS EV Long Range Cena: 37.990", Value = "37990;187" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 250 Cena: 50.777", Value = "50777;187" });
            listAvti.Add(new SelectListItem { Text = "Genesis G80 Electrified Luxury Cena: 69.200", Value = "69200;188" });
            listAvti.Add(new SelectListItem { Text = "BYD ATTO 3 Cena: 44.625", Value = "44625;188" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pro Performance Cena: 46.335", Value = "46335;188" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pro Cena: 48.190", Value = "48190;188" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 250+ Cena: 52.205", Value = "52205;188" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya 63kWh Cena: 47.490", Value = "47490;188" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Plus Sport Turismo Cena: 95.074", Value = "95074;188" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE AMG 53 4MATIC+ Cena: 109.777", Value = "109777;189" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 40 Cena: 51.900", Value = "51900;189" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV60 Premium Cena: 54,342", Value = "54,342;190" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 5 Long Range 2WD Cena: 47.900", Value = "47900;190" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 300 4MATIC Cena: 53.746", Value = "53746;190" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQA 350 4MATIC Cena: 56.424", Value = "56424;190" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.5 GTX Cena: 56.455", Value = "56455;190" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq Coupe iV RS Cena: 61.960", Value = "61960;190" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 Pro 4MOTION Cena: 49.020", Value = "49020;190" });
            listAvti.Add(new SelectListItem { Text = "Toyota bZ4X FWD Cena: 47.490", Value = "47490;190" });
            listAvti.Add(new SelectListItem { Text = "Aiways U5 Cena: 39.563", Value = "39563;190" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS AMG 53 4MATIC+ Cena: 155.009", Value = "155009;191" });
            listAvti.Add(new SelectListItem { Text = "Smart #1 Brabus Cena: 48.990", Value = "48990;191" });
            listAvti.Add(new SelectListItem { Text = "Lexus UX 300e Cena: 47.550", Value = "47550;191" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 45 quattro Cena: 55.900", Value = "55900;192" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 Sportback e-tron 50 quattro Cena: 59.900", Value = "59900;192" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Sport Turismo Cena: 109.819", Value = "109819;192" });
            listAvti.Add(new SelectListItem { Text = "Hyundai IONIQ 5 Long Range AWD Cena: 59.200", Value = "59200;192" });
            listAvti.Add(new SelectListItem { Text = "BMW iX3 Cena: 67.300", Value = "67300;192" });
            listAvti.Add(new SelectListItem { Text = "Polestar 2 Long Range Dual Motor Cena: 54.995", Value = "54995;192" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV 80x Cena: 49.050", Value = "49050;193" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID.4 GTX Cena: 53.255", Value = "53255;193" });
            listAvti.Add(new SelectListItem { Text = "Seres 3 Cena: 37.995", Value = "37995;193" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 250 Cena: 52.550", Value = "52550;193" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV 350+ Cena: 85.000", Value = "85000;193" });
            listAvti.Add(new SelectListItem { Text = "Toyota bZ4X AWD Cena: 57.390", Value = "57390;193" });
            listAvti.Add(new SelectListItem { Text = "SsangYong Korando e-Motion Cena: 40.490", Value = "40490;193" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 250+ Cena: 53.978", Value = "53978;193" });
            listAvti.Add(new SelectListItem { Text = "MG Marvel R Cena: 46.990", Value = "46990;194" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Plus Sport Turismo Cena: 115.341", Value = "115341;195" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV60 Sport Cena: 56.370", Value = "56370;195" });
            listAvti.Add(new SelectListItem { Text = "Polestar 2 Long Range Performance Cena: 61.495", Value = "61495;195" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya 87kWh Cena: 63.490", Value = "63490;196" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean Extreme Cena: 69.950", Value = "69950;196" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean One Cena: 69.950", Value = "69950;196" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan GTS Sport Turismo Cena: 135.166", Value = "135166;197" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4S Cross Turismo Cena: 114.222", Value = "114222;197" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan 4 Cross Turismo Cena: 98.514", Value = "98514;197" });
            listAvti.Add(new SelectListItem { Text = "MG Marvel R Performance Cena: 50.990", Value = "50990;197" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E SR RWD Cena: 62.900", Value = "62900;197" });
            listAvti.Add(new SelectListItem { Text = "BMW iX xDrive40 Cena: 77.300", Value = "77300;197" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean Sport Cena: 41.560", Value = "41560;197" });
            listAvti.Add(new SelectListItem { Text = "Fisker Ocean Ultra Cena: 57.000", Value = "57000;198" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 300 4MATIC Cena: 55.519", Value = "55519;199" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQB 350 4MATIC Cena: 58.197", Value = "58197;199" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 50 quattro Cena: 57.900", Value = "57900;199" });
            listAvti.Add(new SelectListItem { Text = "Audi Q4 e-tron 45 quattro Cena: 53.990", Value = "53990;199" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV 350 4MATIC Cena: 90.000", Value = "90000;199" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV 500 4MATIC Cena: 115.000", Value = "115000;199" });
            listAvti.Add(new SelectListItem { Text = "BMW i7 xDrive60 Cena: 135.900", Value = "135900;199" });
            listAvti.Add(new SelectListItem { Text = "Skoda Enyaq iV RS Cena: 60.000", Value = "60000;200" });
            listAvti.Add(new SelectListItem { Text = "Volvo XC40 Recharge Pure Electric Cena: 45.080", Value = "45080;200" });
            listAvti.Add(new SelectListItem { Text = "Kia EV6 GT Cena: 69.990", Value = "69990;200" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E ER RWD Cena: 71.200", Value = "71200;200" });
            listAvti.Add(new SelectListItem { Text = "Subaru Solterra AWD Cena: 57.490", Value = "57490;201" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo Cena: 156.348", Value = "156348;202" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron Sportback 55 quattro Cena: 87.550", Value = "87550;202" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model X Dual Motor Cena: 110.000", Value = "110000;202" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron Sportback 50 quattro Cena: 76.650", Value = "76650;202" });
            listAvti.Add(new SelectListItem { Text = "Audi e-tron GT quattro Cena: 106.050", Value = "106050;202" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV60 Sport Plus Cena: 71.010", Value = "71010;203" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E SR AWD Cena: 69.200", Value = "69200;203" });
            listAvti.Add(new SelectListItem { Text = "Volvo C40 Recharge Pure Electric Cena: 48.850", Value = "48850;203" });
            listAvti.Add(new SelectListItem { Text = "Lexus RZ 450e Cena: 60.000", Value = "60000;204" });
            listAvti.Add(new SelectListItem { Text = "Lotus Eletre Cena: 95.990", Value = "95990;206" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo Sport Turismo Cena: 157.300", Value = "157300;207" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E ER AWD Cena: 77.200", Value = "77200;207" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya e-4ORCE 87kWh Cena: 66.490", Value = "66490;207" });
            listAvti.Add(new SelectListItem { Text = "NIO EL7 75 kWh Cena: 86.850", Value = "86850;207" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV AMG 43 4MATIC Cena: 120.000", Value = "120000;208" });
            listAvti.Add(new SelectListItem { Text = "BMW iX xDrive50 Cena: 100.100", Value = "100100;208" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo S Cena: 189.668", Value = "189668;209" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo Cross Turismo Cena: 157.776", Value = "157776;209" });
            listAvti.Add(new SelectListItem { Text = "NIO EL7 100 kWh Cena: 95.850", Value = "95850;209" });
            listAvti.Add(new SelectListItem { Text = "Audi e-tron GT RS Cena: 146.050", Value = "146050;210" });
            listAvti.Add(new SelectListItem { Text = "Tesla Model X Plaid Cena: 141.970", Value = "141970;211" });
            listAvti.Add(new SelectListItem { Text = "Genesis GV70 Electrified Sport Cena: 67.300", Value = "67300;211" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo S Sport Turismo Cena: 190.620", Value = "190620;212" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQE SUV AMG 53 4MATIC+ Cena: 135.000", Value = "135000;213" });
            listAvti.Add(new SelectListItem { Text = "Ford Mustang Mach-E GT Cena: 86.200", Value = "86200;214" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron 55 quattro Cena: 85.300", Value = "85300;214" });
            listAvti.Add(new SelectListItem { Text = "Volvo C40 Recharge Twin Pure Electric Cena: 57.650", Value = "57650;214" });
            listAvti.Add(new SelectListItem { Text = "Audi Q8 e-tron 50 quattro Cena: 74.400", Value = "74400;214" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQC 400 4MATIC Cena: 66.069", Value = "66069;216" });
            listAvti.Add(new SelectListItem { Text = "BMW iX M60 Cena: 136.100", Value = "136100;217" });
            listAvti.Add(new SelectListItem { Text = "Porsche Taycan Turbo S Cross Turismo Cena: 191.096", Value = "191096;217" });
            listAvti.Add(new SelectListItem { Text = "Polestar 3 Long Range Dual motor Cena: 88.600", Value = "88600;218" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS SUV 450+ Cena: 110.658", Value = "110658;219" });
            listAvti.Add(new SelectListItem { Text = "Toyota Proace City Verso Electric L1 50 kWh Cena: 37.800", Value = "37800;220" });
            listAvti.Add(new SelectListItem { Text = "Rolls-Royce Spectre Cena: 400.000", Value = "400000;220" });
            listAvti.Add(new SelectListItem { Text = "Volvo XC40 Recharge Twin Pure Electric Cena: 56.150", Value = "56150;221" });
            listAvti.Add(new SelectListItem { Text = "Jaguar I-Pace EV400 Cena: 76.815", Value = "76815;223" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS SUV 450 4MATIC Cena: 114.446", Value = "114446;224" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQS SUV 580 4MATIC Cena: 135.291", Value = "135291;224" });
            listAvti.Add(new SelectListItem { Text = "Opel Combo-e Life 50 kWh Cena: 43.050", Value = "43050;225" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Berlingo M 50 kWh Cena: 37.790", Value = "37790;225" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Rifter Standard 50 kWh Cena: 41.240", Value = "41240;225" });
            listAvti.Add(new SelectListItem { Text = "Toyota Proace City Verso Electric L2 50 kWh Cena: 40.150", Value = "40150;225" });
            listAvti.Add(new SelectListItem { Text = "Audi SQ8 e-tron Sportback Cena: 115.000", Value = "115000;226" });
            listAvti.Add(new SelectListItem { Text = "Nissan Ariya e-4ORCE 87kWh Performance Cena: 66.000", Value = "66000;226" });
            listAvti.Add(new SelectListItem { Text = "Polestar 3 Long Range Performance Cena: 95.200", Value = "95200;228" });
            listAvti.Add(new SelectListItem { Text = "Opel Combo-e Life XL 50 kWh Cena: 44.750", Value = "44750;231" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Berlingo XL 50 kWh Cena: 43.640", Value = "43640;231" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Rifter Long 50 kWh Cena: 42.440", Value = "42440;231" });
            listAvti.Add(new SelectListItem { Text = "Volkswagen ID. Buzz Pro Cena: 64.581", Value = "64581;233" });
            listAvti.Add(new SelectListItem { Text = "Volvo EX90 Twin Motor Cena: 105.550", Value = "105550;235" });
            listAvti.Add(new SelectListItem { Text = "Lotus Eletre R Cena: 150.990", Value = "150990;238" });
            listAvti.Add(new SelectListItem { Text = "Audi SQ8 e-tron Cena: 110.000", Value = "110000;238" });
            listAvti.Add(new SelectListItem { Text = "BYD TANG Cena: 71.400", Value = "71400;240" });
            listAvti.Add(new SelectListItem { Text = "Volvo EX90 Twin Motor Performance Cena: 110.650", Value = "110650;243" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi M 50 kWh Cena: 51.940", Value = "51940;250" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer M 50 kWh Cena: 53.640", Value = "53640;250" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi M 50 kWh Cena: 50.992", Value = "50992;250" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life M 50 kWh Cena: 63.250", Value = "63250;250" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Standard 50 kWh Cena: 51.940", Value = "51940;250" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Standard 50 kWh Cena: 53.640", Value = "53640;250" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L2 50 kWh Cena: 55.990", Value = "55990;250" });
            listAvti.Add(new SelectListItem { Text = "Hongqi E-HS9 84 kWh Cena: 83.590", Value = "83590;251" });
            listAvti.Add(new SelectListItem { Text = "Hongqi E-HS9 99 kWh Cena: 94.590", Value = "94590;254" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi M 75 kWh Cena: 57.940", Value = "57940;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer M 75 kWh Cena: 59.640", Value = "59640;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi M 75 kWh Cena: 56.942", Value = "56942;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life M 75 kWh Cena: 69.250", Value = "69250;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Standard 75 kWh Cena: 57.940", Value = "57940;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Standard 75 kWh Cena: 59.640", Value = "59640;257" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L2 75 kWh Cena: 61.990", Value = "61990;257" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Verso M 75 kWh Cena: 64.530", Value = "64530;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi XL 50 kWh Cena: 52.730", Value = "52730;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer XL 50 kWh Cena: 54.430", Value = "54430;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi L 50 kWh Cena: 51.825", Value = "51825;257" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life L 50 kWh Cena: 64.075", Value = "64075;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Long 50 kWh Cena: 52.730", Value = "52730;257" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Long 50 kWh Cena: 54.430", Value = "54430;257" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle M 50 kWh Cena: 58.845", Value = "58845;257" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle L 50 kWh Cena: 68.545", Value = "68545;257" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L3 50 kWh Cena: 56.990", Value = "56990;257" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-Jumpy Combi XL 75 kWh Cena: 58.730", Value = "58730;262" });
            listAvti.Add(new SelectListItem { Text = "Citroen e-SpaceTourer XL 75 kWh Cena: 60.430", Value = "60430;262" });
            listAvti.Add(new SelectListItem { Text = "Opel Vivaro-e Combi L 75 kWh Cena: 57.775", Value = "57775;262" });
            listAvti.Add(new SelectListItem { Text = "Opel Zafira-e Life L 75 kWh Cena: 70.075", Value = "70075;262" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Expert Combi Long 75 kWh Cena: 58.730", Value = "58730;262" });
            listAvti.Add(new SelectListItem { Text = "Peugeot e-Traveller Long 75 kWh Cena: 60.430", Value = "60430;262" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle M 75 kWh Cena: 60.045", Value = "60045;262" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Shuttle L 75 kWh Cena: 69.745", Value = "69745;262" });
            listAvti.Add(new SelectListItem { Text = "Fiat E-Ulysse L3 75 kWh Cena: 62.990", Value = "62990;262" });
            listAvti.Add(new SelectListItem { Text = "Toyota PROACE Verso L 75 kWh Cena: 65.385", Value = "65385;262" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Long 60 kWh Cena: 60.678", Value = "60678;286" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Long 90 kWh Cena: 64.248", Value = "64248;290" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 250 Long Cena: 68.056", Value = "68056;293" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 250 Extra-Long Cena: 68.949", Value = "68949;293" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Extra-Long 60 kWh Cena: 61.571", Value = "61571;293" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 300 Extra-Long Cena: 72.519", Value = "72519;295" });
            listAvti.Add(new SelectListItem { Text = "Mercedes eVito Tourer Extra-Long 90 kWh Cena: 65.140", Value = "65140;295" });
            listAvti.Add(new SelectListItem { Text = "Mercedes EQV 300 Long Cena: 71.626", Value = "71626;295" });


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