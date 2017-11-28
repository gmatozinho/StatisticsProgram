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
            Stream stream = assembly.GetManifestResourceStream("StatisticsProgram.mediadashorasefetivamentetrabalhadas.csv");

            string[] test;
            using (var reader = new StreamReader(stream))
            {
                test = reader.ReadToEnd().Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            }

            string data = test[1];

            /*List<string> forRemove = new List<string> {"","Ate 1/2 salario minimo", "Mais de 1/2 a 1 salario minimo", "Mais de 1 a 2 salarios minimos", "Mais de 2 a 3 salarios minimos", "Mais de 3 a 5 salarios minimos",

                "Brasil", "OPCAO", "1992", "1993", "1995", "1996", "1997", "1998", "1999", "2001", "2002", "2003", "2004", "2005", "2006", "2007", "2008", "2009" };*/

            List<string> forRemove = new List<string> {"Total das areas"};

            var originalList = data.Split(new[] {";"}, StringSplitOptions.None).ToList();
            originalList.RemoveAll(t => forRemove.Contains(t));

            var doubleList = ConvertToListDouble(originalList);
            doubleList.Sort();

            var classNumber = ClassNumberCalculator(doubleList.Count());
            var amplitude = ClassAmplitudeCalculator(classNumber, doubleList[doubleList.Count - 1], doubleList[0]);

            var listClass = CreateClasses(amplitude, classNumber, doubleList);

            Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = {
                    new Label { Text = "Valores Estatisticos:" },
                    new Label { Text = "Média:" + CalculateMedia(doubleList)},
                    new Label { Text = "Moda:" + CalculateModa(doubleList)},
                    new Label { Text = "Mediana:" + CalculateMediana(doubleList)},
                    new Label { Text = originalList.ToString() },
                    new Label {Text = "Classes:" + classNumber},
                    new Label {Text = "Amplitude:" + amplitude},
                    new Label {Text = "Ponto médio:" + ClassMidPointCalculator(5,10)}
                }
            };
        }


        private static List<double> ConvertToListDouble(List<string> values)
        {
            var doubleList = new List<double>();

            foreach (var value in values)
            {
                doubleList.Add(double.Parse(value.Replace(",", ".")));
            }

            return doubleList;
        }

        private static string CalculateMediana(List<double> values)
        {             

            string mediana;

            if (values.Count % 2 == 0)
            {
                var medMe = values.Count / 2;
                var medMa = (values.Count + 2) / 2;
                                
                mediana = " Elemento = " + ((values[medMa-1] + values[medMe-1])/2);

            }
            else
            {
                mediana = "Elemento = " + values[((values.Count + 1) / 2)-1];
            }

            return mediana;
        }

        private static double CalculateMedia(List<double> values)
        {
            double soma = 0;

            foreach(var value in values)
            {
                soma += value;
            }


            return soma / values.Count;
        }

        private static string CalculateModa(List<double> values)
        {
                        string moda;
            
            var groups = values.GroupBy(i => i).Select(i => new { Number = i.Key, Count = i.Count() }).OrderByDescending(i => i.Count);

            var dict = new Dictionary<double, int>();
            foreach (var d1 in values)
            {
                var d = d1;
                if (dict.ContainsKey(d))
                    dict[d]++;
                else
                    dict.Add(d, 1);
            }

            if (dict.Count == values.Count)
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


        private int ClassNumberCalculator(int qtdNumbers)
        {
            var classNumber = (int)Math.Round(Math.Sqrt(qtdNumbers));

            return classNumber;
        }


        private double ClassAmplitudeCalculator(int classNumber, double maxValue, double minValue)
        {
            var classAmplitude = (maxValue - minValue) / classNumber;

            return Math.Round(classAmplitude,3);
        }


        private float ClassMidPointCalculator(float upperLimit, int lowerLimit)
        {
            var midPoint = (upperLimit + lowerLimit) / 2;

            return midPoint;
        }

        private List<StatisticClass> CreateClasses(double classAmplitude,int classNumber, List<double> allElements)
        {
            var list = new List<StatisticClass>();
            var classVariable = allElements[0];

            for(var i=0;i<classNumber;i++)
            {
                var myClass = new StatisticClass() { Begin = classVariable, End = classVariable += classAmplitude };
                myClass.DefineName();
                classVariable += classAmplitude;
                list.Add(myClass);
            }


            return list;
        }
    }


}