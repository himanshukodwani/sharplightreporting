using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        private void PutAllPageBreaksOnReport()
        {
            foreach (var pageBreak in this.PageBreaks)
            {
                Document.InsertPageBreak(pageBreak.row + 1, pageBreak.col);
            }
        }
        private void AdjustPageBreaksOnColumnRemoved(int colIndexRemoved, int noOfCols)
        {
            foreach (var pageBreak in this.PageBreaks)
            {
                if (colIndexRemoved <= pageBreak.col)
                {
                    pageBreak.col += noOfCols;
                }
            }
        }

        // Since rows are removed in the last sequence after the parser has completed all page breaks after the row being removed need to be caliberated
        private void AdjustPageBreaksOnRowsRemoved(int rowIndexRemoved, int noOfRows)
        {
            foreach (var pageBreak in this.PageBreaks)
            {
                if (rowIndexRemoved <= pageBreak.row)
                {
                    pageBreak.row -= noOfRows;
                }
            }
        }

        //If rows are added then there will be no effect on previously define page breaks and next pagebreaks will automatically be parsed based on their position which will change with row addition automatically. So this method is not required.
        //private void AdjustPageBreaksOnRowAdded(int rowIndexAdded, int noOfRows)
        //{
        //    foreach (var pageBreak in this.PageBreaks)
        //    {
        //        if (rowIndexAdded <= pageBreak.row)
        //        {
        //            pageBreak.row += noOfRows;
        //        }
        //    }
        //}

        //If a column is added or removed then the page breaks previously defined need to be clibrated
        private void AdjustPageBreakOnColumnAdded(int colIndexAdded, int noOfCols)
        {
            foreach (var pageBreak in this.PageBreaks)
            {
                if (colIndexAdded <= pageBreak.col)
                {
                    pageBreak.col += noOfCols;
                }
            }
        }

        private bool HasPageBreak(string cellText)
        {
            if (cellText.ToLower().Contains("<pagebreak/>"))
            {
                return true;
            }
            return false;
        }

        private string RemovePageBreakDef(string cellText)
        {
            int pageBreakStartsAt = cellText.ToLower().IndexOf("<pagebreak/>");
            return cellText.Remove(pageBreakStartsAt, "<pagebreak/>".Length);
        }
    }

    public class PageBreakItems
    {
        public PageBreakItems(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
        public int row { get; set; }
        public int col { get; set; }
    }
}
