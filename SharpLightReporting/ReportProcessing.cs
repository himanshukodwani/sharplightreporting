using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpreadsheetLight;
using SpreadsheetLight.Drawing;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        private Bounds CurrentReportBounds = null;
        private IReportModel _reportModelData;
        public SLDocument Document;
        private string CurrentSheet;
        private int CurrentColumn = 0;
        private int CurrentRow = 0;
        private List<int> RowsToRemove = new List<int>();
        private List<int> ColsToRemove = new List<int>();
        private List<ReportPictureItem> ReportPictures = new List<ReportPictureItem>();
        private List<ReportChartItem> ReportCharts = new List<ReportChartItem>();
        private List<PageBreakItems> PageBreaks = new List<PageBreakItems>();
        private List<RowsInsertedItem> RowsInserted = new List<RowsInsertedItem>();
        private List<RowsShiftedItem> RowsShifted = new List<RowsShiftedItem>();
        private List<ColsInsertedItem> ColsInseted = new List<ColsInsertedItem>();
        private List<ColsShiftedItem> ColsShifted = new List<ColsShiftedItem>();
        private string DestinationFile = "Temp.xlsx";

        public delegate void ChartCreatedDelegate(ReportChartItem reportChartItem);

        public event ChartCreatedDelegate ChartCreatedEvent;

        public delegate void CustomTagDelegate(SLDocument workSheet, int calledFromRow, int calledFromCol, string tagName, List<StringKeyValue> tagParams);

        public event CustomTagDelegate CustomTagFound;

        public delegate void NotifyReportLogDelegate(string log);

        public event NotifyReportLogDelegate NotifyReportLogEvent;

        public void ProcessReport(string TemplateFile, string destinationFile, IReportModel Data)
        {
            Document = new SLDocument(TemplateFile);
            DestinationFile = destinationFile;
            SubProcess(Data);
            Document.SaveAs(DestinationFile);
        }

        public void ProcessReport(string TemplateFile, Stream OutputStream, IReportModel Data)
        {
            Document = new SLDocument(TemplateFile);
            SubProcess(Data);
            Document.SaveAs(OutputStream);
        }

        public void ProcessReport(Stream TemplateFileStream, Stream OutputStream, IReportModel Data)
        {
            Document = new SLDocument(TemplateFileStream);
            SubProcess(Data);
            Document.SaveAs(OutputStream);
        }

        public void ProcessReport(Stream TemplateFileStream, string destinationFile, IReportModel Data)
        {
            DestinationFile = destinationFile;
            Document = new SLDocument(TemplateFileStream);
            SubProcess(Data);
            Document.SaveAs(DestinationFile);
        }

        private void SubProcess(IReportModel Data)
        {
            _reportModelData = Data;
            //1) Go through each sheet
            foreach (var sheetName in Document.GetSheetNames())
            {
                CurrentSheet = sheetName;
                Document.SelectWorksheet(sheetName);
                if (HasReportDefinition())
                {
                    this.RowsToRemove.Clear();
                    this.ColsToRemove.Clear();
                    this.ColsInseted.Clear();
                    this.RowsInserted.Clear();
                    this.ColsShifted.Clear();
                    this.RowsShifted.Clear();
                    this.ReportPictures.Clear();
                    this.ReportCharts.Clear();
                    this.PageBreaks.Clear();
                    this.CurrentColumn = 1;
                    this.CurrentRow = 1;
                    CurrentSheet = sheetName;
                    //2) Set template bounds from definition
                    CurrentReportBounds = null;
                    CurrentReportBounds = GetReportTemplateBounds();
                    AddRowsToRemove(1);

                    EnumerateReport();

                    //4) Now call EnumerateReport and Go through each cell in template
                    //5) Get cell data replace dynamic data with values from ReportData class

                    //foreach (var item in this.RowsInserted)
                    //{
                    //    //Picture, Charts, And Pagebreaks defined next will have their position autocorrected since they have not been parsed yet. Previously defined objects will have no effect since row is added beneath. Only charts data need to be corrected
                    //   // AdjustChartDataPostionOnRowInserted(item.InsertedAt, item.InsertedCount);
                    //}

                    int rowsRemoveCount = 0;
                    RowsToRemove.Sort();

                    foreach (int row in RowsToRemove)
                    {
                        //////////////////////////////////////////////////////////////////////  - Removing row and setting height
                        List<PairedValues<int, double>> rowsandHeights = new List<PairedValues<int, double>>();

                        //Get all next rows height, delete the row which needs to be deleted, set replacing row height to it's actual height

                        for (int i = row; i <= CurrentReportBounds.Bottom; i++)
                        {
                            //get next row height and set it at what index it will be after row is removed
                            double nextrowheight = Document.GetRowHeight(i - rowsRemoveCount + 1);
                            //Note we are not adding +1 to row index as row index after removal of previous row will be one less then it's current index
                            rowsandHeights.Add(new PairedValues<int, double>(i - rowsRemoveCount, nextrowheight));
                        }

                        //Delete One Row
                        Document.DeleteRow(row - rowsRemoveCount, 1);

                        //Now all next rows have got their index one less then their previous index so it will now match the indexes as saved in list of rowsandheight
                        foreach (var rowsandHeight in rowsandHeights)
                        {
                            Document.SetRowHeight(rowsandHeight.ValueA, rowsandHeight.ValueB);
                        }
                        /////////////////////////////////////////////////////////////////////////

                        this.CurrentReportBounds.Bottom -= 1;
                        AdjustPicturePositionOnRowRemoved(row - rowsRemoveCount, 1);
                        AdjustChartPositionOnRowRemoved(row - rowsRemoveCount, 1);
                        AdjustPageBreaksOnRowsRemoved(row - rowsRemoveCount, 1);
                        AdjustRowInsertedListOnRowRemoval((row - rowsRemoveCount), 1);
                        rowsRemoveCount++;
                    }
                    foreach (var item in this.ColsInseted)
                    {
                        //This will effect all previously defined objects since pagebreaks and pics defined previously need to be repostioned as the were already parsed. Also charts data position which will be defined next will have to be checked and corrected.
                        //AdjustChartPositionOnColumnInserted(item.InsertedAt, item.InsertedCount);
                        AdjustPicturePositionOnColInsert(item.InsertedAt, item.InsertedCount);
                        AdjustPageBreakOnColumnAdded(item.InsertedAt, item.InsertedCount);
                    }
                    int colsRemoveCount = 0;
                    ColsToRemove.Sort();
                    foreach (int col in ColsToRemove)
                    {
                        //////////////////////////////////////////////////////////////////////////////////

                        List<PairedValues<int, double>> colsandWidths = new List<PairedValues<int, double>>();
                        //Get next cols width, remove the col that needs to be removed, set replacing columns width
                        for (int i = col; i <= CurrentReportBounds.Right; i++)
                        {
                            double nextcolwidth = Document.GetColumnWidth(i - colsRemoveCount + 1);
                            colsandWidths.Add(new PairedValues<int, double>(i - colsRemoveCount, nextcolwidth));
                        }

                        ///Remove one column
                        Document.DeleteColumn(col - colsRemoveCount, 1);
                        ///Each column next to the one removed should have their width reset to their previous width
                        foreach (var colsandWidth in colsandWidths)
                        {
                            Document.SetColumnWidth(colsandWidth.ValueA, colsandWidth.ValueB);
                        }
                        ////////////////////////////////////////////////////////////////////////////////////

                        this.CurrentReportBounds.Right -= 1;
                        AdjustPicturePositionOnColumnRemoved(col - colsRemoveCount, 1);
                        AdjustChartPositionOnColumnRemoved(col - colsRemoveCount, 1);
                        AdjustPageBreaksOnColumnRemoved(col - colsRemoveCount, 1);
                        AdjustColInsertedListOnColRemoval(col - colsRemoveCount, 1);
                        colsRemoveCount++;
                    }

                    PutAllPicturesOnReport();
                    PutAllChartsOnReport();
                    PutAllPageBreaksOnReport();
                }
            }
        }

        private void AddRowsToRemove(int rowNumber)
        {
            bool found = false;
            foreach (int i in RowsToRemove)
            {
                if (i == rowNumber)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                this.RowsToRemove.Add(rowNumber);
            }
        }

        private void AddColsToRemove(int colNumber)
        {
            bool found = false;
            foreach (int i in ColsToRemove)
            {
                if (i == colNumber)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                this.ColsToRemove.Add(colNumber);
            }
        }

        public Bounds GetReportBounds()
        {
            return CurrentReportBounds;
        }

        private void EnumerateReport()
        {
            CurrentRow = 0;
            CurrentColumn = 0;
            while (MoveNextRow())
            {
                while (MoveNextColumn())
                {
                    string originalText = Document.GetCellValueAsString(CurrentRow, CurrentColumn);
                    //Process variable and methods *********
                    //While a cell is having both variables and methods process them in sequence they are defined
                    while (HasVariable(originalText) && HasMethodDefinition(originalText))
                    {
                        int variableIndex = originalText.ToLower().Replace(" ", "").IndexOf("<variable");
                        int methodIndex = originalText.ToLower().Replace(" ", "").IndexOf("<method");
                        if (variableIndex < methodIndex)
                        {
                            //Process variable first next process method
                            originalText = ProcessVariableFound(originalText);

                            CallMethod(GetMethodName(originalText));
                            originalText = RemoveMethodDef(originalText);
                        }
                        else
                        {
                            //Process method first next variable
                            CallMethod(GetMethodName(originalText));
                            originalText = RemoveMethodDef(originalText);

                            originalText = ProcessVariableFound(originalText);
                        }
                    }
                    //if cell is having only methods but no variables, call alll methods
                    while (HasMethodDefinition(originalText))
                    {
                        CallMethod(GetMethodName(originalText));
                        originalText = RemoveMethodDef(originalText);
                    }

                    //If not a picture cell
                    //Process all variables if cell is not having any methods but variables only
                    string val = "";
                    val = ReplaceVriablesWithValue(originalText);
                    if (HasCustomTag(val))
                    {
                        val = CallCustomTags(val);
                    }
                    SetCellValuesAsPerFormatDefined(CurrentRow, CurrentColumn, val);

                    //********* Completed processing variables and methods

                    //Start processing repeaters
                    if (HasVertRepeaterDefined(val))
                    {
                        RepeatMode repeatMode = RepeatMode.insert;
                        AddRowsToRemove(CurrentRow);
                        int count = 0;
                        Bounds vertRepeatBounds = GetVertRepeaterBounds(val);
                        count = int.Parse(GetVertRepeatFrequency(val));
                        repeatMode = GetVertRepeatMode(val);
                        StartVertRepeat(vertRepeatBounds, count, repeatMode);
                    }

                    if (HasHorizRepeaterDefined(val))
                    {
                        RepeatMode repeatMode = GetHorizRepeatMode(val);
                        AddRowsToRemove(CurrentRow);
                        // AddColsToRemove(CurrentColumn);
                        int count = 0;
                        count = int.Parse(GetHorizRepeatFrequency(val));
                        Bounds horizRepeatBounds = GetHorizRepeaterBounds(val/*, out count, out repeatMode*/);
                        StartHorizRepeat(horizRepeatBounds, count, repeatMode);
                    }
                    while (HasChartDefinition(val))
                    {
                        ProcessChart(val);
                        int chartDefStart = val.ToLower().IndexOf(@"<chart");
                        int chartDefEnd = val.ToLower().IndexOf(@"/>", chartDefStart) + 2;
                        val = val.Remove(chartDefStart, chartDefEnd - chartDefStart);
                        Document.SetCellValue(CurrentRow, CurrentColumn, val);
                    }
                    while (HasPictureDefinition(val))
                    {
                        ProcessPicture(val);
                        int picDefStart = val.ToLower().IndexOf(@"<pic");
                        int picDefEnd = val.ToLower().IndexOf(@"/>", picDefStart) + 2;
                        val = val.Remove(picDefStart, picDefEnd - picDefStart);
                        Document.SetCellValue(CurrentRow, CurrentColumn, val);
                    }
                    //Remove Marked Rows - ?
                    if (IsRowDeleted(val))
                    {
                        this.AddRowsToRemove(CurrentRow);
                    }
                    // Remove Marked Cols -?
                    if (IsColumnDeleted(val))
                    {
                        this.AddColsToRemove(CurrentColumn);
                    }
                    if (HasPageBreak(val))
                    {
                        // this.AddRowsToRemove(CurrentRow);
                        // this.AddColsToRemove(CurrentColumn);
                        this.PageBreaks.Add(new PageBreakItems(CurrentRow, CurrentColumn));
                        val = RemovePageBreakDef(val);
                        Document.SetCellValue(CurrentRow, CurrentColumn, val);
                    }
                }
            }
        }

        private bool HasReportDefinition()
        {
            SLDocument doc = Document;
            if (doc.SelectWorksheet(CurrentSheet))
            {
                string firstCellvalue = doc.GetCellValueAsString(1, 1).ToLower().Trim();

                if (firstCellvalue.StartsWith(@"<report") && firstCellvalue.EndsWith(@"/>"))
                {
                    firstCellvalue = firstCellvalue.Remove(0, 7);
                    firstCellvalue = firstCellvalue.Remove(firstCellvalue.Length - 2, 2);
                    firstCellvalue.Replace(" ", "");
                    string[] bounds = firstCellvalue.Split(new char[] { ',' }, firstCellvalue.Length);
                    bool leftFound = false, rightFound = false, topFound = false, bottomFound = false;
                    foreach (var bound in bounds)
                    {
                        if (bound.ToLower().Trim().StartsWith("left"))
                        {
                            try
                            {
                                int.Parse(bound.ToLower().Trim().Replace("left=", ""));
                                leftFound = true;
                            }
                            catch
                            {
                            }

                            continue;
                        }
                        if (bound.ToLower().Trim().StartsWith("right"))
                        {
                            try
                            {
                                int.Parse(bound.ToLower().Trim().Replace("right=", ""));
                                rightFound = true;
                            }
                            catch
                            {
                            }
                            continue;
                        }
                        if (bound.ToLower().Trim().StartsWith("top"))
                        {
                            try
                            {
                                int.Parse(bound.ToLower().Trim().Replace("top=", ""));
                                topFound = true;
                            }
                            catch
                            {
                            }
                            continue;
                        }
                        if (bound.ToLower().Trim().StartsWith("bottom"))
                        {
                            try
                            {
                                int.Parse(bound.ToLower().Trim().Replace("bottom=", ""));
                                bottomFound = true;
                            }
                            catch
                            {
                            }
                        }
                    }
                    if (leftFound && rightFound && topFound && bottomFound)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }

            throw new Exception("Sheet not found");
        }

        private Bounds GetReportTemplateBounds()
        {
            Bounds repbounds = new Bounds();
            if (HasReportDefinition())
            {
                SLDocument doc = Document;
                if (doc.SelectWorksheet(CurrentSheet))
                {
                    string firstCellvalue = doc.GetCellValueAsString(1, 1).ToLower().Trim();
                    if (firstCellvalue.StartsWith(@"<report") && firstCellvalue.EndsWith(@"/>"))
                    {
                        firstCellvalue = firstCellvalue.Remove(0, 7);
                        firstCellvalue = firstCellvalue.Remove(firstCellvalue.Length - 2, 2);
                        firstCellvalue.Replace(" ", "");
                        string[] bounds = firstCellvalue.Split(new char[] { ',' }, firstCellvalue.Length);
                        bool leftFound = false, rightFound = false, topFound = false, bottomFound = false;
                        foreach (var bound in bounds)
                        {
                            if (bound.ToLower().Trim().StartsWith("left"))
                            {
                                repbounds.Left =
                                    int.Parse(bound.ToLower().Trim().Replace("left=", ""));
                                continue;
                            }
                            if (bound.ToLower().Trim().StartsWith("right"))
                            {
                                repbounds.Right =
                                    int.Parse(bound.ToLower().Trim().Replace("right=", ""));
                                continue;
                            }
                            if (bound.ToLower().Trim().StartsWith("top"))
                            {
                                repbounds.Top =
                                int.Parse(bound.ToLower().Trim().Replace("top=", ""));

                                continue;
                            }
                            if (bound.ToLower().Trim().StartsWith("bottom"))
                            {
                                repbounds.Bottom =
                                    int.Parse(bound.ToLower().Trim().Replace("bottom=", ""));
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Correct report definition in first cell not found");
            }
            return repbounds;
        }
    }

    public class PairedValues<TA, TB>
    {
        public PairedValues(TA valA, TB valB)
        {
            this.ValueA = valA;
            this.ValueB = valB;
        }

        public TA ValueA { get; set; }
        public TB ValueB { get; set; }
    }
}