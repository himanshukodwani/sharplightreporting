using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpreadsheetLight.Charts;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        public void AddChart(string chartName, double rowPos, double colPos, double chartHeight, double chartWidth)
        {
            this.ReportCharts.Add(new ReportChartItem()
            {

                ChartName = chartName,
                ChartColPosition = colPos,
                ChartRowPosition = rowPos,
                ChartHeight = chartHeight - 1,
                ChartWidth = chartWidth - 1
            });
        }

        private void PutAllChartsOnReport()
        {
            foreach (var chartItem in ReportCharts)
            {

                try
                {
                    chartItem.Chart = Document.CreateChart(chartItem.ChartDataRange_Top, chartItem.ChartDataRange_Left,
                                                                   chartItem.ChartDataRange_Bottom, chartItem.ChartDataRange_Right);

                    chartItem.Chart.SetChartPosition(chartItem.ChartRowPosition - 1 + chartItem.AddToRowPos, chartItem.ChartColPosition - 1 + chartItem.AddToColPos,
                                                     chartItem.ChartRowPosition - 1 + chartItem.ChartHeight + chartItem.AddToRowPos,
                                                     chartItem.ChartColPosition - 1 + chartItem.ChartWidth + chartItem.AddToColPos);


                    chartItem.AddChartTypeToChart();
                    chartItem.AddChartStyleToChart();
                }
                catch
                {

                    NotifyReportLogEvent("Could not create chart");
                    return;
                }
                try
                {
                    Document.InsertChart(chartItem.Chart);
                }
                catch
                {

                    NotifyReportLogEvent("Could not insert chart into the spreadsheet. ");
                    return;
                }

            }
        }

        private void AdjustChartPositionOnRowRemoved(int rowRemovedAt, int noOfRowsRemoved)
        {

            foreach (var chart in ReportCharts)
            {

                if (chart.ChartRowPosition >= rowRemovedAt)
                {

                    chart.ChartRowPosition = chart.ChartRowPosition - noOfRowsRemoved;

                }
                if (chart.ChartDataRange_Top >= rowRemovedAt)
                {
                    chart.ChartDataRange_Top = chart.ChartDataRange_Top - noOfRowsRemoved;
                }
                if (chart.ChartDataRange_Bottom >= rowRemovedAt)
                {
                    chart.ChartDataRange_Bottom = chart.ChartDataRange_Bottom - noOfRowsRemoved;
                }
            }
        }

        private void AdjustChartPositionOnColumnRemoved(int colRemovedAt, int noOfCosRemoved)
        {
            foreach (var chart in ReportCharts)
            {
                if (chart.ChartColPosition >= colRemovedAt)
                {
                    chart.ChartColPosition = chart.ChartColPosition - noOfCosRemoved;
                }
                if (chart.ChartDataRange_Left >= colRemovedAt)
                {
                    chart.ChartDataRange_Left = chart.ChartDataRange_Left - noOfCosRemoved;
                }
                if (chart.ChartDataRange_Right >= colRemovedAt)
                {
                    chart.ChartDataRange_Right = chart.ChartDataRange_Right - noOfCosRemoved;
                }

            }
        }

        private void AdjustChartDataPostionOnRowInserted(int rowInsertedAt, int noOfRowsInserted)
        {
            foreach (var chart in ReportCharts)
            {


                //if (chart.ChartRowPosition >= rowInsertedAt)
                //{

                //    chart.ChartRowPosition = chart.ChartRowPosition + noOfRowsInserted;


                //}
                if (chart.ChartDataRange_Top >= rowInsertedAt)
                {

                    chart.ChartDataRange_Top = chart.ChartDataRange_Top + noOfRowsInserted;

                }
                if (chart.ChartDataRange_Bottom >= rowInsertedAt)
                {

                    chart.ChartDataRange_Bottom = chart.ChartDataRange_Bottom + noOfRowsInserted;

                }
            }
        }

        private void AdjustChartPositionOnRowShift(int rowInserted, int noOfRowsInserted, int colLeft, int colRight)
        {
            foreach (var chart in ReportCharts)
            {
                if (chart.ChartRowPosition >= rowInserted &&
                    (chart.ChartColPosition >= colLeft && chart.ChartColPosition <= colRight))
                {
                    chart.ChartRowPosition = chart.ChartRowPosition + noOfRowsInserted;

                }
                if (chart.ChartDataRange_Top >= rowInserted &&
                    ((chart.ChartDataRange_Left >= colLeft && chart.ChartDataRange_Left <= colRight) ||
                     (chart.ChartDataRange_Right >= colLeft && chart.ChartDataRange_Right <= colRight)))
                {
                    chart.ChartDataRange_Top = chart.ChartDataRange_Top + noOfRowsInserted;
                }
                if (chart.ChartDataRange_Bottom >= rowInserted &&
                    ((chart.ChartDataRange_Left >= colLeft && chart.ChartDataRange_Left <= colRight) ||
                     (chart.ChartDataRange_Right >= colLeft && chart.ChartDataRange_Right <= colRight)))
                {
                    chart.ChartDataRange_Bottom = chart.ChartDataRange_Bottom + noOfRowsInserted;
                }
            }
        }

        private void AdjustChartPositionOnColShift(int colInsertAt, int noOfColsInserted, int rowTop, int rowBottom)
        {


            foreach (var chart in ReportCharts)
            {

                if (chart.ChartColPosition >= colInsertAt &&
                    (chart.ChartRowPosition >= rowTop && chart.ChartRowPosition <= rowBottom))
                {
                    chart.ChartColPosition = chart.ChartColPosition + noOfColsInserted;

                }
                if (chart.ChartDataRange_Left >= colInsertAt &&
                    ((chart.ChartDataRange_Top >= rowTop && chart.ChartDataRange_Top <= rowBottom) ||
                     (chart.ChartDataRange_Bottom >= rowTop && chart.ChartDataRange_Bottom <= rowBottom)))
                {

                    chart.ChartDataRange_Left = chart.ChartDataRange_Left + noOfColsInserted;



                }
                if (chart.ChartDataRange_Right >= colInsertAt &&
                    ((chart.ChartDataRange_Top >= rowTop && chart.ChartDataRange_Top <= rowBottom) ||
                     (chart.ChartDataRange_Bottom >= rowTop && chart.ChartDataRange_Bottom <= rowBottom)))
                {
                    chart.ChartDataRange_Right = chart.ChartDataRange_Right + noOfColsInserted;
                }
            }

        }

        private void AdjustChartPositionOnColumnInserted(int colInserted, int noOfColsInserted)
        {
            foreach (var chart in ReportCharts)
            {
                if (chart.ChartColPosition >= colInserted)
                {
                    chart.ChartColPosition = chart.ChartColPosition + noOfColsInserted;
                }
                if (chart.ChartDataRange_Left >= colInserted)
                {
                    chart.ChartDataRange_Left = chart.ChartDataRange_Left + noOfColsInserted;
                }
                if (chart.ChartDataRange_Right >= colInserted)
                {
                    chart.ChartDataRange_Right = chart.ChartDataRange_Right + noOfColsInserted;

                }

            }
        }

        private bool HasChartDefinition(string cellText)
        {
            if (cellText.Replace(" ", "").ToLower().Contains(@"<chart"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ProcessChart(string cellText)
        {
            try
            {
                if (HasChartDefinition(cellText))
                {
                    int chartDefStart = cellText.ToLower().IndexOf(@"<chart");
                    int chartDefEnd = cellText.ToLower().IndexOf(@"/>", chartDefStart);
                    string chartDefinition = cellText.Substring(chartDefStart, chartDefEnd - chartDefStart);
                    chartDefinition = chartDefinition.ToLower().Replace(@"<chart", "").Replace(@"/>", "");
                    string[] chartAtts = chartDefinition.Split(new char[] { ',' }, chartDefinition.Length);
                    ReportChartItem chartItem = new ReportChartItem();
                    chartItem.ChartRowPosition = CurrentRow;
                    chartItem.ChartColPosition = CurrentColumn;
                    foreach (string chartAtt in chartAtts)
                    {

                        try
                        {
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("name="))
                            {
                                chartItem.ChartName = chartAtt.ToLower().Replace(" ", "").Replace("name=", "");
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("top="))
                            {
                                chartItem.ChartDataRange_Top = int.Parse(chartAtt.ToLower().Replace(" ", "").Replace("top=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("left="))
                            {
                                chartItem.ChartDataRange_Left = int.Parse(chartAtt.ToLower().Replace(" ", "").Replace("left=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("right="))
                            {
                                chartItem.ChartDataRange_Right = int.Parse(chartAtt.ToLower().Replace(" ", "").Replace("right=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("bottom="))
                            {
                                chartItem.ChartDataRange_Bottom = int.Parse(chartAtt.ToLower().Replace(" ", "").Replace("bottom=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("height="))
                            {
                                chartItem.ChartHeight = double.Parse(chartAtt.ToLower().Replace(" ", "").Replace("height=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("width="))
                            {
                                chartItem.ChartWidth = double.Parse(chartAtt.ToLower().Replace(" ", "").Replace("width=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("style="))
                            {
                                chartItem.ChartStyle = int.Parse(chartAtt.ToLower().Replace(" ", "").Replace("style=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("type="))
                            {
                                chartItem.ChartType = chartAtt.ToLower().Replace(" ", "").Replace("type=", "");
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("addtorowpos="))
                            {
                                chartItem.AddToRowPos += double.Parse(chartAtt.ToLower().Replace(" ", "").Replace("addtorowpos=", ""));
                            }
                            if (chartAtt.ToLower().Replace(" ", "").StartsWith("addtocolpos="))
                            {
                                chartItem.AddToColPos += double.Parse(chartAtt.ToLower().Replace(" ", "").Replace("addtocolpos=", ""));
                            }
                        }
                        catch
                        {

                            NotifyReportLogEvent("Could not process chart attribute :" + chartAtt);
                            return;
                        }

                    }

                    this.ReportCharts.Add(chartItem);
                }
            }
            catch
            {

                NotifyReportLogEvent("Could not process the chart");
                return;
            }
        }
    }
    public class ReportChartItem
    {

        public ReportChartItem()
        {
            this.AddToColPos = 0.0;
            this.AddToRowPos = 0.0;
        }

        public string ChartName { get; set; }
        private SLChart _chart;
        public SLChart Chart
        {
            get { return this._chart; }
            set
            {
                this._chart = value;
                if (SetChartTypeAndStyle != null)
                {

                    SetChartTypeAndStyle(this._chart, this.ChartName);
                }
            }
        }
        public double ChartRowPosition { get; set; }
        public double ChartColPosition { get; set; }
        public double ChartHeight { get; set; }
        public double ChartWidth { get; set; }
        public int ChartDataRange_Top { get; set; }
        public int ChartDataRange_Bottom { get; set; }
        public int ChartDataRange_Left { get; set; }
        public int ChartDataRange_Right { get; set; }
        public string ChartType { get; set; }
        public int ChartStyle { get; set; }
        public double AddToRowPos { get; set; }
        public double AddToColPos { get; set; }
        
        public void AddChartTypeToChart()
        {
            if (!string.IsNullOrEmpty(this.ChartType) && this.Chart != null) //if chart type provided
            {
                if (!this.ChartType.ToLower().StartsWith("sl") && !this.ChartType.Contains("."))
                {
                    //Process basic charts as no spreadsheet light enum provided
                    string chartType = this.ChartType.Replace(" ", "").ToLower();
                    switch (chartType)
                    {
                        case ("pie"):
                            {
                                this.Chart.SetChartType(SLPieChartType.Pie);
                                break;

                            }
                        case ("bar"):
                            {
                                this.Chart.SetChartType(SLBarChartType.StackedBar);
                                break;

                            }
                        case ("line"):
                            {
                                this.Chart.SetChartType(SLLineChartType.Line);
                                break;

                            }
                        case ("area"):
                            {
                                this.Chart.SetChartType(SLAreaChartType.Area);
                                break;

                            }
                        case ("bubble"):
                            {
                                this.Chart.SetChartType(SLBubbleChartType.Bubble);
                                break;

                            }
                        case ("doughnut"):
                            {
                                this.Chart.SetChartType(SLDoughnutChartType.Doughnut);
                                break;

                            }
                        case ("surface"):
                            {
                                this.Chart.SetChartType(SLSurfaceChartType.Surface3D);
                                break;

                            }
                        case ("radar"):
                            {
                                this.Chart.SetChartType(SLRadarChartType.Radar);
                                break;

                            }
                        default:
                            {
                                this.Chart.SetChartType(SLBarChartType.ClusteredBar);
                                break;
                            }
                    }
                }
                else//Enum type provided
                {
                    try
                    {


                        if (this.ChartType.ToLower().StartsWith("SLPieChartType.".ToLower()))
                        {
                            try
                            {   
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLPieChartType.", "");
                                this.Chart.SetChartType((SLPieChartType)Enum.Parse(typeof(SLPieChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default pie chart
                                this.Chart.SetChartType(SLPieChartType.Pie);


                            }


                        }
                        else if (this.ChartType.ToLower().StartsWith("SLBarChartType.".ToLower()))
                        {
                            try
                            {
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLBarChartType.", "");
                                this.Chart.SetChartType((SLBarChartType)Enum.Parse(typeof(SLBarChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default Bar chart
                                this.Chart.SetChartType(SLBarChartType.StackedBar);

                            }
                        }
                        else if (this.ChartType.ToLower().StartsWith("SLLineChartType.".ToLower()))
                        {
                            try
                            {
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLLineChartType.", "");
                                this.Chart.SetChartType((SLLineChartType)Enum.Parse(typeof(SLLineChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default Line chart
                                this.Chart.SetChartType(SLLineChartType.Line);

                            }
                        }
                        else if (this.ChartType.ToLower().StartsWith("SLAreaChartType.".ToLower()))
                        {
                            try
                            {
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLAreaChartType.", "");
                                this.Chart.SetChartType((SLAreaChartType)Enum.Parse(typeof(SLAreaChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default Area chart
                                this.Chart.SetChartType(SLAreaChartType.Area);

                            }

                        }
                        else if (this.ChartType.ToLower().StartsWith("SLBubbleChartType.".ToLower()))
                        {
                            try
                            {
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLBubbleChartType.", "");
                                this.Chart.SetChartType((SLBubbleChartType)Enum.Parse(typeof(SLBubbleChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default Bubble chart
                                this.Chart.SetChartType(SLBubbleChartType.Bubble);

                            }
                        }
                        else if (this.ChartType.ToLower().StartsWith("SLDoughnutChartType.".ToLower()))
                        {
                            try
                            {
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLDoughnutChartType.", "");
                                this.Chart.SetChartType((SLDoughnutChartType)Enum.Parse(typeof(SLDoughnutChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default Doughnut chart
                                this.Chart.SetChartType(SLDoughnutChartType.Doughnut);

                            }
                        }
                        else if (this.ChartType.ToLower().StartsWith("(SLSurfaceChartType.".ToLower()))
                        {
                            try
                            {
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLSurfaceChartType.", "");
                                this.Chart.SetChartType((SLSurfaceChartType)Enum.Parse(typeof(SLSurfaceChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default Surface chart
                                this.Chart.SetChartType(SLSurfaceChartType.Surface3D);

                            }
                        }
                        else if (this.ChartType.ToLower().StartsWith("SLRadarChartType.".ToLower()))
                        {
                            try
                            {
                                
                                string subType = this.ChartType.Trim().Replace(" ", "").Replace("SLRadarChartType.", "");
                                this.Chart.SetChartType((SLRadarChartType)Enum.Parse(typeof(SLRadarChartType), subType));
                            }
                            catch (Exception)
                            {
                                //default Radar chart
                                this.Chart.SetChartType(SLRadarChartType.Radar);

                            }
                        }
                        else // default
                        {
                            this.Chart.SetChartType(SLBarChartType.ClusteredBar);

                        }
                    }
                    catch (Exception)
                    {

                        this.Chart.SetChartType(SLBarChartType.ClusteredBar);

                    }
                }


            }
            else //no chart type provided
            {
                this.Chart.SetChartType(SLBarChartType.ClusteredBar);
            }


        }
        public void AddChartStyleToChart()
        {
            int maxChartStyle = 48;
            if (this.ChartStyle == null || this.ChartStyle <= 0 || this.ChartStyle > maxChartStyle)
            {
                this.ChartStyle = 1;
            }
            SLChartStyle style = (SLChartStyle)Enum.Parse(typeof(SLChartStyle), "Style" + this.ChartStyle.ToString());
        }
        public delegate void SetChartTypeAndStyleDelegate(SLChart chart, string ChartName);
        public event SetChartTypeAndStyleDelegate SetChartTypeAndStyle;




    }
}
