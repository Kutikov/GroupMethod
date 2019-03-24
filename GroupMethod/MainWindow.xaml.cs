using Microsoft.Win32;
using Syncfusion.UI.Xaml.CellGrid.Helpers;
using Syncfusion.UI.Xaml.Spreadsheet.Helpers;
using Syncfusion.XlsIO.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Drawing;
using System.IO;
using Syncfusion.UI.Xaml.Charts;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace GroupMethod
{
    public partial class MainWindow : Window
    {
        string[] ColArgs = new string[4] { "ignore", "names", "normal item", "ranked item" };
        public string MethodName;
        List<string> NormalizedItems;
        List<string> RankedItems;
        System.Drawing.Color[] colors = new System.Drawing.Color[4] {
            System.Drawing.Color.FromArgb(255, 180, 180, 180), System.Drawing.Color.FromArgb(255, 159, 168, 218),
            System.Drawing.Color.FromArgb(255, 206, 147, 216), System.Drawing.Color.FromArgb(255, 239, 154, 154) };

        public MainWindow()
        {
            InitializeComponent();
            InputSpread.WorkbookLoaded += InputSpread_WorkbookLoaded;
            InputSpread.WorksheetAdded += InputSpread_WorksheetAdded;
            Vars.DropDownClosed += Vars_DropDownClosed;
            ShowGroups.Click += ShowGroups_Click;
        }

        private void ShowGroups_Click(object sender, RoutedEventArgs e)
        {
            if (Vars.SelectedItem != null)
            {
                RenderGraphic(Vars.SelectedItem.ToString(), (bool)ShowGroups.IsChecked);
            }
        }

        public void UpdateStats(string stat)
        {
            Analized.Text = stat;
        }

        private void Vars_DropDownClosed(object sender, EventArgs e)
        {
            if (Vars.SelectedItem != null)
            {
                RenderGraphic(Vars.SelectedItem.ToString(), (bool)ShowGroups.IsChecked);
            }
        }

        private void InputSpread_WorkbookLoaded(object sender, WorkbookLoadedEventArgs args)
        {
            string sheetName;
            for (int sheets = 0; sheets < InputSpread.Workbook.Worksheets.Count; sheets++)
            {
                sheetName = InputSpread.Workbook.Worksheets[sheets].Name;
                int adopted = 0;
                for (int col0 = 1; col0 < InputSpread.ActiveSheet.UsedRange.LastColumn + 1; col0++)
                {
                    if (adopted == 0)
                    {
                        for (int v = 0; v < 4; v++)
                        {
                            if (InputSpread.ActiveSheet.Range[2, col0].Value.ToString() == ColArgs[v])
                            {
                                adopted = 1;
                            }
                        }
                    }
                    else
                    {
                        for (int col4 = 1; col4 < InputSpread.ActiveSheet.UsedRange.LastColumn + 1; col4++)
                        {
                            InputSpread.ActiveSheet.Range[2, col4].CellStyle.Color = colors[Int32.Parse(InputSpread.ActiveSheet.Range[1, col4].Value)];
                            InputSpread.ActiveSheet.Range[2, col4].CellStyle.Locked = true;
                            InputSpread.ActiveGrid.InvalidateCell(2, col4);
                        }
                        break;
                    }
                }
                if (adopted == 0)
                {
                    InputSpread.SetActiveSheet(sheetName);
                    InputSpread.ActiveSheet.InsertRow(1, 2);
                    int lastcol = InputSpread.ActiveSheet.UsedRange.LastColumn + 1;
                    InputSpread.ActiveGrid.FrozenRows = 3;
                    for (int col = 1; col < lastcol; col++)
                    {
                        InputSpread.ActiveSheet.Range[1, col].Value = "0";
                        InputSpread.ActiveSheet.Range[2, col].Value = ColArgs[Int32.Parse(InputSpread.ActiveSheet.Range[1, col].Value)];
                        InputSpread.ActiveSheet.Range[2, col].CellStyle.Color = colors[Int32.Parse(InputSpread.ActiveSheet.Range[1, col].Value)];
                        InputSpread.ActiveSheet.Range[2, col].CellStyle.Locked = true;
                    }
                }
                InputSpread.ActiveSheet.CellValueChanged += ActiveSheet_CellValueChanged;
                InputSpread.ActiveGrid.Model.ColumnsInserted += Model_ColumnsInserted;
            }
        }

        private void InputSpread_WorksheetAdded(object sender, Syncfusion.UI.Xaml.Spreadsheet.Helpers.WorksheetAddedEventArgs args)
        {
            string sheetName = args.SheetName;
            InputSpread.SetActiveSheet(sheetName);
            InputSpread.ActiveSheet.InsertRow(1, 2);
            InputSpread.ActiveGrid.FrozenRows = 3;
            for (int col = 1; col < InputSpread.ActiveGrid.ColumnCount + 1; col++)
            {
                InputSpread.ActiveSheet.Range[1, col].Value = "0";
                InputSpread.ActiveSheet.Range[2, col].Value = ColArgs[Int32.Parse(InputSpread.ActiveSheet.Range[1, col].Value)];
                InputSpread.ActiveSheet.Range[2, col].CellStyle.Color = colors[Int32.Parse(InputSpread.ActiveSheet.Range[1, col].Value)];
                InputSpread.ActiveSheet.Range[2, col].CellStyle.Locked = true;
            }
            InputSpread.ActiveSheet.CellValueChanged += ActiveSheet_CellValueChanged;
            InputSpread.ActiveGrid.Model.ColumnsInserted += Model_ColumnsInserted;
        }

        private void ActiveSheet_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Range.Row == 1)
            {
                int col = e.Range.Column;
                if (e.NewValue.ToString() != "" && e.OldValue.ToString() != "" && Int32.Parse(e.NewValue.ToString()) < 4)
                {
                    InputSpread.ActiveSheet.Range[2, col].Value = ColArgs[Int32.Parse(e.NewValue.ToString())];
                    InputSpread.ActiveSheet.Range[2, col].CellStyle.Color = colors[Int32.Parse(e.NewValue.ToString())];
                }
                else
                {
                    InputSpread.ActiveSheet.Range[2, col].Value = ColArgs[0];
                }
                InputSpread.ActiveGrid.InvalidateCell(2, col);
            }
        }

        private void Model_ColumnsInserted(object sender, GridRangeInsertedEventArgs e)
        {
            int col = e.InsertAt;
            InputSpread.ActiveSheet.Range[1, col].Value = "0";
            InputSpread.ActiveSheet.Range[2, col].Value = ColArgs[Int32.Parse(InputSpread.ActiveSheet.Range[1, col].Value)];
            InputSpread.ActiveSheet.Range[2, col].CellStyle.Color = colors[Int32.Parse(InputSpread.ActiveSheet.Range[1, col].Value)];
            InputSpread.ActiveSheet.Range[2, col].CellStyle.Locked = true;
        }

        public Task<string> InputWorker(double Upper, double Entropy)
        {
            NormalizedItems = new List<string>();
            RankedItems = new List<string>();
            int startrow = int.TryParse(RowBox.Text, out int res) ? res : 0;
            int LastRow = int.TryParse(RowBoxEnd.Text, out int res2) ? res2 : 0;
            List<string> rangeSymbolsList = new List<string>();
            if (startrow > 0 && LastRow > startrow)
            {
                int LastCol = InputSpread.ActiveSheet.UsedRange.LastColumn;
                Vars.Items.Clear();
                Ranger.Items.Clear();
                List<Objects.InputHuman> inputHumenList = new List<Objects.InputHuman>();
                for (int row = startrow; row < LastRow + 1; row++)
                {
                    List<Objects.Names> namesList = new List<Objects.Names>();
                    List<Objects.Normalized> normalizedsList = new List<Objects.Normalized>();
                    List<Objects.Range> rangesList = new List<Objects.Range>();
                    for (int column = 1; column < LastCol + 1; column++)
                    {
                        List<string> ColNamesList = new List<string>();
                        for (int pseudoRow = 3; pseudoRow < startrow; pseudoRow++)
                        {
                            ColNamesList.Add(InputSpread.ActiveSheet.Range[pseudoRow, column].Value.ToString());
                        }
                        string[] ColNamesArr = new string[ColNamesList.Count];
                        ColNamesArr = ColNamesList.ToArray();
                        string compoundName = GetCompoundCaption(ColNamesArr);
                        int typeIndex = Array.IndexOf(ColArgs, ColArgs.FirstOrDefault(x => x == InputSpread.ActiveSheet.Range[2, column].Value.ToString()));
                        switch (typeIndex)
                        {
                            case 1:
                                namesList.Add(new Objects.Names(column, ColNamesArr, InputSpread.ActiveSheet.Range[row, column].Value.ToString()));
                                break;
                            case 2:
                                double value = 0;
                                if (Double.TryParse(InputSpread.ActiveSheet.Range[row, column].Value.ToString(), out double dbl) == true)
                                {
                                    value = dbl;
                                }
                                if (row == startrow)
                                {
                                    Vars.Items.Add(compoundName);
                                    NormalizedItems.Add(compoundName);
                                    Ranger.Items.Add(compoundName);
                                }
                                normalizedsList.Add(new Objects.Normalized(column, ColNamesArr, value, compoundName));
                                break;
                            case 3:
                                if (row == startrow)
                                {
                                    for (int pseudoRow = startrow; pseudoRow < LastRow + 1; pseudoRow++)
                                    {
                                        if (!rangeSymbolsList.Any(x => x == InputSpread.ActiveSheet.Range[pseudoRow, column].Value.ToString()))
                                        {
                                            rangeSymbolsList.Add(InputSpread.ActiveSheet.Range[pseudoRow, column].Value.ToString());
                                        }
                                    }
                                }
                                if (row == startrow)
                                {
                                    Ranger.Items.Add(compoundName);
                                    RankedItems.Add(compoundName);
                                }
                                string[] rangeSymbolsArray = new string[rangeSymbolsList.Count];
                                rangeSymbolsArray = rangeSymbolsList.ToArray();
                                string current = InputSpread.ActiveSheet.Range[row, column].Value.ToString();
                                int rangeIndex = Array.IndexOf(rangeSymbolsArray, rangeSymbolsArray.FirstOrDefault(x => x == current));
                                rangesList.Add(new Objects.Range(0, rangeSymbolsArray.Length, column, ColNamesArr, rangeSymbolsArray, rangeIndex, compoundName));
                                break;
                        }
                    }
                    Objects.Names[] namesArray = new Objects.Names[namesList.Count];
                    Objects.Normalized[] normalizedsArray = new Objects.Normalized[normalizedsList.Count];
                    Objects.NormalizedRange[] normalizedsRangeArray = new Objects.NormalizedRange[normalizedsList.Count];
                    Objects.Range[] rangesArray = new Objects.Range[rangesList.Count];
                    namesArray = namesList.ToArray();
                    normalizedsArray = normalizedsList.ToArray();
                    rangesArray = rangesList.ToArray();
                    inputHumenList.Add(new Objects.InputHuman(row, row.ToString())
                    {
                        Names = namesArray,
                        Normalized = normalizedsArray,
                        Ranges = rangesArray,
                        NormalizedRanges = normalizedsRangeArray
                    });
                }
                Objects.InputHuman[] inputHumenArray = new Objects.InputHuman[inputHumenList.Count];
                inputHumenArray = inputHumenList.ToArray();
                double DispersiaDivideIndex = 2;
                if (Double.TryParse(Dispers.Text, out double dbl2))
                {
                    if (dbl2 > 0)
                    {
                        DispersiaDivideIndex = dbl2;
                    }
                }
                string method = "Dense Adsorbes Undense";
                Entropy = 0;
                if (AlgorhythChooser.SelectedItem == null || AlgorhythChooser.SelectedItem != DAU)
                {
                    method = "Distant Becomes Last";
                    Entropy = 1;
                }
                return Task.Run(() =>
                {
                    Collector collector = new Collector(inputHumenArray, this, method, DispersiaDivideIndex);
                    collector.DataGetter(Upper, Entropy, 0, 0, false);
                    return "ok";
                });
            }
            else {
                return Task.Run(() =>
                {
                    return "fail";
                });
            }                       
        }

        public void ResultsBuilder(double Enclosing, int sortBy, bool isRange)
        {
            ExcelBuilder excelBuilder = new ExcelBuilder(this);
            var excel = excelBuilder.Build(Enclosing, sortBy, isRange);
            string[][][] excelStringTemplate = excel.Item1;
            System.Drawing.Color[][][] excelColorTemplate = excel.Item2;
            OutPutSpread.Create(excelStringTemplate.Length);
            OutPutSpread.Workbook.Worksheets[0].Name = "Начальные данные";
            OutPutSpread.Workbook.Worksheets[1].Name = "Группы";
            OutPutSpread.Workbook.Worksheets[2].Name = "Ранжированные данные";
            OutPutSpread.Workbook.Worksheets[3].Name = "Корреляция Спирмена";
            for (int sheet = 0; sheet < excelStringTemplate.Length; sheet++)
            {
                if(excelStringTemplate[sheet] != null)
                {
                    for (int column = 0; column < excelStringTemplate[sheet].Length; column++)
                    {
                        for (int row = 0; row < excelStringTemplate[sheet][column].Length; row++)
                        {
                            OutPutSpread.Workbook.Worksheets[sheet].Range[row + 1, column + 1].Value = excelStringTemplate[sheet][column][row];
                        }
                    }
                }
                if(excelColorTemplate[sheet] != null)
                {
                    for (int column = 0; column < excelColorTemplate[sheet].Length; column++)
                    {
                        for (int row = 0; row < excelColorTemplate[sheet][column].Length; row++)
                        {
                            if (excelColorTemplate[sheet][column][row].Name != "0")
                            {
                                OutPutSpread.Workbook.Worksheets[sheet].Range[row + 1, column + 1].CellStyle.Color = excelColorTemplate[sheet][column][row];
                            }                            
                        }
                    }
                }
            }
        }

        private void SaveExcelResult_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = InputSpread.FileName + " " + MethodName,
                DefaultExt = ".xlsx",
                Filter = "Excel (.xlsx)|*.xlsx"
            };
            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string FileNameBase = saveFileDialog.FileName;
                try
                {
                    OutPutSpread.SaveAs(FileNameBase);
                }
                catch (IOException e2)
                {
                    e2.StackTrace.ToString();
                }
            }
        }

        private void OpenExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true)
            {
                InputSpread.Open(openFileDialog.FileName);
            }
        }

        private void SaveExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "Source",
                DefaultExt = ".xlsx",
                Filter = "Excel (.xlsx)|*.xlsx"
            };
            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string FileNameBase = saveFileDialog.FileName;
                try
                {
                    InputSpread.SaveAs(FileNameBase);
                }
                catch (IOException e2)
                {
                    e2.StackTrace.ToString();
                }
            }
        }

        async private void EnterResult_Click(object sender, RoutedEventArgs e)
        {
            Analized.Text = "Обработка...";
            if (await InputWorker(15.0, 0) == "ok")
            {
                ResultsBuilder(1, 0, false);
                Analized.Text = "Готово!";
            }
            else
            {
                Analized.Text = "Что-то не так...";
            }
        }

        public string GetCompoundCaption(string[] captions)
        {
            string result = "";
            for(int i = 0; i < captions.Length; i++)
            {
                if(captions[i] != "")
                {
                    result = result + captions[i] + " | ";
                }
            }
            if(result == "")
            {
                result = "undefined   ";
            }
            return result.Substring(0, result.Length - 3);
        }

        private void RecalcResult_Click(object sender, RoutedEventArgs e)
        {
            double Enclosing = EncloseSlider.Value;
            bool isRange = false;
            int sortBy = 0;
            if(Ranger.SelectedItem != null)
            {
                sortBy = NormalizedItems.FindIndex(x => x == Ranger.SelectedItem.ToString());
                if (sortBy == -1)
                {
                    sortBy = RankedItems.FindIndex(x => x == Ranger.SelectedItem.ToString());
                    isRange = true;
                }
            }            
            ResultsBuilder(Enclosing, sortBy, isRange);
        }

        public void RenderGraphic(string CompoundString, bool showGroups)
        {
            VariantChart.Series.Clear();
            Graphics graphics = new Graphics(CompoundString, this);
            var denseArr = graphics.DenseFunction();
            
            ViewModel denseViewModel = new ViewModel(denseArr.Item1);           
            GraphicTop.Text = CompoundString;
            NumericalAxis primary = new NumericalAxis()
            {
                Header = "Нормализованные величины",
                Maximum = 1.01
            };
            NumericalAxis secondary = new NumericalAxis()
            {
                Header = "Плотность величины",
                Maximum = denseArr.Item2
            };
            VariantChart.PrimaryAxis = primary;
            VariantChart.SecondaryAxis = secondary;
            SplineAreaSeries denseFunction = new SplineAreaSeries()
            {
                ItemsSource = denseViewModel.Data,
                XBindingPath = "AddressOfPrimitive",
                YBindingPath = "CountInPrimitive",
                SplineType = SplineType.Monotonic,
                Interior = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0))
            };
            if (showGroups)
            {
                List<Graphics.ItemPrimitive>[] groups = graphics.GroupFunction();
                for (int i = 0; i < groups.Length; i++)
                {
                    SolidColorBrush solidColorBrush;
                    if (i % 2 == 0)
                    {
                        solidColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 239, 154, 154));
                    }
                    else
                    {
                        solidColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 63, 81, 181));
                    }
                    ViewModel groupViewModel = new ViewModel(groups[i]);
                    StepAreaSeries groupsFunction = new StepAreaSeries()
                    {
                        ItemsSource = groupViewModel.Data,
                        XBindingPath = "AddressOfPrimitive",
                        YBindingPath = "CountInPrimitive",
                        Interior = solidColorBrush
                    };
                    VariantChart.Series.Add(groupsFunction);
                }
                ViewModel medianas = new ViewModel(graphics.MedianaFunction());
                ColumnSeries medianasFunction = new ColumnSeries()
                {
                    ItemsSource = medianas.Data,
                    XBindingPath = "AddressOfPrimitive",
                    YBindingPath = "CountInPrimitive",
                    Interior = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0)),
                    SegmentSpacing = 0.1
                };
                //VariantChart.Series.Add(medianasFunction);
            }            
            VariantChart.Series.Add(denseFunction);
        }

        private void RenderGraph_Click(object sender, RoutedEventArgs e)
        {
            string Filename = "Graphic";
            if(Vars.SelectedItem != null)
            {
                Filename = Vars.SelectedItem.ToString() + " " + MethodName;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = Filename,
                DefaultExt = ".png",
                Filter = "Image (.png)|*.png"
            };
            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string FileNameBase = saveFileDialog.FileName;
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                    (int)ImageHolder.ActualWidth, (int)ImageHolder.ActualHeight + 40, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(ImageHolder);
                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                MemoryStream stream = new MemoryStream();
                png.Save(stream);
                Image image = Image.FromStream(stream);
                image.Save(FileNameBase);
            }            
        }

        private class ViewModel
        {
            public List<Graphics.ItemPrimitive> Data { get; set; }
            public ViewModel(List<Graphics.ItemPrimitive> Data)
            {
                this.Data = Data;
            }
        }
    }
}
