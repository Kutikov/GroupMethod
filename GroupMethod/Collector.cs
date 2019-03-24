using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace GroupMethod
{
    public class Collector
    {
        public Objects.InputHuman[] inputHumenArray;
        public Objects.AnalizedTuple bestAnalizedTuple;
        public List<Objects.AnalizedTuple> bestAnalizedTuplesList = new List<Objects.AnalizedTuple>();
        public List<Objects.AnalizedTuple> candidatsAnalizedTuples = new List<Objects.AnalizedTuple>();
        public List<double> EvaluationOfEmpty = new List<double>();
        public List<double> EvaluationOfDispersia = new List<double>();
        public List<int> groupCountsList = new List<int>();
        public double minDispers;
        public readonly string DBL = "Distant Becomes Last";
        public readonly string DAU = "Dense Adsorbes Undense";
        public string method;
        public int MaxCalls = 50;
        public MainWindow mainPage;
        public double Entropy;
        public double DispersiaDivideIndex;
        private readonly string storageFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\damirkut\\GroupingMethod";

        public Collector(Objects.InputHuman[] inputHumenArray, MainWindow mainWindow, string method, double DispersiaDivideIndex)
        {
            this.inputHumenArray = inputHumenArray;
            this.mainPage = mainWindow;
            this.method = method;
            this.DispersiaDivideIndex = DispersiaDivideIndex;
        }

        public void DataGetter(double Upper, double Entropy, int times, int column, bool OneSided)
        {
            if(method == DBL)
            {
                DistantBecomesLast distantBecomesLast = new DistantBecomesLast();
                if (column < inputHumenArray[0].Normalized.Length && !OneSided)
                {
                    this.Entropy = Entropy;
                    if (times == 0)
                    {
                        this.minDispers = 10000;
                    }
                    EvaluatorDBL(column, times, distantBecomesLast.Sorter(inputHumenArray, column, Upper, Entropy, false));
                }
                else if (column < inputHumenArray[0].Normalized.Length)
                {
                    bestAnalizedTuplesList.Add(distantBecomesLast.Sorter(inputHumenArray, column, Upper, Entropy, true));
                    //mainPage.UpdateStats("ended " + column);
                }
                else
                {
                    Objects.AnalizedTuple[] analizedTuplesArray = new Objects.AnalizedTuple[bestAnalizedTuplesList.Count];
                    analizedTuplesArray = GroupsWritter(bestAnalizedTuplesList.ToArray());
                    string bestObjects = JsonConvert.SerializeObject(new Objects.AlgorhytmOutPut(inputHumenArray, analizedTuplesArray, method));
                    if (!Directory.Exists(storageFolder))
                    {
                        Directory.CreateDirectory(storageFolder);
                    }
                    File.WriteAllText(storageFolder + "\\temp.json", bestObjects);
                }
            }
            else
            {
                DenseAbsorbsUndense denseAbsorbsUndense = new DenseAbsorbsUndense();
                if (column < inputHumenArray[0].Normalized.Length && !OneSided)
                {
                    this.Entropy = Entropy;
                    if (times == 0)
                    {
                        this.minDispers = denseAbsorbsUndense.DispersiaOfArray(column, inputHumenArray) / DispersiaDivideIndex;
                    }
                    Objects.AnalizedTuple analizedTuple = denseAbsorbsUndense.Sorter(inputHumenArray, column, Entropy);
                    int step = 0;
                    do
                    {
                        step++;
                        analizedTuple = DAUResearcher(column, Entropy, analizedTuple);
                    } while (analizedTuple.analizedGroups.Any(x => x.dispersia > minDispers) && step < MaxCalls / 5);
                    EvaluatorDAU(column, times, analizedTuple);
                    //mainPage.UpdateStats("ended " + column);
                }
                else
                {
                    Objects.AnalizedTuple[] analizedTuplesArray = new Objects.AnalizedTuple[bestAnalizedTuplesList.Count];
                    analizedTuplesArray = GroupsWritter(bestAnalizedTuplesList.ToArray());
                    string bestObjects = JsonConvert.SerializeObject(new Objects.AlgorhytmOutPut(inputHumenArray, analizedTuplesArray, method));
                    if (!Directory.Exists(storageFolder))
                    {
                        Directory.CreateDirectory(storageFolder);
                    }
                    File.WriteAllText(storageFolder + "\\temp.json", bestObjects);
                }
            }           
        }

        public void EvaluatorDAU(int column, int times, Objects.AnalizedTuple analizedTuple)
        {
            if(times < MaxCalls / 2)
            {
                times++;
                candidatsAnalizedTuples.Add(analizedTuple);
                groupCountsList.Add(analizedTuple.groupsCount);
                DataGetter(0, (double) times * 2 / MaxCalls, times, column, false);
            }
            else
            {
                bestAnalizedTuplesList.Add(candidatsAnalizedTuples[groupCountsList.IndexOf(groupCountsList.Min())]);
                candidatsAnalizedTuples = new List<Objects.AnalizedTuple>();
                groupCountsList = new List<int>();
                times = 0;
                column++;
                DataGetter(0, times * 2 / MaxCalls, times, column, false);
            }
        }

        public void EvaluatorDBL(int column, int times, Objects.AnalizedTuple analizedTuple)
        {
            EvaluationOfEmpty.Add(0);
            EvaluationOfDispersia.Add(0);
            for (int l = 0; l < analizedTuple.groupedArray.Length; l++)
            {
                if (analizedTuple.groupedArray[l] == 0)
                {
                    EvaluationOfEmpty[times] = EvaluationOfEmpty[times] + 1;
                }
            }
            for(int m = 0; m < analizedTuple.analizedGroups.Length; m++)
            {
                EvaluationOfDispersia[times] = EvaluationOfDispersia[times] + analizedTuple.analizedGroups[m].dispersia;
            }
            EvaluationOfDispersia[times] = EvaluationOfDispersia[times] / analizedTuple.analizedGroups.Length;
            EvaluationOfEmpty[times] = EvaluationOfEmpty[times] / analizedTuple.InputArray.Length;
            if (minDispers >= EvaluationOfDispersia[times] && times < MaxCalls)
            {
                this.bestAnalizedTuple = analizedTuple;
                minDispers = EvaluationOfDispersia[times];
                Entropy = Entropy + 0.25;
                times++;
                DataGetter(15, Entropy, times, column, false);
            }
            else
            {
                Entropy = Entropy - 0.25;
                DataGetter(15, Entropy, times, column, true);
                Entropy = 1.0;
                times = 0;
                column++;
                DataGetter(15, Entropy, times, column, false);
            }
        }

        public Objects.AnalizedTuple DAUResearcher(int column, double Entropy, Objects.AnalizedTuple analizedTuple)
        {
            DenseAbsorbsUndense denseAbsorbsUndense = new DenseAbsorbsUndense();
            List<Objects.AnalizedGroup> analizedGroupsList = analizedTuple.analizedGroups.ToList();
            for (int i = 0; i < analizedTuple.analizedGroups.Length; i++)
            {
                if(analizedTuple.analizedGroups[i].dispersia > minDispers)
                {
                    Objects.AnalizedTuple analizedTupleNew = denseAbsorbsUndense.Worker(analizedTuple.analizedGroups[i].participatns, column, Entropy);
                    analizedGroupsList.Remove(analizedTuple.analizedGroups[i]);
                    analizedGroupsList.AddRange(analizedTupleNew.analizedGroups);
                }
            }
            Objects.AnalizedGroup[] analizedGroupsArray = new Objects.AnalizedGroup[analizedGroupsList.Count];
            analizedGroupsArray = analizedGroupsList.OrderBy(x => x.mediana).ToArray();
            analizedTuple.groupsCount = analizedGroupsArray.Length;
            analizedTuple.analizedGroups = analizedGroupsArray;
            return analizedTuple;
        }

        public Objects.AnalizedTuple[] GroupsWritter(Objects.AnalizedTuple[] inputTupleArray)
        {
            for(int tuple = 0; tuple < inputTupleArray.Length; tuple++)
            {
                for(int group = 0; group < inputTupleArray[tuple].analizedGroups.Length; group++)
                {
                    for (int human = 0; human < inputHumenArray.Length; human++)
                    {
                        if(inputHumenArray[human].Normalized[tuple].ThisDouble >= inputTupleArray[tuple].analizedGroups[group].min &&
                            inputHumenArray[human].Normalized[tuple].ThisDouble <= inputTupleArray[tuple].analizedGroups[group].max)
                        {
                            inputHumenArray[human].NormalizedRanges[tuple] = new Objects.NormalizedRange(
                                inputHumenArray[human].Normalized[tuple].colIndex,
                                inputHumenArray[human].Normalized[tuple].colNames,
                                group + 1, inputHumenArray[human].Normalized[tuple].compoundName);
                        }
                    }
                    inputTupleArray[tuple].analizedGroups[group].percent = (double) inputTupleArray[tuple].analizedGroups[group].count / inputHumenArray.Length;
                }
                int dopGroups = inputTupleArray[tuple].analizedGroups.Length;
                for (int human2 = 0; human2 < inputHumenArray.Length; human2++)
                {
                    if(inputHumenArray[human2].NormalizedRanges[tuple] == null)
                    {
                        inputHumenArray[human2].NormalizedRanges[tuple] = new Objects.NormalizedRange(
                                inputHumenArray[human2].Normalized[tuple].colIndex,
                                inputHumenArray[human2].Normalized[tuple].colNames,
                                dopGroups, inputHumenArray[human2].Normalized[tuple].compoundName);
                        dopGroups++;
                    }
                }
                double[] SortedArray = inputHumenArray.Select(x => x.Normalized.ElementAt(tuple).ThisDouble).OrderBy(x => x).ToArray();
                double min = SortedArray.Min();
                double max = SortedArray.Max();
                double diffstep = (max - min) / SortedArray.Length;
                int[] groupedArray = new int[SortedArray.Length];
                int initial = 1;
                for(int patientsDouble = 0; patientsDouble < SortedArray.Length; patientsDouble++)
                {
                    if(SortedArray[patientsDouble] < initial * diffstep + min)
                    {
                        if(initial < SortedArray.Length - 1)
                        {
                            groupedArray[initial - 1]++;
                        }
                        else
                        {
                            groupedArray[initial - 2]++;
                        }
                    }
                    else
                    {
                        initial++;
                        patientsDouble--;
                    }
                }
                inputTupleArray[tuple].groupedArray = groupedArray;
                inputTupleArray[tuple].diffstep = diffstep;
            }
            return inputTupleArray;
        }
    }
}