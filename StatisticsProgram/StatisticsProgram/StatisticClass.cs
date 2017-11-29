using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsProgram
{
    class StatisticClass
    {
        public string Name { get; set; }
        public double Begin { get; set; }
        public double End { get; set; }
        public List<double> Elements { get; set; }
        public int AbsolutFrequency { get; set; }
        public int AccumulatedFrequency { get; set; }
        public double MidPoint { get; set; }

        public StatisticClass()
        {
            Elements = new List<double>();
        }

        public void DefineName()
        {
            Name = Begin + " |- " + End;
        }

    }
}
