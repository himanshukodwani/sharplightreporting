using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpreadsheetLight;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        private void AdjustRowInsertedListOnRowRemoval(int rowRemovedAt, int noOfRowsRemoved)
        {
            foreach (var item in this.RowsInserted)
            {
                if (item.InsertedAt >= rowRemovedAt)
                {
                    item.InsertedAt = item.InsertedAt - noOfRowsRemoved;
                }
            }
        }

        private void AdjustColInsertedListOnColRemoval(int colRemovedAt, int noOfColsRemoved)
        {
            foreach (var item in this.ColsInseted)
            {
                if (item.InsertedAt >= colRemovedAt)
                {
                    item.InsertedAt = item.InsertedAt - noOfColsRemoved;
                }

            }
        }

        private bool IsRowDeleted(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").Contains("<deleterow/>"))
            {
                return true;
            }
            return false;
        }

        private bool IsColumnDeleted(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").Contains("<deletecol/>"))
            {
                return true;
            }
            return false;
        }

        private bool MoveNextColumn()
        {
            if (CurrentColumn < CurrentReportBounds.Right)
            {
                if (CurrentColumn == 0)
                {
                    CurrentColumn = CurrentReportBounds.Left;
                    return true;
                }
                else
                {
                    CurrentColumn = CurrentColumn + 1;
                }

                return true;
            }
            return false;
        }

        private bool MoveNextRow()
        {
            if (CurrentRow < CurrentReportBounds.Bottom)
            {
                if (CurrentRow == 0)
                {
                    CurrentRow = CurrentReportBounds.Top;
                    return true;
                }
                else
                {
                    CurrentRow = CurrentRow + 1;
                    //Marking it 0 will automatically assign CurrentReportBounds.Left
                    CurrentColumn = 0;
                }

                return true;
            }
            return false;
        }

        private void ShiftRowDown(int startAtCol, int endingAtCol, int rowFromWhere, int numberofPlacesToMove)
        {
            int bot = CurrentReportBounds.Bottom;

            while (bot >= rowFromWhere)
            {
                int col = startAtCol;
                while (col <= endingAtCol)
                {
                    Document.CopyCell(bot, col, bot + numberofPlacesToMove, col, true);

                    col = col + 1;
                }
                bot = bot - 1;
            }
            CurrentReportBounds.Bottom += numberofPlacesToMove;

        }

        private void ShiftColRight(int startingAtRow, int endingAtRow, int colFromWhere, int numberofPlacesToMove)
        {
            int rit = CurrentReportBounds.Right;
            while (rit >= colFromWhere)
            {
                int row = startingAtRow;
                while (row <= endingAtRow)
                {
                    string cellValue = Document.GetCellValueAsString(row, rit);

                    Document.CopyCell(row, rit, row, rit + numberofPlacesToMove, SLPasteTypeValues.Paste);

                    row = row + 1;
                }
                rit = rit - 1;
            }
            CurrentReportBounds.Right += numberofPlacesToMove;
        }

    }
}
