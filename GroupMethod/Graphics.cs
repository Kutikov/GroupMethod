using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace GroupMethod
{
    public class Graphics
    {
        private readonly string compoundString;
        private Objects.AlgorhytmOutPut AlgorhytmOutPut;
        private readonly Objects.AnalizedTuple[] analizedTuples;
        private readonly Objects.InputHuman[] inputHumen;
        private int NeededIndex;
        private int top;
        private readonly MainWindow mainWindow;
        private readonly string storageFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\damirkut\\GroupingMethod";

        public Graphics(string compoundString, MainWindow mainWindow)
        {
            this.compoundString = compoundString;
            string content = File.ReadAllText(storageFolder + "\\temp.json");
            this.AlgorhytmOutPut = JsonConvert.DeserializeObject<Objects.AlgorhytmOutPut>(content);
            this.analizedTuples = AlgorhytmOutPut.analizedTuples;
            this.inputHumen = AlgorhytmOutPut.inputHumen;
            this.mainWindow = mainWindow;
            mainWindow.MethodName = AlgorhytmOutPut.algName;
            this.top = 0;
        }

        public Tuple<List<ItemPrimitive>,int> DenseFunction()
        {
            this.NeededIndex = Array.IndexOf(inputHumen[0].Normalized, inputHumen[0].Normalized.FirstOrDefault(x => x.compoundName == compoundString));
            List<ItemPrimitive> itemPrimitives = new List<ItemPrimitive>();
            for(int i = 0; i < analizedTuples[NeededIndex].groupedArray.Length; i++)
            {
                if(top < analizedTuples[NeededIndex].groupedArray[i])
                {
                    top = analizedTuples[NeededIndex].groupedArray[i];
                }
                ItemPrimitive itemPrimitive = new ItemPrimitive(analizedTuples[NeededIndex].groupedArray[i], (double)(i + 1) / analizedTuples[NeededIndex].groupedArray.Length);
                itemPrimitives.Add(itemPrimitive);
            }
            return new Tuple<List<ItemPrimitive>,int>(itemPrimitives, top + 3);
        }

        public List<ItemPrimitive>[] GroupFunction()
        {
            List<ItemPrimitive>[] arrayOfItemPrimitives = new List<ItemPrimitive>[analizedTuples[NeededIndex].analizedGroups.Length];
            double min = analizedTuples[NeededIndex].min;
            double max = analizedTuples[NeededIndex].max;
            for (int i = 0; i < analizedTuples[NeededIndex].analizedGroups.Length; i++)
            {
                List<ItemPrimitive> itemPrimitives = new List<ItemPrimitive>();
                for (int h = 0; h < inputHumen.Length; h++)
                {
                    double thisAddress = (double)(h + 1) / analizedTuples[NeededIndex].groupedArray.Length;
                    double groupMin = (analizedTuples[NeededIndex].analizedGroups[i].min - min) / (max - min);
                    double groupMax = (analizedTuples[NeededIndex].analizedGroups[i].max - min) / (max - min);
                    ItemPrimitive itemPrimitive;
                    if (groupMax >= thisAddress && groupMin <= thisAddress)
                    {
                        itemPrimitive = new ItemPrimitive(top + 3, thisAddress);                        
                    }
                    else
                    {
                        itemPrimitive = new ItemPrimitive(0, thisAddress);
                    }
                    itemPrimitives.Add(itemPrimitive);
                }
                arrayOfItemPrimitives[i] = itemPrimitives;
            }            
            return arrayOfItemPrimitives;
        }

        public List<ItemPrimitive> MedianaFunction()
        {
            double min = analizedTuples[NeededIndex].min;
            double max = analizedTuples[NeededIndex].max;
            List<ItemPrimitive> itemPrimitives = new List<ItemPrimitive>();
            for (int i = 0; i < analizedTuples[NeededIndex].analizedGroups.Length; i++)
            {
                double GroupMediana = (analizedTuples[NeededIndex].analizedGroups[i].mediana - min) / (max - min);
                ItemPrimitive itemPrimitive = new ItemPrimitive(top + 3, GroupMediana);
                itemPrimitives.Add(itemPrimitive);
            }
            return itemPrimitives;
        }

        public class ItemPrimitive
        {
            public int CountInPrimitive { get; set; }
            public double AddressOfPrimitive { get; set; }
            public ItemPrimitive(int CountInPrimitive, double AddressOfPrimitive)
            {
                this.AddressOfPrimitive = AddressOfPrimitive;
                this.CountInPrimitive = CountInPrimitive;
            }
        }
    }
}