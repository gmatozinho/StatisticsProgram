using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Xamarin.Forms;

namespace StatisticsProgram
{
    public class MainPage : ContentPage
    {
        public MainPage()
        {

            var assembly = typeof(App).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("StatisticsProgram.teste.csv");

            string[] test;
            using (var reader = new System.IO.StreamReader(stream))
            {
                test = reader.ReadToEnd().Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            }

            var untreatyears = test[0];
            var data = test[1];

            var values = data.Split(new[] {";"}, StringSplitOptions.None).ToList();
            values.Remove("Brasil");

            Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = {
                    new Label { Text = "Valores Estatisticos:" },
                    new Label { Text = "Média:" + CalculateMedia(values)},
                    new Label { Text = "Moda:" + CalculateModa(values)},
                    new Label { Text = "Mediana:" + CalculateMediana(values)},
                    new Label { Text = data },
                }
            };
        }

        private static string CalculateMediana(List<string> values)
        {
            var list = values.Select(double.Parse).ToList();

            string mediana;

            if (list.Count % 2 == 0)
            {
                var medMe = list.Count / 2;
                var medMa = (list.Count + 2) / 2;

                // ReSharper disable once PossibleLossOfFraction
                
                mediana = " Elemento = " + ((list[medMa-1] + list[medMe-1])/2);

            }
            else
            {
                // ReSharper disable once PossibleLossOfFraction
                mediana = "Elemento = " + list[((list.Count + 1) / 2)-1];
            }

            return mediana;
        }

        private static double CalculateMedia(IReadOnlyCollection<string> values)
        {
            var soma = values.Sum(value => double.Parse(value));

            return soma / values.Count;
        }

        private static string CalculateModa(IEnumerable<string> values)
        {
            var list = values.Select(double.Parse).ToList();

            string moda;
            
            var groups = list.GroupBy(i => i).Select(i => new { Number = i.Key, Count = i.Count() }).OrderByDescending(i => i.Count);

            var dict = new Dictionary<double, int>();
            foreach (var d1 in list)
            {
                var d = d1;
                if (dict.ContainsKey(d))
                    dict[d]++;
                else
                    dict.Add(d, 1);
            }

            if (dict.Count == list.Count)
            {
                moda = "Amodal";
            }
            else
            {
                var modas = (from data in dict where data.Value == groups.First().Count select data.Key).ToList();

                switch (modas.Count)
                {
                    case 1:
                        moda = "Unimodal-" + modas[0];
                        break;
                    case 2:
                        moda = modas.Aggregate("Bimodal-", (current, m) => current + (m + ";"));
                        break;
                    default:
                        moda = modas.Aggregate("Multimodal-", (current, m) => current + (m + ";"));
                        break;
                }
            }

            return moda;
        }
    }
}