using GenericParsing;
using Microsoft.Extensions.Configuration;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;

namespace IRV_Evaluator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

                var fileSettings = configuration.GetSection("FileSettings").Get<FileSettings>();
                var offices = configuration.GetSection("Offices").Get<IEnumerable<Office>>();
                using (var parser = new GenericParser(fileSettings.Path))
                {
                    parser.FirstRowHasHeader = fileSettings.FirstRowHasHeader;
                    parser.ColumnDelimiter = fileSettings.Delimiter;
                    parser.FirstRowSetsExpectedColumnCount = true;
                    parser.TrimResults = true;
                    while (parser.Read())
                    {
                        foreach (var office in offices)
                        {
                            office.ReadVote(parser);
                        }
                    }
                }
                foreach (var office in offices)
                {
                    var result = Evaluator.Evaluate(office);
                    GenerateImage(result);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"A fatal error occurred.{Environment.NewLine}{ex}");
            }
            Console.WriteLine("Evaluation complete");
            Console.ReadKey();
        }

        private static void GenerateImage(Result result)
        {
            var width = 1920;
            var height = 1080;
            var plot = new Plot(width, height);
            var color = "0A5A8D";
            var r = int.Parse(color[..2], NumberStyles.HexNumber);
            var g = int.Parse(color[2..4], NumberStyles.HexNumber);
            var b = int.Parse(color[^2..^0], NumberStyles.HexNumber);
            var finalRound = result.Rounds.Last();
            var values = finalRound.Percentages.Select(p => (double)p.Value.Item1).ToArray();
            var pie = plot.AddPie(values);
            pie.SliceFont.Name = "Roboto Medium";
            pie.SliceFont.Size = 18;
            var background = Color.FromArgb(r, g, b);
            //plot.Title(result.Office.Name);
            plot.Layout(width * 0.2f, width * 0.2f, 0f, height * 0.2f, height * 0.05f);
            plot.Style(background, background, titleLabel: Color.White);
            pie.SliceLabels = finalRound.Percentages.Select(p => $"{p.Key}{Environment.NewLine}{p.Value.Item1} - {p.Value.Item2:0.00} %").ToArray();

            pie.ShowLabels = true;
            plot.MatchLayout(plot);

            var bitmap = plot.Render();
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                using var brush = new SolidBrush(Color.White);
                using var font = new Font("Roboto Regular", 36, GraphicsUnit.Pixel);
                graphics.DrawString(result.Office.Name, font, brush, new PointF(10, 10));
                DrawFooter(width, height, plot, graphics);
            };
            bitmap.Save(result.Office.Name + ".png", ImageFormat.Png);

        }

        private static void DrawFooter(int width, int height, Plot plot, Graphics graphics)
        {
            using var brush = new SolidBrush(Color.White);
            using var font = new Font("Roboto Light", 13, GraphicsUnit.Pixel);
            var bottomText = $"Generated with IRV-Evaluator by drehtisch - {DateTime.UtcNow} UTC{Environment.NewLine}https://github.com/Free-Nations-Region/IRV-Evaluator";

            var size = graphics.MeasureString(bottomText, font);
            var position = new PointF((width - 10 - size.Width), (height - 10 - size.Height));
            graphics.DrawString(bottomText, font, brush, position);
        }
    }
}