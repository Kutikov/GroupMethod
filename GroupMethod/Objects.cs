namespace GroupMethod
{
    public class Objects
    {
        public class InputHuman
        {
            public string Id;
            public int Row;
            public Names[] Names;
            public int forOrder;
            public Range[] Ranges;
            public Normalized[] Normalized;
            public NormalizedRange[] NormalizedRanges;
            public InputHuman(int Row, string Id)
            {
                this.Id = Id;
                this.Row = Row;
            }
        }
        public class Normalized
        {
            public int colIndex;
            public string[] colNames;
            public string compoundName;
            public double ThisDouble;
            public Normalized(int colIndex, string[] colNames, double ThisDouble, string compoundName)
            {
                this.colIndex = colIndex;
                this.colNames = colNames;
                this.ThisDouble = ThisDouble;
                this.compoundName = compoundName;
            }
        }
        public class NormalizedRange
        {
            public int colIndex;
            public string[] colNames;
            public int ThisDouble;
            public string compoundName;
            public NormalizedRange(int colIndex, string[] colNames, int ThisDouble, string compoundName)
            {
                this.colIndex = colIndex;
                this.colNames = colNames;
                this.ThisDouble = ThisDouble;
                this.compoundName = compoundName;
            }
        }
        public class Names
        {
            public int colIndex;
            public string[] colNames;
            public string ThisName;
            public Names(int colIndex, string[] colNames, string ThisName)
            {
                this.colNames = colNames;
                this.colIndex = colIndex;
                this.ThisName = ThisName;
            }
        }
        public class Range
        {
            public int minRange;
            public int maxRange;
            public int colIndex;
            public string[] colNames;
            public string[] RangeSymbols;
            public int ThisRange;
            public string compoundName;
            public Range(int minRange, int maxRange, int colIndex, string[] colNames, string[] RangeSymbols, int ThisRange, string compoundName)
            {
                this.colIndex = colIndex;
                this.colNames = colNames;
                this.maxRange = maxRange;
                this.RangeSymbols = RangeSymbols;
                this.ThisRange = ThisRange;
                this.compoundName = compoundName;
                this.minRange = minRange;
            }
        }
        public class AnalizedGroup
        {
            public int count;
            public double min;
            public double max;
            public double percent;
            public double mediana;
            public double[] participatns;
            public double dispersia;
            public AnalizedGroup(double percent, double mediana)
            {
                this.mediana = mediana;
                this.percent = percent;
            }

        }
        public class ExcelGroup
        {
            public int count;
            public double min;
            public double max;
            public double percent;
            public double[] vars;
            public double mediana;
            public double standartDerivation;
            public double assymetry;
            public double closingTo;
            public double number;
            public ExcelGroup(double percent, double mediana, double number)
            {
                this.mediana = mediana;
                this.percent = percent;
                this.number = number;
            }
        }
        public class AnalizedTuple
        {
            public AnalizedGroup[] analizedGroups;
            public int colIndex;
            public double Entropy;
            public int groupsCount;
            public double min;
            public double max;
            public double diffstep;
            public double[] InputArray;
            public int[] groupedArray;
            public AnalizedTuple(AnalizedGroup[] analizedGroups, int colIndex, int groupsCount, 
                double min, double max, double diffstep, double[] InputArray, int[] groupedArray,
                double Entropy)
            {
                this.analizedGroups = analizedGroups;
                this.colIndex = colIndex;
                this.diffstep = diffstep;
                this.groupsCount = groupsCount;
                this.InputArray = InputArray;
                this.max = max;
                this.min = min;
                this.groupedArray = groupedArray;
                this.Entropy = Entropy;
            }
        }
        public class AlgorhytmOutPut
        {
            public InputHuman[] inputHumen;
            public AnalizedTuple[] analizedTuples;
            public string algName;
            public AlgorhytmOutPut(InputHuman[] inputHumen, AnalizedTuple[] analizedTuples, string algName)
            {
                this.analizedTuples = analizedTuples;
                this.algName = algName;
                this.inputHumen = inputHumen;
            }
        }
        public Objects()
        {

        }
    }
}