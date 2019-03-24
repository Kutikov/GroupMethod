using System;
using System.Collections.Generic;
using System.Linq;

namespace GroupMethod
{
    public class DistantBecomesLast
    {
        double[,] SortedMatrix;
        double[,] NonModifiedSortedMatrix;
        double[,] FakeHumans;
        double[] Peaks;
        double allPeaks;
        int proposedGroups;
        int step = 1;

        public Objects.AnalizedTuple Sorter(Objects.InputHuman[] inputHumenArray, int col, double Upper, double Entropy, bool OneSided)
        {
            double[] NonSortedArrayDouble = inputHumenArray.Select(x => x.Normalized.ElementAt(col).ThisDouble).ToArray();
            int lastrow = NonSortedArrayDouble.Length;
            double[] SortedArrayDouble = new double[lastrow];
            var sorted = NonSortedArrayDouble.OrderByDescending(x => x);
            int i = lastrow - 1;
            foreach (var dbl in sorted)
            {
                SortedArrayDouble[i] = dbl;
                i--;
            }
            double HighestPeak = 0;
            double min = SortedArrayDouble[0];
            double max = SortedArrayDouble[lastrow - 1];
            int propose = 0;
            double diffstep;
            do
            {
                propose++;
                proposedGroups = lastrow / propose;
                SortedMatrix = new double[3, proposedGroups + 1];
                diffstep = (max - min) / Convert.ToDouble(proposedGroups);
                for (int human = 1; human < lastrow; human++)
                {
                    for (int diff = 1; diff < proposedGroups + 1; diff++)
                    {
                        if (NonSortedArrayDouble[human - 1] >= min && NonSortedArrayDouble[human - 1] <= min + diffstep && diff == 1)
                        {
                            SortedMatrix[1, 1]++;
                        }
                        else if (NonSortedArrayDouble[human - 1] > min + (diff - 1) * diffstep && NonSortedArrayDouble[human - 1] <= min + diff * diffstep)
                        {
                            SortedMatrix[1, diff]++;
                        }
                    }
                }
                HighestPeak = SortedMatrix[1, 1];
                for (int l = 1; l < proposedGroups + 1; l++)
                {
                    if (SortedMatrix[1, l] > HighestPeak)
                    {
                        HighestPeak = SortedMatrix[1, l];
                    }
                }
            } while (HighestPeak < lastrow / Upper);
            Peaks = new double[proposedGroups];
            FakeHumans = new double[proposedGroups + 1, 3];
            int secondLevelPeak = 0;
            for (int k = 0; k < proposedGroups; k++)
            {
                if (SortedMatrix[1, k + 1] > HighestPeak / Entropy && !OneSided)
                {
                    if (SortedMatrix[1, k + 1] > HighestPeak / Entropy * 2 && secondLevelPeak == 0)
                    {
                        secondLevelPeak = 1;
                    }
                    else if (SortedMatrix[1, k + 1] > HighestPeak / Entropy * 2 && secondLevelPeak != 0)
                    {
                        if (SortedMatrix[1, k] < HighestPeak / Entropy * 2)
                        {
                            step = step + 1;
                        }
                    }
                    Peaks[step] = Peaks[step] + SortedMatrix[1, k + 1];
                    SortedMatrix[2, k + 1] = step;
                }
                else if (SortedMatrix[1, k + 1] > HighestPeak / Entropy)
                {
                    if (SortedMatrix[1, k + 1] > HighestPeak / Entropy && secondLevelPeak == 0)
                    {
                        secondLevelPeak = 1;
                    }
                    Peaks[step] = Peaks[step] + SortedMatrix[1, k + 1];
                    SortedMatrix[2, k + 1] = step;
                }
                else
                {
                    secondLevelPeak = 0;
                    while (Peaks[step] > 0)
                    {
                        step++;
                    }
                }
            }
            for (int m = 0; m < proposedGroups; m++)
            {
                allPeaks = allPeaks + Peaks[m];
            }
            NonModifiedSortedMatrix = (double[,])SortedMatrix.Clone();
            for (int n = 1; n < proposedGroups + 1; n++)
            {
                if (SortedMatrix[2, n] > 0)
                {
                    SortedMatrix[1, n] = (SortedMatrix[1, n] / Peaks[Convert.ToInt32(SortedMatrix[2, n])]) * (diffstep * (n - 0.5));
                    FakeHumans[Convert.ToInt32(SortedMatrix[2, n]), 1] = FakeHumans[Convert.ToInt32(SortedMatrix[2, n]), 1] + SortedMatrix[1, n];
                    FakeHumans[Convert.ToInt32(SortedMatrix[2, n]), 2] = Convert.ToDouble(Peaks[Convert.ToInt32(SortedMatrix[2, n])]) / Convert.ToDouble(allPeaks);
                }
            }
            List<Objects.AnalizedGroup> analizedGroupsList = new List<Objects.AnalizedGroup>();
            for (int o = 1; o < step + 1; o++)
            {
                if (FakeHumans[o, 1] != 0)
                {
                    analizedGroupsList.Add(new Objects.AnalizedGroup(Math.Round(FakeHumans[o, 2], 2), Math.Round(FakeHumans[o, 1] + min, 2)));
                }
            }
            if(analizedGroupsList.Count == 0)
            {
                analizedGroupsList.Add(new Objects.AnalizedGroup(100, Math.Round((max - min) / 2 + min, 2)));
            }
            Objects.AnalizedGroup[] analizedGroupArray = new Objects.AnalizedGroup[analizedGroupsList.Count];
            analizedGroupArray = analizedGroupsList.ToArray();
            for(int j = 0; j < analizedGroupArray.Length; j++)
            {
                double localMin = min;
                double localMax = max;
                if (j != 0 && j != analizedGroupArray.Length - 1)
                {
                    localMin = (analizedGroupArray[j].mediana - analizedGroupArray[j - 1].mediana) / 2 + analizedGroupArray[j - 1].mediana;
                    localMax = (analizedGroupArray[j + 1].mediana - analizedGroupArray[j].mediana) / 2 + analizedGroupArray[j].mediana;
                }
                else if (j != 0)
                {
                    localMin = (analizedGroupArray[j].mediana - analizedGroupArray[j - 1].mediana) / 2 + analizedGroupArray[j - 1].mediana;
                }
                else if(j != analizedGroupArray.Length - 1)
                {
                    localMax = (analizedGroupArray[j + 1].mediana - analizedGroupArray[j].mediana) / 2 + analizedGroupArray[j].mediana;
                }
                int count = 0;
                List<double> partList = new List<double>();
                double dispersia = 0;
                for (int g = 0; g < SortedArrayDouble.Length; g++)
                {                    
                    if(SortedArrayDouble[g] >= localMin && SortedArrayDouble[g] < localMax)
                    {
                        count++;
                        partList.Add(SortedArrayDouble[g]);
                        dispersia = dispersia + Math.Pow(SortedArrayDouble[g] - analizedGroupArray[j].mediana, 2);
                    }
                }
                double[] partArray = new double[partList.Count];
                partArray = partList.ToArray();
                analizedGroupArray[j].participatns = partArray;
                analizedGroupArray[j].dispersia = Math.Sqrt(dispersia / partArray.Length);
                analizedGroupArray[j].max = localMax;
                analizedGroupArray[j].min = localMin;
                analizedGroupArray[j].count = count;
            }
            analizedGroupArray[analizedGroupArray.Length - 1].count = analizedGroupArray[analizedGroupArray.Length - 1].count + 1;
            int[] indicesArray = new int[SortedMatrix.GetUpperBound(1) + 1];
            for(int p = 0; p < indicesArray.Length; p++)
            {
                indicesArray[p] = Convert.ToInt32(SortedMatrix[2, p]);
            }
            return new Objects.AnalizedTuple(analizedGroupArray, col, proposedGroups, min, max, diffstep, NonSortedArrayDouble, indicesArray, Entropy);
        }
    }
}