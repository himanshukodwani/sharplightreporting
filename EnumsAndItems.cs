using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using SpreadsheetLight.Charts;
using SpreadsheetLight.Drawing;

namespace SharpLightReporting
{
    public class Bounds
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }


    }

    public class StringKeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    
    
    public class RowsInsertedItem
    {
        public RowsInsertedItem(int insertedAt, int insertedCount)
        {
            InsertedAt = insertedAt;
            InsertedCount = insertedCount;
        }
        public int InsertedAt { get; set; }
        public int InsertedCount { get; set; }
    }
    public class RowsShiftedItem
    {
        public RowsShiftedItem(int rowFromWhichShifted, int shiftedCount, int startColumn, int endColumn)
        {
            RowFromWhichShifted = rowFromWhichShifted;
            ShiftedCount = shiftedCount;
            StartColumn = startColumn;
            EndColumn = endColumn;
        }
        public int RowFromWhichShifted { get; set; }
        public int ShiftedCount { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

    }
    public class ColsInsertedItem
    {
        public ColsInsertedItem(int insertedAt, int insertedCount)
        {
            InsertedAt = insertedAt;
            InsertedCount = insertedCount;
        }
        public int InsertedAt { get; set; }
        public int InsertedCount { get; set; }
    }
    public class ColsShiftedItem
    {
        public ColsShiftedItem(int colFromWhichShifted, int shiftedCount, int startRow, int endRow)
        {
            ColFromWhichShifted = colFromWhichShifted;
            ShiftedCount = shiftedCount;
            StartRow = startRow;
            EndRow = endRow;
        }
        public int ColFromWhichShifted { get; set; }
        public int ShiftedCount { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
    }



    public enum RepeatMode
    {
        insert, shift, overwrite
    }

    public interface IReportModel
    {

    }
}
