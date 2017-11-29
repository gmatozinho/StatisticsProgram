using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Steema.TeeChart;
using Xamarin.Forms;

namespace StatisticsProgram
{
    public class MainPage : ContentPage
    {
        public MainPage()
        {

            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("StatisticsProgram.mediadashorasefetivamentetrabalhadas.csv");

            string[] test;
            using (var reader = new StreamReader(stream))
            {
                test = reader.ReadToEnd().Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            }

            var data = test[1];
            
            var forRemove = new List<string> {"Total das areas"};

            var originalList = data.Split(new[] {";"}, StringSplitOptions.None).ToList();
            originalList.RemoveAll(t => forRemove.Contains(t));

            var doubleList = ConvertToListDouble(originalList);
            doubleList.Sort();

            var classNumber = ClassNumberCalculator(doubleList.Count());
            var amplitude = ClassAmplitudeCalculator(classNumber, doubleList[doubleList.Count - 1], doubleList[0]);

            var listClass = CreateClasses(amplitude, classNumber, doubleList);
            var average = CalculateAverage(listClass);
            var variance = CalculateSampleVariance(listClass,average);

            PrepareForBoxSplot(doubleList);

            var button = new Button(){Text = "Mostrar Informações do Box Splot"};

            button.Clicked += Button_Clicked;

            Content =  new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = {
                    new Label { Text = "Valores Estatisticos:\n" },
                    new Label { Text = "Média: " + average},
                    new Label { Text = "Moda:" + CalculateCzuberMode(listClass,amplitude)},
                    new Label { Text = "Mediana:" + CalculateMedian(listClass,amplitude)},
                    new Label {Text = "Variância:" + variance },
                    new Label {Text = "Desvio Padrão:" + CalculateSampleStandardDeviation(variance )},
                    new Label {Text = "Classes:" + classNumber},
                    new Label {Text = "Amplitude:" + amplitude +"\n"},
                    button
                }
            };
        }

        private static List<double> ConvertToListDouble(List<string> values)
        {
            var doubleList = new List<double>();

            foreach (var value in values)
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.Android:
                        doubleList.Add(double.Parse(value));
                        break;
                    case Device.Windows:
                        doubleList.Add(double.Parse(value.Replace(",", ".")));
                        break;

                }
                
            }

            return doubleList;
        }

        private static double CalculateAverage(IEnumerable<StatisticClass> values)
        {

            double total = 0;
            double sumFrequencyXMidPoint = 0;

            foreach (var value in values)
            {
                sumFrequencyXMidPoint += value.AbsolutFrequency * value.MidPoint;
                total += value.AbsolutFrequency;
            }

            return Math.Round(sumFrequencyXMidPoint / total, 3);
        }

        private static double CalculateCzuberMode(List<StatisticClass> values, double amplitude)
        {

            var listAux = values.Select(value => value.AbsolutFrequency).ToList();
            listAux.Sort();
            var mode = listAux.Last();
            var modeClass = values.Select(modeC => modeC).First(modeC => modeC.AbsolutFrequency == mode);

            var modeClassPos = values.FindIndex(a => a == modeClass);
            var lowerLimitMiddleClass = modeClass.Begin;
            var absoluteFrequencyMiddleClass = modeClass.AbsolutFrequency;

            var absoluteFrequencyPreviousMiddleClass = PreviousFrequencyMode(values, modeClassPos);
            var absoluteFrequencyAfterMiddleClass = AfterFrequencyMode(values, modeClassPos);
            
            var czuberMode = lowerLimitMiddleClass +
                             // ReSharper disable once PossibleLossOfFraction
                             (amplitude*((absoluteFrequencyMiddleClass - absoluteFrequencyPreviousMiddleClass) /
                                        ((2 * absoluteFrequencyMiddleClass) -
                                         (absoluteFrequencyPreviousMiddleClass + absoluteFrequencyAfterMiddleClass))));

            return czuberMode;

        }


        private static int PreviousFrequencyMode(List<StatisticClass> values, int modeClassPos)
        {
            try
            {
                return values[modeClassPos - 1].AbsolutFrequency;
            }
            catch (Exception e)
            {
                return  0;
            }
        }

        private static int AfterFrequencyMode(List<StatisticClass> values, int modeClassPos)
        {
            try
            {
                return values[modeClassPos + 1].AbsolutFrequency;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private static double CalculateMedian(List<StatisticClass> values, double amplitude)
        {
            double medianPos = values.Count/2;

            var medianClass = new StatisticClass();

            foreach (var value in values)
            {
                if (!(medianPos < value.AccumulatedFrequency)) continue;
                medianClass = value;
                break;
            }

            var medianClassPos = values.FindIndex(a => a == medianClass);
            var previousMedianClassFrequency = PreviousFrequencyMedian(values, medianClassPos);
            var divisionTop = medianPos - previousMedianClassFrequency;
            var lowerLimitMiddleClass = medianClass.Begin;
            var absoluteFrequencyMiddleClass = medianClass.AbsolutFrequency;

            var median = lowerLimitMiddleClass + ((divisionTop / absoluteFrequencyMiddleClass) * amplitude);

            return median;
        }

        private static int PreviousFrequencyMedian(List<StatisticClass > values, int medianClassPos)
        {
            try
            {
                return values[medianClassPos - 1].AbsolutFrequency;
            }
            catch (Exception e)
            {
                return 0;
            }

        }

        private static double CalculateSampleVariance(List<StatisticClass> values, double average)
        {
            var sum = values.Sum(value => Math.Pow(value.MidPoint - average, 2));

            var total = (values.Last().AccumulatedFrequency - 1);
            var variance = sum / total ;

            return Math.Round(variance,3);
        }

        private static double CalculateSampleStandardDeviation(double variance)
        {
            return Math.Sqrt(variance);
        }

        private static int ClassNumberCalculator(int qtdNumbers)
        {
            var classNumber = (int)Math.Round(Math.Sqrt(qtdNumbers));

            return classNumber;
        }


        private static double ClassAmplitudeCalculator(int classNumber, double maxValue, double minValue)
        {
            var classAmplitude = (maxValue - minValue) / classNumber;

            return Math.Round(classAmplitude,3);
        }


        private static double ClassMidPointCalculator(double upperLimit, double lowerLimit)
        {
            var midPoint = (upperLimit + lowerLimit) / 2;

            return midPoint;
        }

        private static List<StatisticClass> CreateClasses(double classAmplitude,int classNumber, List<double> allElements)
        {
            var list = new List<StatisticClass>();
            var classVariable = Math.Round(allElements[0],3);
            var accumulatedFrequency = 0;

            for(var i=0;i<classNumber;i++)
            {
                var myClass = new StatisticClass() { Begin = Math.Round(classVariable,3), End = Math.Round(classVariable += classAmplitude,3) };
                myClass.DefineName();
                foreach (var item in allElements)
                {
                    if (item >= myClass.Begin && item < myClass.End)
                    {
                        myClass.Elements.Add(item);
                    }
                }
                myClass.AbsolutFrequency = myClass.Elements.Count();
                myClass.AccumulatedFrequency = accumulatedFrequency += myClass.AbsolutFrequency;
                myClass.MidPoint = ClassMidPointCalculator(myClass.End, myClass.Begin);
                list.Add(myClass);
            }


            return list;
        }


        private void BoxPlot()
        {

            
        }


        private void Button_Clicked(object sender, EventArgs e)
        {
            var stack = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = "Valores BoxPlot:\n" },
                    new Label { Text = "Q2: " + mediana },
                    new Label { Text = "Q1: " + q1 },
                    new Label { Text = "Q3: " + q3 },
                    new Label { Text = "Amplitude interquantil: " + AIQ },
                    new Label { Text = "Valor atipico inferior: " + xinf },
                    new Label { Text = "Valor atipico superior: " + xsup },
                    new Label { Text = "Valor Minimo:" + valorMin },
                    new Label { Text = "Valor Maximo:" + valorMax},
                    
                }
            };

            Navigation.PushAsync(new ContentPage() { Content = stack });
        }

        private double mediana;
        private double q1;
        private double q3;
        private double AIQ;
        private double xinf;
        private double xsup;
        private double valorMin;
        private double valorMax;

        private void PrepareForBoxSplot(List<double> values)
        {
            mediana = OldCalculateMediana(values);
            var result = Q1AndQ2(values,mediana);
            q1 = result[0];
            q3 = result[1];
            AIQ = q3 - q1;
            xinf = Math.Round(q1 - (1.5 * AIQ),3);
            xsup = Math.Round(q3 + (1.5 * AIQ),3);
            valorMin = values[0];
            valorMax = values.Last();
        }


        private double[] Q1AndQ2(List<double> values,double mediana)
        {
            var listQ1 = (from data in values where data <= mediana select data).ToList();
            var listQ2 = (from data in values where data > mediana select data).ToList();

            var q1 = OldCalculateMediana(listQ1);
            var q3 = OldCalculateMediana(listQ2);


            return new[]{q1, q3};
        }


        private static double OldCalculateMediana(IReadOnlyList<double> values)
        {

            double mediana;

            if (values.Count % 2 == 0)
            {
                var medMe = values.Count / 2;
                var medMa = (values.Count + 2) / 2;

                mediana = ((values[medMa - 1] + values[medMe - 1]) / 2);

            }
            else
            {
                mediana = values[((values.Count + 1) / 2) - 1];
            }

            return mediana;
        }



        private static double OldCalculateMedia(IReadOnlyCollection<double> values)
        {
            double soma = 0;

            foreach (var value in values)
            {
                soma += value;
            }


            return soma / values.Count;
        }

        private static string OldCalculateModa(IReadOnlyCollection<double> values)
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
    }

}