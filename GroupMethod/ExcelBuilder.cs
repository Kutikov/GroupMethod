using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;

namespace GroupMethod
{
    public class ExcelBuilder
    {
        public string[][][] ExcelStringTemplates;
        public Color[][][] ExcelColorsTemplate;
        private Objects.AlgorhytmOutPut AlgorhytmOutPut;
        private Objects.InputHuman[] inputHumen;
        private Objects.ExcelGroup[][] builded;
        private readonly Objects.AnalizedTuple[] analizedTuplesArray;
        private readonly MainWindow mainWindow;
        private readonly string storageFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\damirkut\\GroupingMethod";

        public ExcelBuilder(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            string prevOutput = File.ReadAllText(storageFolder + "\\temp.json");
            this.AlgorhytmOutPut = JsonConvert.DeserializeObject<Objects.AlgorhytmOutPut>(prevOutput);
            this.inputHumen = AlgorhytmOutPut.inputHumen;
            this.analizedTuplesArray = AlgorhytmOutPut.analizedTuples;
        }

        public Tuple<string[][][],Color[][][]> Build(double Enclosing, int orderBy, bool isRange)
        {
            ExcelStringTemplates = new string[4][][];
            ExcelColorsTemplate = new Color[4][][];

            ExcelStringTemplates[1] = GroupsWritter(BuildGroups(Enclosing));
            ReWritePatients();
            var Patients = PatientsWritter(orderBy, isRange);
            ExcelStringTemplates[2] = Patients.Item1;
            ExcelColorsTemplate[2] = Patients.Item2;
            var Correlation1 = Correlation();
            ExcelStringTemplates[3] = Correlation1.Item1;
            ExcelColorsTemplate[3] = Correlation1.Item2;
            ExcelStringTemplates[0] = SourcesWritter();
            return new Tuple<string[][][], Color[][][]>(ExcelStringTemplates, ExcelColorsTemplate);
        }

        private Tuple<string[][],Color[][]> Correlation()
        {
            int rowColumnIndex = 1 + inputHumen[0].NormalizedRanges.Length + inputHumen[0].Ranges.Length;
            string[][] excelSheetTemplate = new string[rowColumnIndex][];
            Color[][] excelColorTemplate = new Color[rowColumnIndex][];
            for(int i = 0; i < rowColumnIndex; i++)
            {
                excelSheetTemplate[i] = new string[rowColumnIndex];
                excelColorTemplate[i] = new Color[rowColumnIndex];
            }
            for(int column1 = 0; column1 < rowColumnIndex - 1; column1++)
            {
                int[] column1Val;
                if (column1 > inputHumen[0].NormalizedRanges.Length - 1)
                {
                    column1Val = inputHumen.Select(x => x.Ranges.ElementAt(column1 - inputHumen[0].NormalizedRanges.Length).ThisRange).ToArray();
                    excelSheetTemplate[0][column1 + 1] = inputHumen[0].Ranges[column1 - inputHumen[0].NormalizedRanges.Length].compoundName;
                }
                else
                {
                    column1Val = inputHumen.Select(x => x.NormalizedRanges.ElementAt(column1).ThisDouble).ToArray();
                    excelSheetTemplate[0][column1 + 1] = inputHumen[0].NormalizedRanges[column1].compoundName;
                }
                for (int column2 = 0; column2 < column1 + 1; column2++)
                {                    
                    int[] column2Val;                     
                    if (column2 > inputHumen[0].NormalizedRanges.Length - 1)
                    {
                        column2Val = inputHumen.Select(x => x.Ranges.ElementAt(column2 - inputHumen[0].NormalizedRanges.Length).ThisRange).ToArray();
                        excelSheetTemplate[column2 + 1][0] = inputHumen[0].Ranges[column2 - inputHumen[0].NormalizedRanges.Length].compoundName;
                    }
                    else
                    {
                        column2Val = inputHumen.Select(x => x.NormalizedRanges.ElementAt(column2).ThisDouble).ToArray();
                        excelSheetTemplate[column2 + 1][0] = inputHumen[0].NormalizedRanges[column2].compoundName;
                    }
                    double Spearmen = ComputeRankCorrelation(column1Val, column2Val);
                    excelSheetTemplate[column1 + 1][column2 + 1] = string.Format("{0:0.00}", Spearmen);
                    excelColorTemplate[column1 + 1][column2 + 1] = GetColor(Spearmen, -1, 1);
                }
            }
            return new Tuple<string[][], Color[][]>(excelSheetTemplate, excelColorTemplate);
        }

        private void ReWritePatients()
        {
            for(int column = 0; column < inputHumen[0].Normalized.Length; column++)
            {
                for(int group = 0; group < builded[column].Length; group++)
                {
                    for(int human = 0; human < inputHumen.Length; human++)
                    {
                        if(inputHumen[human].NormalizedRanges[column] != null)
                        {
                            if (inputHumen[human].Normalized[column].ThisDouble >= builded[column][group].min
                            && inputHumen[human].Normalized[column].ThisDouble <= builded[column][group].max)
                            {
                                inputHumen[human].NormalizedRanges[column].ThisDouble = group + 1;
                            }
                        }                        
                    }
                }
            }
        }

        private string[][] SourcesWritter()
        {
            int rowsCount = inputHumen.Length + inputHumen[0].Normalized[0].colNames.Length;
            int columnCount = inputHumen[0].NormalizedRanges.Length + inputHumen[0].Names.Length + inputHumen[0].Ranges.Length;
            string[][] excelSheetTemplate = new string[columnCount][];
            for (int i = 0; i < columnCount; i++)
            {
                excelSheetTemplate[i] = new string[rowsCount];
                
            }
            for (int human = 0; human < inputHumen.Length; human++)
            {
                int humanRow = human + inputHumen[human].Normalized[0].colNames.Length;
                int siding = 0;
                for (int name = 0; name < inputHumen[human].Names.Length; name++)
                {
                    excelSheetTemplate[name][humanRow] = inputHumen[human].Names[name].ThisName;
                    if (human == 0)
                    {
                        for (int pseudoRow = 0; pseudoRow < inputHumen[human].Names[name].colNames.Length; pseudoRow++)
                        {
                            excelSheetTemplate[name][pseudoRow] = inputHumen[human].Names[name].colNames[pseudoRow];
                        }
                    }
                }
                siding = inputHumen[human].Names.Length;
                for (int normRange = 0; normRange < inputHumen[human].Normalized.Length; normRange++)
                {
                    if (inputHumen[human].Normalized[normRange] != null)
                    {
                        excelSheetTemplate[siding + normRange][humanRow] = inputHumen[human].Normalized[normRange].ThisDouble.ToString();                        
                        if (human == 0)
                        {
                            for (int pseudoRow = 0; pseudoRow < inputHumen[human].Normalized[normRange].colNames.Length; pseudoRow++)
                            {
                                excelSheetTemplate[siding + normRange][pseudoRow] = inputHumen[human].Normalized[normRange].colNames[pseudoRow];
                            }
                        }
                    }
                    else
                    {
                        excelSheetTemplate[siding + normRange][humanRow] = "NaN";
                    }
                }
                siding = siding + inputHumen[human].NormalizedRanges.Length;
                for (int range = 0; range < inputHumen[human].Ranges.Length; range++)
                {
                    excelSheetTemplate[siding + range][humanRow] = inputHumen[human].Ranges[range].RangeSymbols[inputHumen[human].Ranges[range].ThisRange];                    
                }
            }
            return excelSheetTemplate;
        }

        private Tuple<string[][],Color[][]> PatientsWritter(int orderBy, bool isRange)
        {
            int rowsCount = inputHumen.Length + inputHumen[0].Normalized[0].colNames.Length;
            int columnCount = inputHumen[0].NormalizedRanges.Length + inputHumen[0].Names.Length + inputHumen[0].Ranges.Length;
            string[][] excelSheetTemplate = new string[columnCount][];
            Color[][] excelColorTemplate = new Color[columnCount][];
            for(int i = 0; i < columnCount; i++)
            {
                excelSheetTemplate[i] = new string[rowsCount];
                excelColorTemplate[i] = new Color[rowsCount];
            }
            if (!isRange)
            {
                for(int i = 0; i < inputHumen.Length; i++)
                {
                    inputHumen[i].forOrder = inputHumen[i].NormalizedRanges[orderBy].ThisDouble;
                }
                inputHumen = inputHumen.OrderBy(x => x.forOrder).ToArray();
            }
            else
            {
                for (int i = 0; i < inputHumen.Length; i++)
                {
                    inputHumen[i].forOrder = inputHumen[i].Ranges[orderBy].ThisRange;
                }
                inputHumen = inputHumen.OrderBy(x => x.forOrder).ToArray();
            }
            for(int human = 0; human < inputHumen.Length; human++)
            {
                int humanRow = human + inputHumen[human].Normalized[0].colNames.Length;
                int siding = 0;
                for(int name = 0; name < inputHumen[human].Names.Length; name++)
                {
                    excelSheetTemplate[name][humanRow] = inputHumen[human].Names[name].ThisName;
                    if(human == 0)
                    {
                        for(int pseudoRow = 0; pseudoRow < inputHumen[human].Names[name].colNames.Length; pseudoRow++)
                        {
                            excelSheetTemplate[name][pseudoRow] = inputHumen[human].Names[name].colNames[pseudoRow];
                        }                        
                    }
                }
                siding = inputHumen[human].Names.Length;
                for (int normRange = 0; normRange < inputHumen[human].NormalizedRanges.Length; normRange++)
                {
                    if(inputHumen[human].NormalizedRanges[normRange] != null)
                    {
                        excelSheetTemplate[siding + normRange][humanRow] = inputHumen[human].NormalizedRanges[normRange].ThisDouble.ToString();
                        excelColorTemplate[siding + normRange][humanRow] = GetColor(inputHumen[human].NormalizedRanges[normRange].ThisDouble,
                            1, builded[normRange].Length);
                        if (human == 0)
                        {
                            for (int pseudoRow = 0; pseudoRow < inputHumen[human].NormalizedRanges[normRange].colNames.Length; pseudoRow++)
                            {
                                excelSheetTemplate[siding + normRange][pseudoRow] = inputHumen[human].NormalizedRanges[normRange].colNames[pseudoRow];
                            }
                        }
                    }
                    else
                    {
                        excelSheetTemplate[siding + normRange][humanRow] = "NaN";
                        excelColorTemplate[siding + normRange][humanRow] = Color.FromArgb(255, 230, 230, 230);
                        if (human == 0)
                        {
                            for (int pseudoRow = 0; pseudoRow < inputHumen[human].NormalizedRanges[normRange].colNames.Length; pseudoRow++)
                            {
                                excelSheetTemplate[siding + normRange][pseudoRow] = inputHumen[human].NormalizedRanges[normRange].colNames[pseudoRow];
                            }
                        }
                    }
                }
                siding = siding + inputHumen[human].NormalizedRanges.Length;
                for (int range = 0; range < inputHumen[human].Ranges.Length; range++)
                {
                    excelSheetTemplate[siding + range][humanRow] = inputHumen[human].Ranges[range].RangeSymbols[inputHumen[human].Ranges[range].ThisRange];
                    excelColorTemplate[siding + range][humanRow] = GetColor(inputHumen[human].Ranges[range].ThisRange,
                        inputHumen[human].Ranges[range].minRange, inputHumen[human].Ranges[range].maxRange);
                    if (human == 0)
                    {
                        for (int pseudoRow = 0; pseudoRow < inputHumen[human].Ranges[range].colNames.Length; pseudoRow++)
                        {
                            excelSheetTemplate[siding + range][pseudoRow] = inputHumen[human].Ranges[range].colNames[pseudoRow];
                        }
                    }
                }
            }
            return new Tuple<string[][], Color[][]>(excelSheetTemplate, excelColorTemplate);
        }

        private Objects.ExcelGroup[][] BuildGroups(double Enclosing)
        {
            Objects.ExcelGroup[][] excelGroupsArray = new Objects.ExcelGroup[inputHumen[0].NormalizedRanges.Length][];
            for (int column = 0; column < inputHumen[0].NormalizedRanges.Length; column++)
            {
                Objects.ExcelGroup[] excelGroups = new Objects.ExcelGroup[analizedTuplesArray[column].analizedGroups.Length];
                for(int group = 0; group < analizedTuplesArray[column].analizedGroups.Length; group++)
                {
                    Objects.AnalizedGroup currentAnalizedGroup = analizedTuplesArray[column].analizedGroups[group];
                    excelGroups[group] = new Objects.ExcelGroup(currentAnalizedGroup.percent, currentAnalizedGroup.mediana, group);
                    double[] Squares = new double[currentAnalizedGroup.participatns.Length];
                    double[] Qubes = new double[currentAnalizedGroup.participatns.Length];
                    for(int part = 0; part < currentAnalizedGroup.participatns.Length; part++)
                    {
                        Squares[part] = Math.Pow(currentAnalizedGroup.participatns[part] - currentAnalizedGroup.mediana, 2);
                        Qubes[part] = Math.Pow(currentAnalizedGroup.participatns[part] - currentAnalizedGroup.mediana, 3);
                    }
                    excelGroups[group].standartDerivation = Math.Sqrt(Squares.Sum() / (Squares.Length - 1));
                    excelGroups[group].min = currentAnalizedGroup.min;
                    excelGroups[group].max = currentAnalizedGroup.max;
                    excelGroups[group].count = currentAnalizedGroup.count;
                    excelGroups[group].assymetry = Qubes.Sum() / (Squares.Length * Math.Pow(currentAnalizedGroup.dispersia, 3));
                    excelGroups[group].vars = currentAnalizedGroup.participatns;
                    if (group < analizedTuplesArray[column].analizedGroups.Length - 1)
                    {
                        Objects.AnalizedGroup nextAnalizedGroup = analizedTuplesArray[column].analizedGroups[group + 1];
                        double Mean1 = currentAnalizedGroup.participatns.Average();
                        double Mean2 = nextAnalizedGroup.participatns.Average();
                        double Mediana1 = currentAnalizedGroup.mediana;
                        double Mediana2 = nextAnalizedGroup.mediana;
                        double NormAll = analizedTuplesArray[column].max - analizedTuplesArray[column].min;
                        double Min1 = currentAnalizedGroup.min;
                        double Max1 = currentAnalizedGroup.max;
                        double Min2 = nextAnalizedGroup.min;
                        double Max2 = nextAnalizedGroup.max;
                        excelGroups[group].closingTo = CoeffCounter(Mean1, Mean2, Mediana1, Mediana2, NormAll, Min1, Max1, Min2, Max2);
                    }
                }
                excelGroups = Encloser(excelGroups, Enclosing, column);
                excelGroupsArray[column] = excelGroups;
            }
            this.builded = excelGroupsArray;
            return excelGroupsArray;
        }

        private Objects.ExcelGroup[] Encloser(Objects.ExcelGroup[] BuildGroups, double Enclosing, int column)
        {
            BuildGroups = BuildGroups.OrderByDescending(x => x.closingTo).ToArray();
            if(BuildGroups[0].closingTo > Enclosing)
            {
                List<Objects.ExcelGroup> excelGroupsList = BuildGroups.ToList();
                Objects.ExcelGroup firstGroup = BuildGroups[0];
                Objects.ExcelGroup secondGroup = BuildGroups.FirstOrDefault(x => x.number == firstGroup.number + 1);
                double weightFirst = (double)firstGroup.count / (firstGroup.count + secondGroup.count);
                double weightSecond = (double)secondGroup.count / (firstGroup.count + secondGroup.count);
                double meadiana = weightFirst * firstGroup.mediana + weightSecond * secondGroup.mediana;
                double percent = firstGroup.percent + secondGroup.percent;
                double mean = (secondGroup.max - firstGroup.min) / 2;
                List<double> fV = firstGroup.vars.ToList();
                List<double> lV = secondGroup.vars.ToList();
                List<double> sV = secondGroup.vars.ToList();
                sV.AddRange(fV);
                double[] cV = new double[sV.Count];
                cV = sV.ToArray();
                double[] ScV = new double[cV.Length];
                double[] QcV = new double[cV.Length];
                for(int v = 0; v < cV.Length; v++)
                {
                    ScV[v] = Math.Pow(cV[v] - mean, 2);
                    QcV[v] = Math.Pow(cV[v] - mean, 3);
                }
                Objects.ExcelGroup compound = new Objects.ExcelGroup(percent, meadiana, 0)
                {
                    min = firstGroup.min,
                    max = secondGroup.max,
                    count = firstGroup.count + secondGroup.count,
                    vars = cV,
                    standartDerivation = Math.Sqrt(ScV.Sum() / (ScV.Length - 1)),
                    assymetry = QcV.Sum() / (QcV.Length * Math.Pow(ScV.Sum() / ScV.Length, (3 / 2)))
                };
                excelGroupsList.Remove(firstGroup);
                excelGroupsList.Remove(secondGroup);
                excelGroupsList.Add(compound);
                excelGroupsList = excelGroupsList.OrderBy(x => x.mediana).ToList();
                for(int group = 0; group < excelGroupsList.Count; group++)
                {
                    excelGroupsList[group].number = group;
                    if(group < excelGroupsList.Count - 1)
                    {
                        Objects.ExcelGroup nextExcelGroup = excelGroupsList[group + 1];
                        Objects.ExcelGroup currentExcelGroup = excelGroupsList[group];
                        double Mean1 = fV.Average();
                        double Mean2 = lV.Average();
                        double Mediana1 = currentExcelGroup.mediana;
                        double Mediana2 = nextExcelGroup.mediana;
                        double NormAll = analizedTuplesArray[column].max - analizedTuplesArray[column].min;
                        double Min1 = currentExcelGroup.min;
                        double Max1 = currentExcelGroup.max;
                        double Min2 = nextExcelGroup.min;
                        double Max2 = nextExcelGroup.max;
                        excelGroupsList[group].closingTo = CoeffCounter(Mean1, Mean2, Mediana1, Mediana2, NormAll, Min1, Max1, Min2, Max2);
                    }
                }
                Objects.ExcelGroup[] reTry = new Objects.ExcelGroup[excelGroupsList.Count];
                reTry = excelGroupsList.ToArray();
                BuildGroups = Encloser(reTry, Enclosing, column);
            }
            BuildGroups = BuildGroups.OrderBy(x => x.mediana).ToArray();
            return BuildGroups;
        }

        private string[][] GroupsWritter(Objects.ExcelGroup[][] BuildGroups)
        {
            int max = 0;
            string[] Captions = new string[9] { "Медиана ", "Процент ", "Количество ", "Минимум ", "Максимум ", "Среднее ", "СКО ", "Ассиметрия ", "Сродство " };
            string[][] excelSheet = new string[BuildGroups.Length + 1][];
            for(int column = 0; column < BuildGroups.Length; column++)
            {
                string[] excelColumn = new string[BuildGroups[column].Length * 10 + 3];
                if(max < BuildGroups[column].Length)
                {
                    max = BuildGroups[column].Length;
                }
                for (int group = 0; group < BuildGroups[column].Length; group++)
                {
                    excelColumn[group * 10 + 4] = string.Format("{0:0.00}", BuildGroups[column][group].mediana);
                    excelColumn[group * 10 + 5] = string.Format("{0:0.00}", BuildGroups[column][group].percent * 100);
                    excelColumn[group * 10 + 6] = string.Format("{0:0.00}", BuildGroups[column][group].count);
                    excelColumn[group * 10 + 7] = string.Format("{0:0.00}", BuildGroups[column][group].min);
                    excelColumn[group * 10 + 8] = string.Format("{0:0.00}", BuildGroups[column][group].max);
                    excelColumn[group * 10 + 9] = string.Format("{0:0.00}", BuildGroups[column][group].vars.Average());
                    excelColumn[group * 10 + 10] = string.Format("{0:0.00}", BuildGroups[column][group].standartDerivation);
                    excelColumn[group * 10 + 11] = string.Format("{0:0.00}", BuildGroups[column][group].assymetry);
                    excelColumn[group * 10 + 12] = string.Format("{0:0.00}", BuildGroups[column][group].closingTo);
                }
                excelColumn[2] = analizedTuplesArray[column].Entropy.ToString();
                excelColumn[1] = BuildGroups[column].Length.ToString();
                excelColumn[0] = inputHumen[0].Normalized[column].compoundName;
                excelSheet[column + 1] = excelColumn;
            }
            string[] excelColumnCaptions = new string[max * 10 + 3];
            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < Captions.Length; j++)
                {
                    if(j < Captions.Length - 1)
                    {
                        excelColumnCaptions[i * 10 + j + 4] = Captions[j] + (i + 1) + " группы";
                    }
                    else
                    {
                        excelColumnCaptions[i * 10 + j + 4] = Captions[j] + (i + 1) + " к " + (i + 2) + " группе";
                    }
                }               
            }
            excelColumnCaptions[2] = "Энтропия";
            excelColumnCaptions[1] = "Всего групп";
            excelSheet[0] = excelColumnCaptions;
            return excelSheet;
        }

        private double CoeffCounter(double Mean1, double Mean2, double Mediana1, double Mediana2, double NormAll, double Min1, double Max1, double Min2, double Max2)
        {
            double outDbl = 0;
            if (Min2 > 0 && Max2 > 0)
            {
                if (Min1 < Mediana1 && Min2 < Mediana2 && Max1 > Mediana1 && Max2 > Mediana2)
                {
                    double firstWeight = 1 - ((Mean1 - Mediana1) / (Max1 - Min1)) + ((Mean2 - Mediana2) / (Max2 - Min2));
                    double secondWeight = 1 - firstWeight / 2;
                    outDbl = 1 - ((firstWeight * (Mediana2 - Mediana1) / NormAll) + (secondWeight * (Min2 - Max1) / NormAll));
                }
                else if (Min1 > Mediana1 || Max1 < Mediana1)
                {
                    double firstWeight = 1 + ((Mean2 - Mediana2) / (Max2 - Min2));
                    double secondWeight = 1 - firstWeight / 2;
                    outDbl = 1 - ((firstWeight * (Mediana2 - Mediana1) / NormAll) + (secondWeight * (Min2 - Max1) / NormAll));
                }
                else if (Min2 > Mediana2 || Max2 < Mediana2)
                {
                    double firstWeight = 1 - ((Mean1 - Mediana1) / (Max1 - Min1));
                    double secondWeight = 1 - firstWeight / 2;
                    outDbl = 1 - ((firstWeight * (Mediana2 - Mediana1) / NormAll) + (secondWeight * (Min2 - Max1) / NormAll));
                }
                else
                {
                    outDbl = (Mediana2 - Mediana1) / NormAll;
                }
            }
            return outDbl;
        }

        private Color GetColor(double thisDouble, double min, double max)
        {
            double[] maxCol = { 255, 18, 18 };
            double[] midCol = { 250, 250, 250 };
            double[] minCol = { 36, 132, 200 };
            double[] stepCol = new double[3];            
            if (max > min && thisDouble <= max && thisDouble >= min)
            {
                double thisVal = (thisDouble - min) / (max - min) * 100;
                for (int i = 0; i < 3; i++)
                {
                    if(thisVal > 50)
                    {
                        stepCol[i] = ((maxCol[i] - midCol[i]) / 100 * (thisVal - 50) * 2) + midCol[i];
                    }
                    else
                    {
                        stepCol[i] = ((midCol[i] - minCol[i]) / 100 * thisVal * 2) + minCol[i];
                    }                    
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    stepCol[i] = 255;
                }
            }
            return Color.FromArgb(255, Convert.ToByte(stepCol[0]), Convert.ToByte(stepCol[1]), Convert.ToByte(stepCol[2]));
        }

        private static double ComputeRankCorrelation(int[] X, int[] Y)
        {
            var n = Math.Min(X.Length, Y.Length);
            var list = new List<DataPoint>(n);
            for (var i = 0; i < n; i++)
            {
                list.Add(new DataPoint() { X = X[i], Y = Y[i] });
            }
            var byXList = list.OrderBy(r => r.X).ToArray();
            var byYList = list.OrderBy(r => r.Y).ToArray();
            for (var i = 0; i < n; i++)
            {
                byXList[i].RankByX = i + 1;
                byYList[i].RankByY = i + 1;
            }
            var sumRankDiff = list.Aggregate((long) 0, (total, r) => total += Lsqr(r.RankByX - r.RankByY));
            var rankCorrelation = 1 - (double)(6 * sumRankDiff) / (n * ((long) n * n - 1));
            return rankCorrelation;
        }

        private class DataPoint
        {
            public double X, Y;
            public int RankByX, RankByY;
        }

        private static long Lsqr(long d)
        {
            return d * d;
        }
    }
}