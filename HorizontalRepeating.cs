using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using SpreadsheetLight;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        private void StartHorizRepeat(Bounds horizRepeatBounds, int repeatFrequency, RepeatMode repeatMode)
        {
            //Also if repeated cols have subRepeaters than add cols inserted to left and right of such repeated defs.
            // All repeaters beneath the repeated zone must also have their left and right colFromWhere increased by the number of cols being inserted.
            int colsPerRepeat = horizRepeatBounds.Right - horizRepeatBounds.Left + 1;
            int repeatedTimes = 1;
            int colIndexWhereInserted = 0;

            if ((CurrentRow >= horizRepeatBounds.Top && CurrentRow <= horizRepeatBounds.Bottom || (CurrentColumn > horizRepeatBounds.Left && CurrentColumn > horizRepeatBounds.Right)))
            {
                NotifyReportLogEvent(
                 "A horizontal repeater should be defined in row and coloumn before the area it is repeating. This row will be automatically deleted after repeating." +
                 "Current Row: " + CurrentRow.ToString() + " Repeater Bounds Top: " + horizRepeatBounds.Top.ToString() +
                 " Repeater Bounds Bottom: " + horizRepeatBounds.Bottom.ToString());
                return;
            }

            try
            {
                while (repeatedTimes < repeatFrequency)
                {
                    int RowBeingCopiedPos = horizRepeatBounds.Top;
                    int colBeingCopiedPos = horizRepeatBounds.Left;
                    if (repeatMode == RepeatMode.insert)
                    {
                        try
                        {
                            //this will make space for cells to be copied and this will insert all cols required for single repeat
                            colIndexWhereInserted = (colsPerRepeat * repeatedTimes) + colBeingCopiedPos;
                            Document.InsertColumn(colIndexWhereInserted, colsPerRepeat);
                            CurrentReportBounds.Right += colsPerRepeat;
                            IncreaseRepeatersColsRightOfHorizRepeatedZone(horizRepeatBounds, colIndexWhereInserted,
                                                                          colsPerRepeat);
                            IncreaseRepeatersColsBottomOfHorizRepeatColInsertedZone(horizRepeatBounds,
                                                                                    colIndexWhereInserted, colsPerRepeat);
                            AdjustChartPositionOnColumnInserted(colIndexWhereInserted, colsPerRepeat);
                            IncreaseAllUnParsedChartsHavingDataRightOfTheRepeaterZoneColsBy(colsPerRepeat,
                                                                                            colIndexWhereInserted);
                            this.ColsInseted.Add(new ColsInsertedItem(colIndexWhereInserted, colsPerRepeat));
                        }
                        catch
                        {
                            NotifyReportLogEvent("Could not process horizontal repeater in insert mode ");
                            return;
                        }
                    }
                    else if (repeatMode == RepeatMode.shift)
                    {
                        try
                        {
                            colIndexWhereInserted = (colsPerRepeat * repeatedTimes) + colBeingCopiedPos;
                            IncreaseAllUnParsedChartsHavingDataRightOfTheHorizRepeaterShiftZone(colsPerRepeat, horizRepeatBounds, repeatedTimes);
                            ShiftColRight(horizRepeatBounds.Top, horizRepeatBounds.Bottom,
                                          (colsPerRepeat * repeatedTimes) + colBeingCopiedPos, colsPerRepeat);

                            CurrentReportBounds.Right += colsPerRepeat;
                            IncreaseRepeatersColsRightOfHorizRepeatedZone(horizRepeatBounds, colIndexWhereInserted, colsPerRepeat);
                            AdjustChartPositionOnColShift(colIndexWhereInserted, colsPerRepeat, horizRepeatBounds.Top, horizRepeatBounds.Bottom);
                            IncreaseAllUnParsedChartsAtBottomHavingDataRigthOfTheHorizRepeaterShiftZone(colsPerRepeat, horizRepeatBounds);
                            this.ColsShifted.Add(new ColsShiftedItem((colsPerRepeat * repeatedTimes) + colBeingCopiedPos, colsPerRepeat, horizRepeatBounds.Top, horizRepeatBounds.Bottom));
                        }
                        catch
                        {
                            NotifyReportLogEvent("Could not process horizontal repeater in shift mode.");
                            return;
                        }
                    }
                    else
                    {
                        CurrentReportBounds.Right += colsPerRepeat;
                    }
                    //Copy cells one by one space is alrady made if overwrite option not chosen. Cells are copied from first cell zone but not recursively
                    // copy to right
                    while (RowBeingCopiedPos <= horizRepeatBounds.Bottom)
                    {
                        while (colBeingCopiedPos <= horizRepeatBounds.Right)
                        {
                            Document.CopyCell(RowBeingCopiedPos, colBeingCopiedPos,
                                              RowBeingCopiedPos, (colsPerRepeat * repeatedTimes) + colBeingCopiedPos, SLPasteTypeValues.Paste);

                            //Set destination column width to match with the source
                            double sourceColWidth = Document.GetColumnWidth(colBeingCopiedPos);
                            Document.SetColumnWidth((colsPerRepeat * repeatedTimes) + colBeingCopiedPos, sourceColWidth);
                            // Increase By Value, rowIdofCopiedCell, collIdOfCopedCell this method targets sub repeaters only and dose not targets repeaters left or right
                            IncreaseRepeaterDefinitionInCopiedCellText_ColBy((colsPerRepeat * repeatedTimes),
                                                                              RowBeingCopiedPos,
                                                                              (colsPerRepeat * repeatedTimes) +
                                                                              colBeingCopiedPos);
                            if (repeatMode == RepeatMode.insert)
                            {
                                IncreaseRepeatedChartsColBy((colsPerRepeat * repeatedTimes), colBeingCopiedPos,
                                                            RowBeingCopiedPos,
                                                             (colsPerRepeat * repeatedTimes) + colBeingCopiedPos);
                            }
                            else
                            {
                                CheckChartsInCopiedAndAdjustItsDataIfWithinRepeatedZone((colsPerRepeat * repeatedTimes),
                                                                                        RowBeingCopiedPos,
                                                                                        (colsPerRepeat * repeatedTimes) +
                                                                                        colBeingCopiedPos, horizRepeatBounds);
                            }
                            colBeingCopiedPos++;
                        }
                        //Move next row
                        colBeingCopiedPos = horizRepeatBounds.Left;
                        RowBeingCopiedPos++;
                    }
                    //Copy cell merges inside the repeate zone
                    foreach (var mergeCell in Document.GetWorksheetMergeCells())
                    {
                        if (mergeCell.StartRowIndex >= horizRepeatBounds.Top && mergeCell.EndRowIndex <= horizRepeatBounds.Bottom && mergeCell.StartColumnIndex >= horizRepeatBounds.Left && mergeCell.EndColumnIndex <= horizRepeatBounds.Right)
                        {
                            Document.MergeWorksheetCells(mergeCell.StartRowIndex,
                                                         mergeCell.StartColumnIndex + (repeatedTimes * colsPerRepeat),
                                                         mergeCell.EndRowIndex,
                                                         mergeCell.EndColumnIndex + (colsPerRepeat * repeatedTimes));
                        }
                    }

                    repeatedTimes = repeatedTimes + 1;
                }
            }
            catch
            {
                NotifyReportLogEvent("Could not process horizontal repeater.");
                return;
            }
        }

        // Increase By Value, rowIdofCopiedCell, collIdOfCopedCell this method targets sub repeaters only and dose not targets repeaters right or bottom
        private void IncreaseRepeaterDefinitionInCopiedCellText_ColBy(int increaseByValue, int rowIDOfCopiedCell, int colIDOfCopiedCell)
        {
            string cellText = Document.GetCellValueAsString(rowIDOfCopiedCell, colIDOfCopiedCell);

            while (HasRepeaterDefinition(cellText))
            {
                if (HasVertRepeaterDefined(cellText))
                {
                    int startIndex = cellText.IndexOf("<vertrepeat");
                    int endIndex = cellText.IndexOf("/>", startIndex);
                    int frequency = 0;
                    RepeatMode repeatMode = RepeatMode.insert;
                    Bounds vertRepeatbounds = GetVertRepeaterBounds(cellText, out frequency, out repeatMode);
                    vertRepeatbounds.Left = vertRepeatbounds.Left + increaseByValue;
                    vertRepeatbounds.Right = vertRepeatbounds.Right + increaseByValue;

                    cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                         "<vertrepeat " + "top=" +
                                                                                         vertRepeatbounds.Top.
                                                                                             ToString() +
                                                                                         " , " + "bottom=" +
                                                                                         vertRepeatbounds.
                                                                                             Bottom.
                                                                                             ToString() +
                                                                                         " , " + "left=" +
                                                                                         vertRepeatbounds.Left.
                                                                                             ToString() + " , " +
                                                                                         "right=" +
                                                                                         vertRepeatbounds.Right.
                                                                                             ToString() + " , " +
                                                                                         "frequency=" +
                                                                                         frequency.ToString() + " , " +
                                                                                         "mode=" + repeatMode.ToString());

                    Document.SetCellValue(rowIDOfCopiedCell, colIDOfCopiedCell, cellText);
                }
                if (HasHorizRepeaterDefined(cellText))
                {
                    int startIndex = cellText.IndexOf("<horizrepeat");
                    int endIndex = cellText.IndexOf("/>", startIndex);
                    int frequency = 0;
                    RepeatMode repeatMode = RepeatMode.shift;
                    Bounds horizRepeaterBounds = GetVertRepeaterBounds(cellText, out frequency, out repeatMode);
                    horizRepeaterBounds.Left = horizRepeaterBounds.Left + increaseByValue;
                    horizRepeaterBounds.Right = horizRepeaterBounds.Right + increaseByValue;
                    cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                         "<horizrepeat " +
                                                                                         "top=" +
                                                                                         horizRepeaterBounds.Top.
                                                                                             ToString() +
                                                                                         " , " + "bottom=" +
                                                                                         horizRepeaterBounds.
                                                                                             Bottom.
                                                                                             ToString() +
                                                                                         " , " + "left=" +
                                                                                         horizRepeaterBounds.Left.
                                                                                             ToString() + " , " +
                                                                                         "right=" +
                                                                                         horizRepeaterBounds.Right.
                                                                                             ToString() + " , " +
                                                                                         "frequency=" +
                                                                                         frequency.ToString() + " , " +
                                                                                         "mode=" + repeatMode.ToString());
                    Document.SetCellValue(rowIDOfCopiedCell, colIDOfCopiedCell, cellText);
                }
            }
        }

        // This is called when cols are inserted
        private void IncreaseRepeatersColsBottomOfHorizRepeatColInsertedZone(Bounds horizRepeatBounds, int colInsertedAt, int noOfColsInsertedShifted)
        {
            int topLimit = horizRepeatBounds.Bottom;
            int bottomLimit = CurrentReportBounds.Bottom;
            int leftLimit = CurrentReportBounds.Left;
            int rightLimit = CurrentReportBounds.Right;
            for (int i = topLimit; i <= bottomLimit; i++)
            {
                for (int j = leftLimit; j <= rightLimit; j++)
                {
                    string cellText = Document.GetCellValueAsString(i, j);
                    if (HasHorizRepeaterDefined(cellText))
                    {
                        if (HasVertRepeaterDefined(cellText))
                        {
                            int startIndex = cellText.IndexOf("<vertrepeat");
                            int endIndex = cellText.IndexOf("/>", startIndex);
                            int frequency = 0;
                            RepeatMode repeatMode = RepeatMode.insert;
                            Bounds vertRepeatbounds = GetVertRepeaterBounds(cellText, out frequency, out repeatMode);

                            if ((vertRepeatbounds.Top >= topLimit && vertRepeatbounds.Top <= bottomLimit) ||
                                (vertRepeatbounds.Bottom >= topLimit && vertRepeatbounds.Bottom <= bottomLimit))
                            {
                                if (vertRepeatbounds.Left >= colInsertedAt)
                                {
                                    vertRepeatbounds.Left += noOfColsInsertedShifted;
                                }
                                if (vertRepeatbounds.Right >= colInsertedAt)
                                {
                                    vertRepeatbounds.Right += noOfColsInsertedShifted;
                                }
                                cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                                     "<vertrepeat " +
                                                                                                     "top=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Top.
                                                                                                         ToString() +
                                                                                                     " , " + "bottom=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Bottom.
                                                                                                         ToString() +
                                                                                                     " , " + "left=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Left.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "right=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Right.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "frequency=" +
                                                                                                     frequency.ToString() +
                                                                                                     " , " + "mode=" +
                                                                                                     repeatMode.ToString
                                                                                                         ());

                                Document.SetCellValue(i, j, cellText);
                            }
                        }
                        if (HasHorizRepeaterDefined(cellText))
                        {
                            int startIndex = cellText.IndexOf("<horizrepeat");
                            int endIndex = cellText.IndexOf("/>", startIndex);
                            int frequency = 0;
                            RepeatMode repeatMode = RepeatMode.shift;
                            Bounds horizRepeaterBounds = GetHorizRepeaterBounds(cellText, out frequency,
                                                                                out repeatMode);
                            if ((horizRepeaterBounds.Top >= topLimit && horizRepeaterBounds.Top <= bottomLimit) ||
                                (horizRepeaterBounds.Bottom >= topLimit && horizRepeaterBounds.Bottom <= bottomLimit))
                            {
                                if (horizRepeaterBounds.Left >= colInsertedAt)
                                {
                                    horizRepeaterBounds.Left += noOfColsInsertedShifted;
                                }
                                if (horizRepeaterBounds.Right >= colInsertedAt)
                                {
                                    horizRepeaterBounds.Right += noOfColsInsertedShifted;
                                }
                                cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                                     "<horizrepeat " +
                                                                                                     "top=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .Top.
                                                                                                         ToString() +
                                                                                                     " , " + "bottom=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .
                                                                                                         Bottom.
                                                                                                         ToString() +
                                                                                                     " , " + "left=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .Left.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "right=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .Right.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "frequency=" +
                                                                                                     frequency.ToString() +
                                                                                                     " , " + "mode=" +
                                                                                                     repeatMode.ToString
                                                                                                         ());
                                Document.SetCellValue(i, j, cellText);
                            }
                        }
                    }
                }
            }
        }

        private void IncreaseRepeatersColsRightOfHorizRepeatedZone(Bounds horizRepeatBounds, int startAtCol, int noOfColsInsertedShifted)
        {
            int topLimit = horizRepeatBounds.Top;
            int bottomLimit = horizRepeatBounds.Bottom;
            int leftLimit = startAtCol;
            int rightLimit = CurrentReportBounds.Right;
            for (int i = topLimit; i <= bottomLimit; i++)
            {
                for (int j = leftLimit; j <= rightLimit; j++)
                {
                    string cellText = Document.GetCellValueAsString(i, j);
                    if (HasHorizRepeaterDefined(cellText))
                    {
                        if (HasVertRepeaterDefined(cellText))
                        {
                            int startIndex = cellText.IndexOf("<vertrepeat");
                            int endIndex = cellText.IndexOf("/>", startIndex);
                            int frequency = 0;
                            RepeatMode repeatMode = RepeatMode.insert;
                            Bounds vertRepeatbounds = GetVertRepeaterBounds(cellText, out frequency, out repeatMode);

                            if ((vertRepeatbounds.Top >= topLimit && vertRepeatbounds.Top <= bottomLimit) ||
                                (vertRepeatbounds.Bottom >= topLimit && vertRepeatbounds.Bottom <= bottomLimit))
                            {
                                if (vertRepeatbounds.Left >= startAtCol)
                                {
                                    vertRepeatbounds.Left += noOfColsInsertedShifted;
                                }
                                if (vertRepeatbounds.Right >= startAtCol)
                                {
                                    vertRepeatbounds.Right += noOfColsInsertedShifted;
                                }
                                cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                                     "<vertrepeat " +
                                                                                                     "top=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Top.
                                                                                                         ToString() +
                                                                                                     " , " + "bottom=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Bottom.
                                                                                                         ToString() +
                                                                                                     " , " + "left=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Left.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "right=" +
                                                                                                     vertRepeatbounds.
                                                                                                         Right.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "frequency=" +
                                                                                                     frequency.ToString() +
                                                                                                     " , " + "mode=" +
                                                                                                     repeatMode.ToString
                                                                                                         ());

                                Document.SetCellValue(i, j, cellText);
                            }
                        }
                        if (HasHorizRepeaterDefined(cellText))
                        {
                            int startIndex = cellText.IndexOf("<horizrepeat");
                            int endIndex = cellText.IndexOf("/>", startIndex);
                            int frequency = 0;
                            RepeatMode repeatMode = RepeatMode.shift;
                            Bounds horizRepeaterBounds = GetHorizRepeaterBounds(cellText, out frequency,
                                                                                out repeatMode);
                            if ((horizRepeaterBounds.Top >= topLimit && horizRepeaterBounds.Top <= bottomLimit) ||
                                (horizRepeaterBounds.Bottom >= topLimit && horizRepeaterBounds.Bottom <= bottomLimit))
                            {
                                if (horizRepeaterBounds.Left >= startAtCol)
                                {
                                    horizRepeaterBounds.Left += noOfColsInsertedShifted;
                                }
                                if (horizRepeaterBounds.Right >= startAtCol)
                                {
                                    horizRepeaterBounds.Right += noOfColsInsertedShifted;
                                }
                                cellText = cellText.Remove(startIndex, endIndex - startIndex).Insert(startIndex,
                                                                                                     "<horizrepeat " +
                                                                                                     "top=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .Top.
                                                                                                         ToString() +
                                                                                                     " , " + "bottom=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .
                                                                                                         Bottom.
                                                                                                         ToString() +
                                                                                                     " , " + "left=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .Left.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "right=" +
                                                                                                     horizRepeaterBounds
                                                                                                         .Right.
                                                                                                         ToString() +
                                                                                                     " , " +
                                                                                                     "frequency=" +
                                                                                                     frequency.ToString() +
                                                                                                     " , " + "mode=" +
                                                                                                     repeatMode.ToString
                                                                                                         ());
                                Document.SetCellValue(i, j, cellText);
                            }
                        }
                    }
                }
            }
        }

        private Bounds GetHorizRepeaterBounds(string cellText, out int repeatFrequency, out RepeatMode repeatMode)
        {
            string fullCellText = cellText;
            var bounds = new Bounds();
            string originalText = cellText;
            repeatFrequency = 1;
            string repMode = "shift";
            if (HasHorizRepeaterDefined(cellText))
            {
                int horizRepeatDefStart = cellText.ToLower().IndexOf("<horizrepeat");
                int horizRepeatDefEnds = cellText.ToLower().IndexOf("/>", horizRepeatDefStart) + 2;
                string inSideText = cellText.Substring(horizRepeatDefStart + 12,
                                                       horizRepeatDefEnds - 2 - (horizRepeatDefStart + 12));
                string[] valuePairs = inSideText.Replace(" ", "").Split(new char[] { ',' }, inSideText.Length);
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
                        repMode = valuePair.Replace("mode=", "").ToLower().Trim();
                    }
                }
            }
            repeatMode = (RepeatMode)Enum.Parse(typeof(RepeatMode), repMode);

            return bounds;
        }

        private bool HasRepeaterDefinition(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").StartsWith("<vertrepeat") || cellText.ToLower().Replace(" ", "").StartsWith("<horizrepeat"))
            {
                return true;
            }
            return false;
        }

        private bool HasHorizRepeaterDefined(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").StartsWith("<horizrepeat"))
            {
                return true;
            }
            return false;
        }

        private bool HasVertRepeaterDefined(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").StartsWith("<vertrepeat"))
            {
                return true;
            }
            return false;
        }

        private void IncreaseAllUnParsedChartsHavingDataRightOfTheRepeaterZoneColsBy(int increaseByValue, int colsInsertedAt)
        {
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
                        foreach (string keyVal in keyVals)
                        {
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("left="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("left=", "");
                                int leftVal = int.Parse(val);
                                if (leftVal >= colsInsertedAt)
                                {
                                    leftVal = leftVal + increaseByValue;
                                }
                                newKeyVals.Add("left=" + leftVal.ToString());
                            }
                            else if (keyVal.Replace(" ", "").ToLower().StartsWith("right="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("right=", "");
                                int rightVal = int.Parse(val);
                                if (rightVal >= colsInsertedAt)
                                {
                                    rightVal = rightVal + increaseByValue;
                                }
                                newKeyVals.Add("right=" + rightVal.ToString());
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

        private void IncreaseAllUnParsedChartsHavingDataRightOfTheHorizRepeaterShiftZone(int increaseByValue, Bounds horizRepeatBounds, int repeatedTimes)
        {
            int row = horizRepeatBounds.Top;
            while (row <= horizRepeatBounds.Bottom)
            {
                int col = horizRepeatBounds.Left + (increaseByValue * repeatedTimes);
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
                        foreach (string keyVal in keyVals)
                        {
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("left="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("left=", "");
                                int leftVal = int.Parse(val);
                                if (leftVal >= horizRepeatBounds.Left + increaseByValue)
                                {
                                    leftVal = leftVal + increaseByValue;
                                }
                                newKeyVals.Add("left=" + leftVal.ToString());
                            }
                            else if (keyVal.Replace(" ", "").ToLower().StartsWith("right="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("right=", "");
                                int rightVal = int.Parse(val);
                                if (rightVal >= horizRepeatBounds.Left + increaseByValue)
                                {
                                    rightVal = rightVal + increaseByValue;
                                }
                                newKeyVals.Add("right=" + rightVal.ToString());
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

        private void IncreaseAllUnParsedChartsAtBottomHavingDataRigthOfTheHorizRepeaterShiftZone(int increaseByValue, Bounds horizRepeatBounds)
        {
            int row = horizRepeatBounds.Bottom;
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
                        int top = -1;
                        int bottom = -1;
                        foreach (string keyVal in keyVals)
                        {
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("top="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("top=", "");
                                top = int.Parse(val);
                            }
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("bottom="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("bottom=", "");
                                bottom = int.Parse(val);
                            }
                        }
                        foreach (string keyVal in keyVals)
                        {
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("left="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("left=", "");
                                int leftVal = int.Parse(val);
                                if (leftVal >= horizRepeatBounds.Left + increaseByValue && ((top >= horizRepeatBounds.Top && top <= horizRepeatBounds.Bottom) || (bottom >= horizRepeatBounds.Top && bottom <= horizRepeatBounds.Bottom) || (top <= horizRepeatBounds.Top && bottom >= horizRepeatBounds.Bottom)))
                                {
                                    leftVal = leftVal + increaseByValue;
                                }
                                newKeyVals.Add("left=" + leftVal.ToString());
                            }
                            else if (keyVal.Replace(" ", "").ToLower().StartsWith("right="))
                            {
                                string val = keyVal.Replace(" ", "").ToLower().Replace("right=", "");
                                int rightVal = int.Parse(val);
                                if (rightVal >= horizRepeatBounds.Left + increaseByValue && ((top >= horizRepeatBounds.Top && top <= horizRepeatBounds.Bottom) || (bottom >= horizRepeatBounds.Top && bottom <= horizRepeatBounds.Bottom)))
                                {
                                    rightVal = rightVal + increaseByValue;
                                }
                                newKeyVals.Add("right=" + rightVal.ToString());
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

        private void IncreaseRepeatedChartsColBy(int increaseByValue, int fromColNumber, int rowCopied, int colCopied)
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
                    if (keyVal.Replace(" ", "").ToLower().StartsWith("left="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("left=", "");
                        int leftVal = int.Parse(val);
                        if (leftVal >= fromColNumber)
                        {
                            leftVal = leftVal + increaseByValue;
                        }
                        newKeyVals.Add("left=" + leftVal.ToString());
                    }
                    else if (keyVal.Replace(" ", "").ToLower().StartsWith("right="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("right=", "");
                        int rightVal = int.Parse(val);
                        if (rightVal >= fromColNumber)
                        {
                            rightVal = rightVal + increaseByValue;
                        }
                        newKeyVals.Add("right=" + rightVal.ToString());
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

        public void CheckChartsInCopiedAndAdjustItsDataIfWithinRepeatedZone(int increaseByValue, int destinationCellRow, int destinationCellCol, Bounds horizontalRepeaterBounds)
        {
            string cellText = Document.GetCellValueAsString(destinationCellRow, destinationCellCol);
            if (HasChartDefinition(cellText))
            {
                int chartDefStartedAt = cellText.ToLower().IndexOf("<chart");
                int chartDefEndsAt = cellText.ToLower().IndexOf("/>", chartDefStartedAt);
                string chartDefFull = cellText.ToLower().Substring(chartDefStartedAt, chartDefEndsAt - chartDefStartedAt);
                string chartDef = chartDefFull.Replace(" ", "").Replace("<chart", "").Replace("/>", "");
                // This will hold the values defined in the chart tag
                string[] keyVals = chartDef.Split(new char[] { ',' }, chartDef.Length);
                string newChartDef = "";
                // This will hold the modified values which will be save later
                List<string> newKeyVals = new List<string>();
                int chartDataTop = -1;
                int chartDataBottom = -1;
                int chartDataLeft = -1;
                int chartDataRight = -1;
                foreach (string keyVal in keyVals)
                {
                    if (keyVal.Replace(" ", "").ToLower().StartsWith("top="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("top=", "");
                        chartDataTop = int.Parse(val);
                        newKeyVals.Add("top=" + chartDataTop.ToString());
                    }
                    else
                        if (keyVal.Replace(" ", "").ToLower().StartsWith("bottom="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("bottom=", "");
                        chartDataBottom = int.Parse(val);
                        newKeyVals.Add("bottom=" + chartDataBottom.ToString());
                    }
                    else
                            if (keyVal.Replace(" ", "").ToLower().StartsWith("left="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("left=", "");
                        chartDataLeft = int.Parse(val);
                    }
                    else
                                if (keyVal.Replace(" ", "").ToLower().StartsWith("right="))
                    {
                        string val = keyVal.Replace(" ", "").ToLower().Replace("right=", "");
                        chartDataRight = int.Parse(val);
                    }
                    else
                    {
                        newKeyVals.Add(keyVal);
                    }
                }

                //Check if chart's data is within the top and bottom bounds
                if ((chartDataTop >= horizontalRepeaterBounds.Top && chartDataBottom <= horizontalRepeaterBounds.Bottom) ||
                                        (chartDataBottom >= horizontalRepeaterBounds.Top && chartDataBottom <= horizontalRepeaterBounds.Bottom) || (chartDataTop <= horizontalRepeaterBounds.Top && chartDataBottom >= horizontalRepeaterBounds.Bottom))
                {
                    if (chartDataRight >= horizontalRepeaterBounds.Left)
                    {
                        chartDataRight += increaseByValue;
                        newKeyVals.Add("right=" + chartDataRight.ToString());
                    }
                    if (chartDataLeft >= horizontalRepeaterBounds.Left)
                    {
                        chartDataLeft += increaseByValue;
                        newKeyVals.Add("left=" + chartDataLeft.ToString());
                    }
                }

                //Recreate chart tag
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
                //Insert new chart tag whic is corrected
                cellText = cellText.Remove(chartDefStartedAt, chartDefEndsAt);
                cellText = cellText.Insert(chartDefStartedAt, newChartDef);
                Document.SetCellValue(destinationCellRow, destinationCellCol, cellText);
            }
        }
    }
}