using System;
using System.Collections.Generic;
using System.Linq;

namespace GroupMethod
{
    class DenseAbsorbsUndense
    {
        double[] SortedArrayDouble;
        double[] SortedArrayDoubleBackup;
        int[] koefficients;
        bool[,] TypesOfFunction; //0=up; 1=down; 2=pick; 3=dispick;
        double[,] Distances; //0=bi; 1=left/2; 2=right/2; 3=right;
        double[,] DistributionNumbered; //last right border;
        double[,] DistributionCharacteristics; //x = classes, y = chars, z = itterations

        public class NormalizedDistribution
        {
            public double SummOfBidistances { get; set; }
            public int NumberOfPeople { get; set; }
            public double Minimum { get; set; }
            public double Maximum { get; set; }
            public double AntisummBidistanses { get; set; }
            public int Number { get; set; }
            public NormalizedDistribution() { }
        }

        public class UnityDistribution
        {
            public bool Used { get; set; }
            public bool WasSeen { get; set; }
            public double LR { get; set; }
            public int NumberOfPeople { get; set; }
            public double Size { get; set; }
            public bool Up { get; set; }
            public int Number { get; set; }
            public UnityDistribution() { }
        }

        public Objects.AnalizedTuple Sorter(Objects.InputHuman[] inputHumenArray, int col, double Entropy)
        {
           return Worker(inputHumenArray.Select(x => x.Normalized.ElementAt(col).ThisDouble).ToArray(), col, Entropy);
        }

        public Objects.AnalizedTuple Worker(double[] NonSortedArrayDouble, int col, double Entropy)
        { 
            int lastrow = NonSortedArrayDouble.GetUpperBound(0) + 1;
            SortedArrayDouble = new double[lastrow];
            var sorted = NonSortedArrayDouble.OrderByDescending(x => x);
            int i = lastrow - 1;
            foreach (var dbl in sorted)
            {
                SortedArrayDouble[i] = dbl;
                i--;
            }
            double min = SortedArrayDouble[0];
            double max = SortedArrayDouble[lastrow - 1];
            double meanDensity = 1.00 / SortedArrayDouble.Length; //(max - min) / SortedArrayDouble.Length;
            List<double> arrayListOfVars = new List<double>();
            List<int> arrayListOfTimes = new List<int>();
            for (int j = 0; j < SortedArrayDouble.Length; j++)
            {
                SortedArrayDouble[j] = (SortedArrayDouble[j] - min) / (max - min);
                if (j > 0 && SortedArrayDouble[j] == SortedArrayDouble[j - 1])
                {
                    arrayListOfTimes[arrayListOfTimes.Count - 1] = arrayListOfTimes[arrayListOfTimes.Count - 1] + 1;
                }
                else
                {
                    arrayListOfVars.Add(SortedArrayDouble[j]);
                    arrayListOfTimes.Add(1);
                }
            }
            SortedArrayDoubleBackup = SortedArrayDouble;
            SortedArrayDouble = arrayListOfVars.ToArray();
            koefficients = arrayListOfTimes.ToArray();
            DistributionNumbered = new double[3, SortedArrayDouble.Length]; //legacy method - 0 = Nothing;
            DistributionCharacteristics = new double[SortedArrayDouble.Length, 5]; // 0 = summOfBiDistances; 1 = Number; 2 = min; 3 = max; 4 = SummOfBistances ^ (-1);
            TypesOfFunction = new bool[SortedArrayDouble.Length, 4];
            Array.Clear(TypesOfFunction, 0, TypesOfFunction.Length);
            Distances = new double[SortedArrayDouble.Length, 4];
            Distances[0, 3] = SortedArrayDouble[1] - SortedArrayDouble[0];
            Distances[0, 1] = 0;
            Distances[SortedArrayDouble.Length - 1, 2] = 0;
            int distributionNumber = 0;
            for (int point = 1; point < SortedArrayDouble.Length - 1; point++)
            {
                Distances[point, 3] = SortedArrayDouble[point + 1] - SortedArrayDouble[point];
                Distances[point, 1] = Distances[point - 1, 3] / 2;
                Distances[point, 2] = Distances[point, 3] / 2;
                Distances[point, 0] = Distances[point, 1] + Distances[point, 2];
                if (point == 1)
                {
                    Distances[0, 2] = Distances[point, 1];
                    Distances[0, 0] = Distances[point, 1];
                }
                else if (point == SortedArrayDouble.Length - 2)
                {
                    Distances[point + 1, 1] = Distances[point, 2];
                    Distances[point + 1, 0] = Distances[point, 2];
                }
            }
            double hyper = 0;
            for(int biDistance = 0; biDistance < SortedArrayDouble.Length; biDistance++)
            {
                if (Distances[biDistance, 0] > meanDensity * 1.5)
                {
                    if(Distances[biDistance, 1] > Distances[biDistance, 2] * 1.5)
                    {
                        hyper = hyper + Distances[biDistance, 1] - Distances[biDistance, 2] * 1.5;
                    }
                    else if (Distances[biDistance, 2] > Distances[biDistance, 1] * 1.5)
                    {
                        hyper = hyper + Distances[biDistance, 2] - Distances[biDistance, 1] * 1.5;
                    }
                }
            }
            meanDensity = (1 - hyper) / SortedArrayDoubleBackup.Length;

            for (int point = 1; point < SortedArrayDouble.Length - 1; point++)
            {
                if (Distances[point - 1, 0] > Distances[point, 0] && Distances[point, 0] > Distances[point + 1, 0])
                {
                    TypesOfFunction[point, 0] = true;
                }
                else if (Distances[point - 1, 0] < Distances[point, 0] && Distances[point, 0] < Distances[point + 1, 0])
                {
                    TypesOfFunction[point, 1] = true;
                }
                else if (Distances[point - 1, 0] >= Distances[point, 0] && Distances[point, 0] < Distances[point + 1, 0])
                {
                    TypesOfFunction[point, 2] = true;
                }
                else if (Distances[point - 1, 0] > Distances[point, 0] && Distances[point, 0] == Distances[point + 1, 0])
                {
                    TypesOfFunction[point, 2] = true;
                }
                else if (Distances[point - 1, 0] <= Distances[point, 0] && Distances[point, 0] > Distances[point + 1, 0])
                {
                    TypesOfFunction[point, 3] = true;
                }
                else if (Distances[point - 1, 0] < Distances[point, 0] && Distances[point, 0] == Distances[point + 1, 0])
                {
                    TypesOfFunction[point, 3] = true;
                }
                else if (Distances[point - 1, 0] == Distances[point, 0] && Distances[point, 0] == Distances[point + 1, 0])
                {
                    TypesOfFunction[point, 2] = true;
                }
            }
            for (int point = 0; point < SortedArrayDouble.Length; point++)
            {
                if (TypesOfFunction[point, 3])
                {
                    if (Distances[point, 2] > Distances[point, 1])
                    {
                        DistributionNumbered[2, point] = distributionNumber;
                        distributionNumber++;
                    }
                    else
                    {
                        distributionNumber = Convert.ToInt32(DistributionNumbered[2, point - 1]) + 1;
                        DistributionNumbered[2, point] = distributionNumber;
                    }
                }
                else
                {
                    DistributionNumbered[2, point] = distributionNumber;
                }
                DistributionNumbered[1, point] = SortedArrayDouble[point];
            }
            for (int currClass = 0; currClass < distributionNumber + 1; currClass++)
            {
                DistributionCharacteristics[currClass, 2] = 0;
                bool firstWas = false;
                for (int number = 0; number < SortedArrayDouble.Length; number++)
                {
                    if (DistributionNumbered[2, number] == currClass)
                    {
                        if (!firstWas && number > 0 && currClass > 0)
                        {
                            DistributionCharacteristics[currClass - 1, 3] = GetRealIndex(number - 1, koefficients);
                            DistributionCharacteristics[currClass, 2] = GetRealIndex(number - 1, koefficients) + 1;
                            firstWas = true;
                        }
                        DistributionCharacteristics[currClass, 0] = DistributionCharacteristics[currClass, 0] + Distances[number, 0];
                        DistributionCharacteristics[currClass, 4] = DistributionCharacteristics[currClass, 4] + koefficients[number] / Distances[number, 0];
                        DistributionCharacteristics[currClass, 1] = DistributionCharacteristics[currClass, 1] + koefficients[number];
                    }
                    if (number == SortedArrayDouble.Length - 2)
                    {
                        DistributionCharacteristics[currClass, 3] = GetRealIndex(number + 1, koefficients);
                    }
                }
            }
            int previousNumberOfGroups = distributionNumber;
            int thisNumberOfGroups = distributionNumber;
            do
            {
                List<UnityDistribution> unityList = new List<UnityDistribution>();
                List<NormalizedDistribution> normalizedList = new List<NormalizedDistribution>();
                for (int distribution = 1; distribution < previousNumberOfGroups; distribution++)
                {
                    double densityLeft = (DistributionCharacteristics[distribution - 1, 0] + DistributionCharacteristics[distribution, 0]) /
                        (DistributionCharacteristics[distribution - 1, 1] + DistributionCharacteristics[distribution, 1]);
                    double densityRight = (DistributionCharacteristics[distribution + 1, 1] + DistributionCharacteristics[distribution, 1]) /
                        (DistributionCharacteristics[distribution + 1, 0] + DistributionCharacteristics[distribution, 0]);
                    UnityDistribution unity = new UnityDistribution
                    {
                        Number = distribution,
                        NumberOfPeople = Convert.ToInt32(DistributionCharacteristics[distribution, 1])
                    };
                    if (densityLeft <= densityRight)
                    {
                        if (densityLeft < meanDensity)
                        {
                            unity.LR = densityLeft;
                            unity.Used = false;
                            unity.Up = false;
                        }
                        else
                        {
                            unity.LR = -1000;
                            unity.Used = false;
                            unity.Up = false;
                        }
                    }
                    else if (densityLeft > densityRight)
                    {
                        if (densityRight < meanDensity)
                        {
                            unity.LR = densityRight;
                            unity.Used = false;
                            unity.Up = true;
                        }
                        else
                        {
                            unity.LR = -1000;
                            unity.Used = false;
                            unity.Up = false;
                        }
                    }
                    unity.Size = DistributionCharacteristics[distribution, 0];
                    unityList.Add(unity);
                }
                double densityRightF = (DistributionCharacteristics[1, 0] + DistributionCharacteristics[0, 0]) /
                        (DistributionCharacteristics[1, 1] + DistributionCharacteristics[0, 1]);
                double densityLeftL = (DistributionCharacteristics[previousNumberOfGroups - 1, 0] + DistributionCharacteristics[previousNumberOfGroups, 0]) /
                        (DistributionCharacteristics[previousNumberOfGroups - 1, 1] + DistributionCharacteristics[previousNumberOfGroups, 1]);
                densityRightF = densityRightF > meanDensity ? densityRightF : -1000;
                densityLeftL = densityLeftL > meanDensity ? densityLeftL : -1000;
                UnityDistribution unity0 = new UnityDistribution
                {
                    LR = densityRightF,
                    NumberOfPeople = Convert.ToInt32(DistributionCharacteristics[0, 1]),
                    Size = DistributionCharacteristics[0, 0],
                    Up = true,
                    Used = false,
                    Number = 0
                };
                UnityDistribution unity1 = new UnityDistribution
                {
                    LR = densityLeftL,
                    NumberOfPeople = Convert.ToInt32(DistributionCharacteristics[previousNumberOfGroups, 1]),
                    Size = DistributionCharacteristics[previousNumberOfGroups, 0],
                    Up = false,
                    Used = false,
                    Number = previousNumberOfGroups
                };
                unityList.Add(unity0);
                unityList.Add(unity1);
                UnityDistribution[] unityDistributions = unityList.OrderBy(x => x.LR).ToArray();
                for (int distribution = 0; distribution < unityDistributions.Length; distribution++)
                {
                    NormalizedDistribution normalized = new NormalizedDistribution();
                    int firstIndex = unityDistributions[distribution].Number;
                    if (!unityDistributions[distribution].Used)
                    {
                        if (unityDistributions[distribution].LR != -1000)
                        {
                            int koeff = unityDistributions[distribution].Up ? 1 : -1;
                            int secondIndex = Array.FindIndex(unityDistributions, (x) => (x.Number == firstIndex + koeff));
                            if (!unityDistributions[secondIndex].Used)
                            {
                                if (!unityDistributions[distribution].Up)
                                {
                                    normalized.SummOfBidistances = DistributionCharacteristics[firstIndex - 1, 0] + DistributionCharacteristics[firstIndex, 0];
                                    normalized.NumberOfPeople = Convert.ToInt32(DistributionCharacteristics[firstIndex - 1, 1] + DistributionCharacteristics[firstIndex, 1]);
                                    normalized.Minimum = DistributionCharacteristics[firstIndex - 1, 2];
                                    normalized.Maximum = DistributionCharacteristics[firstIndex, 3];
                                    normalized.AntisummBidistanses = DistributionCharacteristics[firstIndex - 1, 4] + DistributionCharacteristics[firstIndex, 4];
                                }
                                else
                                {
                                    normalized.SummOfBidistances = DistributionCharacteristics[firstIndex + 1, 0] + DistributionCharacteristics[firstIndex, 0];
                                    normalized.NumberOfPeople = Convert.ToInt32(DistributionCharacteristics[firstIndex + 1, 1] + DistributionCharacteristics[firstIndex, 1]);
                                    normalized.Minimum = DistributionCharacteristics[firstIndex, 2];
                                    normalized.Maximum = DistributionCharacteristics[firstIndex + 1, 3];
                                    normalized.AntisummBidistanses = DistributionCharacteristics[firstIndex + 1, 4] + DistributionCharacteristics[firstIndex, 4];
                                }
                                unityDistributions[secondIndex].Used = true;
                                unityDistributions[distribution].Used = true;
                                unityDistributions[secondIndex].WasSeen = false;
                                normalizedList.Add(normalized);
                            }
                            else
                            {
                                unityDistributions[distribution].WasSeen = true;
                            }
                        }
                        else
                        {
                            normalized.SummOfBidistances = DistributionCharacteristics[firstIndex, 0];
                            normalized.NumberOfPeople = Convert.ToInt32(DistributionCharacteristics[firstIndex, 1]);
                            normalized.Minimum = DistributionCharacteristics[firstIndex, 2];
                            normalized.Maximum = DistributionCharacteristics[firstIndex, 3];
                            normalized.AntisummBidistanses = DistributionCharacteristics[firstIndex, 4];
                            unityDistributions[distribution].Used = true;
                            if(normalized.SummOfBidistances != 0)
                            {
                                normalizedList.Add(normalized);
                            }                           
                        }
                    }
                }
                for (int distribution = 0; distribution < unityDistributions.Length; distribution++)
                {
                    NormalizedDistribution normalized = new NormalizedDistribution();
                    int firstIndex = unityDistributions[distribution].Number;
                    if (!unityDistributions[distribution].Used && unityDistributions[distribution].WasSeen)
                    {
                        normalized.SummOfBidistances = DistributionCharacteristics[firstIndex, 0];
                        normalized.NumberOfPeople = Convert.ToInt32(DistributionCharacteristics[firstIndex, 1]);
                        normalized.Minimum = DistributionCharacteristics[firstIndex, 2];
                        normalized.Maximum = DistributionCharacteristics[firstIndex, 3];
                        normalized.AntisummBidistanses = DistributionCharacteristics[firstIndex, 4];
                        unityDistributions[distribution].Used = true;
                        if (normalized.SummOfBidistances != 0)
                        {
                            normalizedList.Add(normalized);
                        }
                    }
                }
                NormalizedDistribution[] normalizedDistributions = normalizedList.OrderBy(x => x.Minimum).ToArray();
                DistributionCharacteristics = new double[normalizedDistributions.Length + 1, 5];
                for (int j = 0; j < normalizedDistributions.Length; j++)
                {
                    DistributionCharacteristics[j, 0] = normalizedDistributions[j].SummOfBidistances;
                    DistributionCharacteristics[j, 1] = normalizedDistributions[j].NumberOfPeople;
                    DistributionCharacteristics[j, 2] = normalizedDistributions[j].Minimum;
                    DistributionCharacteristics[j, 3] = normalizedDistributions[j].Maximum;
                    DistributionCharacteristics[j, 4] = normalizedDistributions[j].AntisummBidistanses;
                    if (j == normalizedDistributions.Length - 1)
                    {
                        DistributionCharacteristics[j, 3] = SortedArrayDoubleBackup.Length;
                    }
                }
                thisNumberOfGroups = previousNumberOfGroups;
                previousNumberOfGroups = normalizedDistributions.Length;
            } while (previousNumberOfGroups < thisNumberOfGroups);
            List<Objects.AnalizedGroup> analizedGroupsList = new List<Objects.AnalizedGroup>();
            for (int finalDistribution = 0; finalDistribution < thisNumberOfGroups; finalDistribution++)
            {
                int[] all = GetIndex(Convert.ToInt32(DistributionCharacteristics[finalDistribution, 2]), Convert.ToInt32(DistributionCharacteristics[finalDistribution, 3]), koefficients);
                int currMin = all[0];
                int currMax = all[1];
                double mediana = 0;
                for (int currPat = currMin; currPat < currMax; currPat++)
                {
                    mediana = mediana + koefficients[currPat] * ((Entropy * SortedArrayDouble[currPat] / DistributionCharacteristics[finalDistribution, 1]) +
                        ((1 - Entropy) * (SortedArrayDouble[currPat] * (1 / Distances[currPat, 0])) / DistributionCharacteristics[finalDistribution, 4]));
                }
                mediana = mediana * (max - min) + min;
                double percent = DistributionCharacteristics[finalDistribution, 1] / SortedArrayDoubleBackup.Length;
                analizedGroupsList.Add(new Objects.AnalizedGroup(percent, mediana)
                {
                    min = SortedArrayDouble[currMin] * (max - min) + min,
                    max = SortedArrayDouble[currMax - 1] * (max - min) + min + 0.00001,
                    count = Convert.ToInt32(DistributionCharacteristics[finalDistribution, 1])
                });                
            }
            Objects.AnalizedGroup[] analizedGroupArray = new Objects.AnalizedGroup[analizedGroupsList.Count];            
            analizedGroupArray = analizedGroupsList.ToArray();
            for(int f = 0; f < analizedGroupArray.Length; f++)
            {
                List<double> partList = new List<double>();
                double dispersia = 0;
                for (int g = 0; g < NonSortedArrayDouble.Length; g++)
                {
                    if(NonSortedArrayDouble[g] >= analizedGroupArray[f].min && NonSortedArrayDouble[g] <= analizedGroupArray[f].max)
                    {
                        partList.Add(NonSortedArrayDouble[g]);
                        dispersia = dispersia + Math.Pow(NonSortedArrayDouble[g] - analizedGroupArray[f].mediana, 2);
                    }
                }
                double[] partArray = new double[partList.Count];
                partArray = partList.ToArray();
                analizedGroupArray[f].dispersia = Math.Sqrt(dispersia / partArray.Length);
                analizedGroupArray[f].participatns = partArray;
            }
            return new Objects.AnalizedTuple(analizedGroupArray, col, thisNumberOfGroups + 1, min, max, 
                meanDensity, NonSortedArrayDouble, new int[analizedGroupArray.Length], Entropy);
        }

        public double DispersiaOfArray(int column, Objects.InputHuman[] inputHumenArray)
        {
            double[] inputArray = inputHumenArray.Select(x => x.Normalized.ElementAt(column).ThisDouble).ToArray();
            double mean = inputArray.Sum() / inputArray.Length;
            for(int i = 0; i < inputArray.Length; i++)
            {
                inputArray[i] = Math.Pow(inputArray[i] - mean, 2);
            }
            return Math.Sqrt(inputArray.Sum() / inputArray.Length);
        }

        public double GetRealIndex(int fakeindex, int[] koefficints)
        {
            double dout = 0;
            for (int i = 0; i < fakeindex + 1; i++)
            {
                dout = dout + koefficients[i];
            }
            return dout;
        }

        public int[] GetIndex(int min, int max, int[] koefficints)
        {
            int[] dout = new int[3];
            int outer = 0;
            int i = 0;
            while (min - 1 > outer)
            {
                outer = outer + koefficients[i];
                i++;
            }
            dout[0] = i;
            outer = 0;
            i = 0;
            while (max > outer)
            {
                outer = outer + koefficients[i];
                i++;
            }
            dout[1] = i;
            for(int z = dout[0]; z < dout[1]; z++)
            {
                dout[2] = dout[2] + koefficints[z];
            }
            return dout;
        }
    }
}
