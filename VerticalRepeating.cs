using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpreadsheetLight;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        private void StartVertRepeat(Bounds vertRepeatBounds, int repeatFrequency, RepeatMode repeatMode)
        {
            
            //Also if repeated rows have subRepeaters than add rowsinserted to top and bottom of such repeated defs.
            // All repeaters beneath the repeated zone must also have their top and bottom row increased by the number of rows being inserted.
            int rowsPerRepeat = vertRepeatBounds.Bottom - vertRepeatBounds.Top + 1;
            int repeatedTimes = 1;
            if (CurrentRow >= vertRepeatBounds.Top && CurrentRow <= vertRepeatBounds.Bottom)
            {
                throw new Exception(
                    "A repeater should be defined in the row before the area it is repeating. This row will be automatically deleted after repeating." +
                    "Current Row: " + CurrentRow.ToString() + " Repeater Bounds Top: " + vertRepeatBounds.Top.ToString() +
                    " Repeater Bounds Bottom: " + vertRepeatBounds.Bottom.ToString());
            }
            while (repeatedTimes < repeatFrequency)
            {
                int colBeingCopiedPos = vertRepeatBounds.Left;
                int RowBeingCopiedPos = vertRepeatBounds.Top;

                if (repeatMode == RepeatMode.insert)
                {

                    Document.InsertRow((rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos, rowsPerRepeat);
                    //Increase report bounds to fit the repeated rows
                    CurrentReportBounds.Bottom += rowsPerRepeat;
                    IncreaseReapetersBeneathDefinitionRowBy(rowsPerRepeat,
                                                            ((rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos));
                    IncreaseChartsBeneathDataRowBy(rowsPerRepeat, (rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos);
                    //Previously parsed chart definition whose definition has been removed from template but are pointing to a range where row is inserted.
                    AdjustChartDataPostionOnRowInserted((rowsPerRepeat * repeatedTimes) + RowBeingCopiedPos, rowsPerRepeat );
                                                               
                    this.RowsInserted.Add(new RowsInsertedItem((rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos,
                                                               rowsPerRepeat));

                }
                else if (repeatMode == RepeatMode.shift)
                {
                    ShiftRowDown(vertRepeatBounds.Left, vertRepeatBounds.Right,
                                 (rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos, rowsPerRepeat);
                    CurrentReportBounds.Bottom += rowsPerRepeat;
                    // Increase to and bottom bounds of all repeaters and sections defined below the inserted rows by the number of rows being inserted

                    IncreaseReapetersBeneathDefinitionRowBy(rowsPerRepeat,
                                                            ((rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos));
                    IncreaseChartDataRowsBeneathShiftedRows(rowsPerRepeat,((rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos), vertRepeatBounds.Left, vertRepeatBounds.Right);
                    AdjustChartPositionOnRowShift(((rowsPerRepeat * repeatedTimes) + RowBeingCopiedPos), rowsPerRepeat, vertRepeatBounds.Left, vertRepeatBounds.Right );
                    this.RowsShifted.Add(new RowsShiftedItem((rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos,
                                                             rowsPerRepeat, vertRepeatBounds.Left,
                                                             vertRepeatBounds.Right));


                }
                else
                {
                    CurrentReportBounds.Bottom += rowsPerRepeat;
                }

                while (RowBeingCopiedPos <= vertRepeatBounds.Bottom)
                {


                    while (colBeingCopiedPos <= vertRepeatBounds.Right)
                    {
                        Document.CopyCell(RowBeingCopiedPos, colBeingCopiedPos,
                                          (rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos, colBeingCopiedPos,
                                          SLPasteTypeValues.Paste);
                        double sourceRowHeight = Document.GetRowHeight(RowBeingCopiedPos);
                        Document.SetRowHeight((rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos, sourceRowHeight);
                        IncreaseChildReapeterRowBy((rowsPerRepeat*repeatedTimes),
                                                   (rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos, colBeingCopiedPos,
                                                   vertRepeatBounds);
                        if (repeatMode == RepeatMode.insert)
                        {
                            IncreaseRepeatedChartsRowBy((rowsPerRepeat*repeatedTimes), RowBeingCopiedPos,
                                                        (rowsPerRepeat*repeatedTimes) + RowBeingCopiedPos,
                                                        colBeingCopiedPos);
                        }
                        else
                        {
                            IncreaseRepeatedChartsRowsIfDataInRepeatedZone((rowsPerRepeat * repeatedTimes),
                                                                          RowBeingCopiedPos,
                                                                          (rowsPerRepeat * repeatedTimes) +
                                                                          RowBeingCopiedPos,
                                                                          colBeingCopiedPos, vertRepeatBounds.Left, vertRepeatBounds.Right);
                        }
                        colBeingCopiedPos++;
                    }
                    colBeingCopiedPos = vertRepeatBounds.Left;
                    RowBeingCopiedPos++;
                }

                //copy cell merging
                foreach (var mergeCell in Document.GetWorksheetMergeCells())
                {
                    if (mergeCell.StartRowIndex >= vertRepeatBounds.Top &&
                        mergeCell.EndRowIndex <= vertRepeatBounds.Bottom &&
                        mergeCell.StartColumnIndex >= vertRepeatBounds.Left &&
                        mergeCell.EndColumnIndex <= vertRepeatBounds.Right)
                    {

                        Document.MergeWorksheetCells(mergeCell.StartRowIndex + (repeatedTimes*rowsPerRepeat),
                                                     mergeCell.StartColumnIndex,
                                                     mergeCell.EndRowIndex + (rowsPerRepeat*repeatedTimes),
                                                     mergeCell.EndColumnIndex);

                    }
                }
                CurrentReportBounds.Bottom = CurrentReportBounds.Bottom + rowsPerRepeat;
                repeatedTimes = repeatedTimes + 1;
            }
        }

        //There might be more repeters below the current repeater whose value of top and bottom might have canged because of new row additions
        private void IncreaseReapetersBeneathDefinitionRowBy(int increaseByValue, int fromRowNumber)
        {
            int i = fromRowNumber;
            while (i <= CurrentReportBounds.Bottom)
            {
                int j = CurrentReportBounds.Left;
                while (j <= CurrentReportBounds.Right)
                {
                    string cellText = Document.GetCellValueAsString(i, j);
                    if (HasRepeaterDefinition(cellText))
                    {
                        if (HasVertRepeaterDefined(cellText))
                        {

                            int startIndex = cellText.IndexOf("<vertrepeat");
                            int endIndex = cellText.IndexOf("/>", startIndex);
                            int frequency = 0;
                            RepeatMode repeatMode = RepeatMode.insert;
                            Bounds vertRepeatbounds = GetVertRepeaterBounds(cellText, out frequency, out repeatMode);
                            cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                                 "<vertrepeat " + "top=" +
                                                                                                 (vertRepeatbounds.Top +
                                                                                                  increaseByValue).
                                                                                                     ToString() +
                                                                                                 " , " + "bottom=" +
                                                                                                 (vertRepeatbounds.
                                                                                                      Bottom +
                                                                                                  increaseByValue).
                                                                                                     ToString() +
                                                                                                 " , " + "left=" +
                                                                                                 vertRepeatbounds.Left.
                                                                                                     ToString() + " , " +
                                                                                                 "right=" +
                                                                                                 vertRepeatbounds.Right.
                                                                                                     ToString() + " , " +
                                                                                                 "frequency=" +
                                                                                                 frequency.ToString() +
                                                                                                 " , " + "mode=" +
                                                                                                 repeatMode.ToString());


                            Document.SetCellValue(i, j, cellText);

                        }
                        if (HasHorizRepeaterDefined(cellText))
                        {
                            int startIndex = cellText.IndexOf("<horizrepeat");
                            int endIndex = cellText.IndexOf("/>", startIndex);
                            int frequency = 0;
                            RepeatMode repeatMode = RepeatMode.shift;
                            Bounds vertRepeatbounds = GetHorizRepeaterBounds(cellText, out frequency, out repeatMode);
                            cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                                 "<horizrepeat " +
                                                                                                 "top=" +
                                                                                                 (vertRepeatbounds.Top +
                                                                                                  increaseByValue).
                                                                                                     ToString() +
                                                                                                 " , " + "bottom=" +
                                                                                                 (vertRepeatbounds.
                                                                                                      Bottom +
                                                                                                  increaseByValue).
                                                                                                     ToString() +
                                                                                                 " , " + "left=" +
                                                                                                 vertRepeatbounds.Left.
                                                                                                     ToString() + " , " +
                                                                                                 "right=" +
                                                                                                 vertRepeatbounds.Right.
                                                                                                     ToString() + " , " +
                                                                                                 "frequency=" +
                                                                                                 frequency.ToString() +
                                                                                                 " , " + "mode=" +
                                                                                                 repeatMode.ToString());
                            Document.SetCellValue(i, j, cellText);
                        }

                    }
                    j++;
                }
                i++;
                j = CurrentReportBounds.Left;
            }
        }

        //Increase the top and bottom values of the sub repeaters that might have been replicated
        private void IncreaseChildReapeterRowBy(int increaseByValue, int row, int col, Bounds parentRepeaterBounds)
        {
            string cellText = Document.GetCellValueAsString(row, col);
            if (HasRepeaterDefinition(cellText))
            {
                if (HasVertRepeaterDefined(cellText))
                {


                    int startIndex = cellText.IndexOf("<vertrepeat");
                    int endIndex = cellText.IndexOf("/>", startIndex);
                    int frequency = 0;
                    RepeatMode repeatMode = RepeatMode.insert;
                    Bounds vertRepeatbounds = GetVertRepeaterBounds(cellText, out frequency, out repeatMode);
                    //if any side is within the parent than make sure that it is completly within it.
                    if ((vertRepeatbounds.Left >= parentRepeaterBounds.Left &&
                         vertRepeatbounds.Left <= parentRepeaterBounds.Right) ||
                        (vertRepeatbounds.Right <= parentRepeaterBounds.Right &&
                         vertRepeatbounds.Right >= parentRepeaterBounds.Left) ||
                        (vertRepeatbounds.Bottom <= parentRepeaterBounds.Bottom &&
                         vertRepeatbounds.Bottom >= parentRepeaterBounds.Top) ||
                        (vertRepeatbounds.Top <= parentRepeaterBounds.Bottom &&
                         vertRepeatbounds.Top >= parentRepeaterBounds.Top))
                    {
                        if (vertRepeatbounds.Top >= parentRepeaterBounds.Top &&
                            vertRepeatbounds.Bottom <= parentRepeaterBounds.Bottom &&
                            vertRepeatbounds.Left >= parentRepeaterBounds.Left &&
                            vertRepeatbounds.Right <= parentRepeaterBounds.Right)
                        {
                            //Its ok do nothing  

                        }
                        else
                        {
                            throw new Exception(
                                "A child repeater cannot define boundaries which are out side the parent boundaries");
                        }
                    }
                    cellText =
                        cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                  "<vertrepeat " + "top=" +
                                                                                  (vertRepeatbounds.Top +
                                                                                   increaseByValue).ToString() + " , " +
                                                                                  "bottom=" +
                                                                                  (vertRepeatbounds.Bottom +
                                                                                   increaseByValue).ToString() + " , " +
                                                                                  "left=" +
                                                                                  (vertRepeatbounds.Left).ToString() +
                                                                                  " , " + "right=" +
                                                                                  (vertRepeatbounds.Right).ToString() +
                                                                                  " , " + "frequency=" +
                                                                                  frequency.ToString()) + " , " +
                        "mode=" + repeatMode.ToString();
                    Document.SetCellValue(row, col, cellText);

                }
                if (HasHorizRepeaterDefined(cellText))
                {
                    int startIndex = cellText.IndexOf("<horizrepeat");
                    int endIndex = cellText.IndexOf("/>", startIndex);
                    int frequency = 0;
                    RepeatMode repeatMode = RepeatMode.shift;
                    Bounds horizRepeaterBounds = GetHorizRepeaterBounds(cellText, out frequency, out repeatMode);
                    //if any side is within the parent than make sure that it is completly within it.
                    if ((horizRepeaterBounds.Left >= parentRepeaterBounds.Left &&
                         horizRepeaterBounds.Left <= parentRepeaterBounds.Right) ||
                        (horizRepeaterBounds.Right <= parentRepeaterBounds.Right &&
                         horizRepeaterBounds.Right >= parentRepeaterBounds.Left) ||
                        (horizRepeaterBounds.Bottom <= parentRepeaterBounds.Bottom &&
                         horizRepeaterBounds.Bottom >= parentRepeaterBounds.Top) ||
                        (horizRepeaterBounds.Top <= parentRepeaterBounds.Bottom &&
                         horizRepeaterBounds.Top >= parentRepeaterBounds.Top))
                    {
                        if (horizRepeaterBounds.Top >= parentRepeaterBounds.Top &&
                            horizRepeaterBounds.Bottom <= parentRepeaterBounds.Bottom &&
                            horizRepeaterBounds.Left >= parentRepeaterBounds.Left &&
                            horizRepeaterBounds.Right <= parentRepeaterBounds.Right)
                        {
                            //Its ok do nothing  
                        }
                        else
                        {
                            throw new Exception(
                                "A child repeater cannot define boundaries which are out side the parent boundaries");
                        }
                    }
                    cellText =
                        cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                  "<horizrepeat " + "top=" +
                                                                                  (horizRepeaterBounds.Top +
                                                                                   increaseByValue).ToString() + " , " +
                                                                                  "bottom=" +
                                                                                  (horizRepeaterBounds.Bottom +
                                                                                   increaseByValue).ToString() + " , " +
                                                                                  "left=" +
                                                                                  (horizRepeaterBounds.Left).ToString() +
                                                                                  " , " + "right=" +
                                                                                  (horizRepeaterBounds.Right).ToString() +
                                                                                  " , " + "frequency=" +
                                                                                  frequency.ToString()) + " , " +
                        "mode=" + repeatMode.ToString();
                    Document.SetCellValue(row, col, cellText);
                }
            }
        }

        private Bounds GetVertRepeaterBounds(string cellText, out int repeatFrequency, out RepeatMode repeatMode)
        {
            string fullCellText = cellText;
            var bounds = new Bounds();
            string originalText = cellText;
            repeatFrequency = 0;
            string repMode = "insert";
            if (HasVertRepeaterDefined(cellText))
            {
                int vertRepeatDefStart = cellText.ToLower().IndexOf("<vertrepeat");
                int vertRepeatDefEnds = cellText.ToLower().IndexOf("/>", vertRepeatDefStart) + 2;
                string inSideText =
                    cellText.Substring(vertRepeatDefStart, vertRepeatDefEnds - vertRepeatDefStart).Replace(
                        "<vertrepeat", "").Replace("/>", "");
                string[] valuePairs = inSideText.Replace(" ", "").Split(new char[] {','}, inSideText.Length);
                foreach (string valuePair in valuePairs)
                {
                    if (valuePair.StartsWith("top="))
                    {
                        bounds.Top = int.Parse(valuePair.Replace("top=", ""));

                    }
                    if (valuePair.StartsWith("bottom="))
                    {
                        bounds.Bottom = int.Parse(valuePair.Replace("bottom=", ""));

                    }
                    if (valuePair.StartsWith("left="))
                    {
                        bounds.Left = int.Parse(valuePair.Replace("left=", ""));

                    }
                    if (valuePair.StartsWith("right="))
                    {

                        bounds.Right = int.Parse(valuePair.Replace("right=", ""));

                    }
                    if (valuePair.StartsWith("frequency="))
                    {

                        repeatFrequency = int.Parse(valuePair.Replace("frequency=", ""));
                    }
                    if (valuePair.StartsWith("mode="))
                    {
                        repMode = valuePair.Replace("mode=", "").Trim().ToLower();



                    }

                }



            }
            repeatMode = (RepeatMode) Enum.Parse(typeof (RepeatMode), repMode);

            return bounds;
        }
        // I have comented this method because if I increase the data top pos it will be increased again when row insert parser will adjust all charts
        // If I leave it as it is than the row insert parser will not change anything as rows are inserted beneath the marked bounds so charts bounds will appear to it as if the rows were inserted beneath it.
        private void IncreaseRepeatedChartsRowBy(int increaseByValue, int fromRowNumber, int rowCopied, int colCopied)
        {

            string cellText = Document.GetCellValueAsString(rowCopied, colCopied);
            if (HasChartDefinition(cellText))
            {

                int chartDefStartedAt = cellText.ToLower().IndexOf("<chart");
                int chartDefEndsAt = cellText.ToLower().IndexOf("/>", chartDefStartedAt);
                string chartDefFull = cellText.Substring(chartDefStartedAt, chartDefEndsAt - chartDefStartedAt);
                string chartDef = chartDefFull.Replace(" ", "").Replace("<chart", "").Replace("/>", "");
                string[] keyVals = chartDef.Split(new char[] { ',' }, chartDef.Length);
                string newChartDef = "";
                List<string> newKeyVals = new List<string>();
                foreach (string keyVal in keyVals)
                {

                    if (keyVal.Replace(" ", "").ToLower().StartsWith("top="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("top=", "");
                        int topVal = int.Parse(val);
                        //if (topVal >= fromRowNumber)
                        //{
                            topVal = topVal + increaseByValue;
                            
                        //}
                        newKeyVals.Add("top=" + topVal.ToString());

                    }
                    else if (keyVal.Replace(" ", "").ToLower().StartsWith("bottom="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("bottom=", "");
                        int botVal = int.Parse(val);
                        //if (botVal >= fromRowNumber)
                        //{
                            botVal = botVal + increaseByValue;
                           
                        //}
                        newKeyVals.Add("bottom=" + botVal.ToString());

                    }
                    else
                    {
                        newKeyVals.Add(keyVal);
                    }


                }


                foreach (var keyVal in newKeyVals)
                {
                    if (!newChartDef.Contains("<chart"))
                    {
                        newChartDef = "<chart " + keyVal;
                    }
                    else
                    {
                        newChartDef = newChartDef + " , " + keyVal;
                    }


                }
                cellText = cellText.Remove(chartDefStartedAt, chartDefEndsAt);
                cellText = cellText.Insert(chartDefStartedAt, newChartDef);
                Document.SetCellValue(rowCopied, colCopied, cellText);
               
               
               
            }
        }

        private void IncreaseChartsBeneathDataRowBy(int increaseByValue, int fromRowNumber)
        {


            int row = fromRowNumber;
            while (row <= CurrentReportBounds.Bottom)
            {
                int col = CurrentReportBounds.Left;
                while (col <= CurrentReportBounds.Right)
                {



                    string cellText = Document.GetCellValueAsString(row, col);
                    if (HasChartDefinition(cellText))
                    {

                        int chartDefStartedAt = cellText.ToLower().IndexOf("<chart");
                        int chartDefEndsAt = cellText.ToLower().IndexOf("/>", chartDefStartedAt);
                        string chartDefFull = cellText.Substring(chartDefStartedAt, chartDefEndsAt - chartDefStartedAt);
                        string chartDef = chartDefFull.Replace(" ", "").Replace("<chart", "").Replace("/>", "");
                        string[] keyVals = chartDef.Split(new char[] {','}, chartDef.Length);
                        string newChartDef = "";
                        List<string> newKeyVals = new List<string>();
                        foreach (string keyVal in keyVals)
                        {

                            if (keyVal.Replace(" ", "").ToLower().StartsWith("top="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("top=", "");
                                int topVal = int.Parse(val);
                                if (topVal >= fromRowNumber)
                                {
                                    topVal = topVal + increaseByValue;
                                    
                                }
                                newKeyVals.Add("top=" + topVal.ToString());

                            }
                            else if (keyVal.Replace(" ", "").ToLower().StartsWith("bottom="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("bottom=", "");
                                int botVal = int.Parse(val);
                                if (botVal >= fromRowNumber)
                                {
                                    botVal = botVal + increaseByValue;
                                    
                                }
                                newKeyVals.Add("bottom=" + botVal.ToString());

                            }
                            else
                            {
                                newKeyVals.Add(keyVal);
                            }


                        }


                        foreach (var keyVal in newKeyVals)
                        {
                            if (!newChartDef.Contains("<chart"))
                            {
                                newChartDef = "<chart " + keyVal;
                            }
                            else
                            {
                                newChartDef = newChartDef + " , " + keyVal;
                            }


                        }
                        cellText = cellText.Remove(chartDefStartedAt, chartDefEndsAt);
                        cellText = cellText.Insert(chartDefStartedAt, newChartDef);
                        Document.SetCellValue(row, col, cellText);
                       



                    }
                    col++;
                }
                row++;
            }
        }

        private void IncreaseChartDataRowsBeneathShiftedRows(int increaseByValue, int fromRowNumber, int repeateleft, int repeaterright)
        {
            //if charts left or right are between repeaters left or right then 
            //increase charts top if it is grater than fromRowNumber 
            //increase charts bottom if it is grater than fromrowNumber
            
            int row = CurrentReportBounds.Top;
            while (row <= CurrentReportBounds.Bottom)
            {
                int col = CurrentReportBounds.Left;
                while (col <= CurrentReportBounds.Right)
                {



                    string cellText = Document.GetCellValueAsString(row, col);
                    if (HasChartDefinition(cellText))
                    {

                        int chartDefStartedAt = cellText.ToLower().IndexOf("<chart");
                        int chartDefEndsAt = cellText.ToLower().IndexOf("/>", chartDefStartedAt);
                        string chartDefFull = cellText.Substring(chartDefStartedAt, chartDefEndsAt - chartDefStartedAt);
                        string chartDef = chartDefFull.Replace(" ", "").Replace("<chart", "").Replace("/>", "");
                        string[] keyVals = chartDef.Split(new char[] { ',' }, chartDef.Length);
                        string newChartDef = "";
                        List<string> newKeyVals = new List<string>();
                        int chartDataLeft = -1;
                        int chartDataRight = -1;
                        foreach (string keyVal in keyVals)
                        {
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("left="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("left=", "");
                                chartDataRight = int.Parse(val);
                            }
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("right="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("right=", "");
                                chartDataRight = int.Parse(val);
                            }

                        }

                        foreach (string keyVal in keyVals)
                        {

                            if (keyVal.Replace(" ", "").ToLower().StartsWith("top=") )
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("top=", "");
                                int topVal = int.Parse(val);
                                
                                    if ((chartDataLeft >= repeateleft && chartDataLeft <= repeaterright) ||
                                        (chartDataRight >= repeateleft && chartDataRight <= repeaterright) || (chartDataLeft <= repeateleft && chartDataRight >= repeaterright) )
                                    {
                                        if (topVal >= fromRowNumber)
                                        {
                                            topVal = topVal + increaseByValue;

                                        }
                                    }
                               
                                newKeyVals.Add("top=" + topVal.ToString());

                            }
                            else if (keyVal.Replace(" ", "").ToLower().StartsWith("bottom="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("bottom=", "");
                                int botVal = int.Parse(val);
                                if ((chartDataLeft >= repeateleft && chartDataLeft <= repeaterright) ||
                                        (chartDataRight >= repeateleft && chartDataRight <= repeaterright) || (chartDataLeft <= repeateleft && chartDataRight >= repeaterright))
                                {
                                    if (botVal >= fromRowNumber)
                                    {
                                        botVal = botVal + increaseByValue;

                                    }
                                }
                                newKeyVals.Add("bottom=" + botVal.ToString());

                            }
                            else
                            {
                                newKeyVals.Add(keyVal);
                            }


                        }


                        foreach (var keyVal in newKeyVals)
                        {
                            if (!newChartDef.Contains("<chart"))
                            {
                                newChartDef = "<chart " + keyVal;
                            }
                            else
                            {
                                newChartDef = newChartDef + " , " + keyVal;
                            }


                        }
                        cellText = cellText.Remove(chartDefStartedAt, chartDefEndsAt);
                        cellText = cellText.Insert(chartDefStartedAt, newChartDef);
                        Document.SetCellValue(row, col, cellText);




                    }
                    col++;
                }
                row++;
            }
        }

        private void IncreaseRepeatedChartsRowsIfDataInRepeatedZone(int increaseByValue, int fromRowNumber, int rowCopied, int colCopied, int repeaterLeft, int repeaterRight)
        {
            string cellText = Document.GetCellValueAsString(rowCopied, colCopied);
            if (HasChartDefinition(cellText))
            {

                int chartDefStartedAt = cellText.ToLower().IndexOf("<chart");
                int chartDefEndsAt = cellText.ToLower().IndexOf("/>", chartDefStartedAt);
                string chartDefFull = cellText.Substring(chartDefStartedAt, chartDefEndsAt - chartDefStartedAt);
                string chartDef = chartDefFull.Replace(" ", "").Replace("<chart", "").Replace("/>", "");
                string[] keyVals = chartDef.Split(new char[] { ',' }, chartDef.Length);
                string newChartDef = "";
                List<string> newKeyVals = new List<string>();
                int chartDataLeft = -1;
                int chartDataRight = -1;
                foreach (string keyVal in keyVals)
                {
                    if (keyVal.Replace(" ", "").ToLower().StartsWith("left="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("left=", "");
                        chartDataLeft = int.Parse(val);
                    }
                    if (keyVal.Replace(" ", "").ToLower().StartsWith("right="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("right=", "");
                        chartDataRight = int.Parse(val);
                    }

                }
                foreach (string keyVal in keyVals)
                {

                    if (keyVal.Replace(" ", "").ToLower().StartsWith("top="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("top=", "");
                        int topVal = int.Parse(val);
                        //if (topVal >= fromRowNumber)
                        //{
                        topVal = topVal + increaseByValue;

                        //}
                        newKeyVals.Add("top=" + topVal.ToString());

                    }
                    else if (keyVal.Replace(" ", "").ToLower().StartsWith("bottom="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("bottom=", "");
                        int botVal = int.Parse(val);
                        if ((chartDataLeft >= repeaterLeft && chartDataLeft <= repeaterRight) ||
                                        (chartDataRight >= repeaterLeft && chartDataRight <= repeaterRight) || (chartDataLeft <= repeaterLeft && chartDataRight >= repeaterRight))
                        {
                            //if (botVal >= fromRowNumber)
                            //{
                            botVal = botVal + increaseByValue;

                            //}
                        }
                        newKeyVals.Add("bottom=" + botVal.ToString());

                    }
                    else
                    {
                        newKeyVals.Add(keyVal);
                    }


                }


                foreach (var keyVal in newKeyVals)
                {
                    if (!newChartDef.Contains("<chart"))
                    {
                        newChartDef = "<chart " + keyVal;
                    }
                    else
                    {
                        newChartDef = newChartDef + " , " + keyVal;
                    }


                }
                cellText = cellText.Remove(chartDefStartedAt, chartDefEndsAt);
                cellText = cellText.Insert(chartDefStartedAt, newChartDef);
                Document.SetCellValue(rowCopied, colCopied, cellText);



            }
        }

    }
}
