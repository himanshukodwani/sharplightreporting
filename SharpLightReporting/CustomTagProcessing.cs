using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        private bool HasCustomTag(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").Contains("<customtag"))
            {
                return true;
            }
            return false;
        }

        private string CallCustomTags(string cellText)
        {
            string ReturnCellText = cellText;
            try
            {

              
            while (HasCustomTag(ReturnCellText))
            {
                string tagName = "";
                string tagDef = "";
                List<StringKeyValue> tagParams = new List<StringKeyValue>();
                int startindexofcustomTag = ReturnCellText.ToLower().IndexOf("<customtag");
                int endindexofcustomTag = ReturnCellText.ToLower().IndexOf("/>", startindexofcustomTag, cellText.Length);
                tagDef = ReturnCellText.Substring(startindexofcustomTag, endindexofcustomTag + 2 - startindexofcustomTag);
                ReturnCellText = ReturnCellText.Remove(startindexofcustomTag,
                                                       endindexofcustomTag + 2 - startindexofcustomTag);
                tagDef = tagDef.ToLower().Replace(" ", "").Replace("<customtag", "").Replace("/>", "");
                string[] rawParams = tagDef.Split(new char[] { ',' }, tagDef.Length);
                foreach (string rawParam in rawParams)
                {
                    if (rawParam.ToLower().Trim().StartsWith("name="))
                    {
                        tagName = rawParam.ToLower().Replace("name=", "");
                    }
                    else
                    {
                        try
                        {
                            string[] splitedParams = rawParam.Split(new char[] { '=' }, rawParam.Length);
                            tagParams.Add(new StringKeyValue() { Key = splitedParams[0], Value = splitedParams[1] });
                        }
                        catch
                        {

                           NotifyReportLogEvent("Custom tag params should have key and value seperated by =. Name param is reserved in custom tags");
                        }



                    }
                }
                if (this.CustomTagFound != null)
                {
                    CustomTagFound(this.Document, CurrentRow, CurrentColumn, tagName, tagParams);
                }


            }
            }
            catch
            {

                NotifyReportLogEvent("Coul not process custom tag.");
            }
            return ReturnCellText;
        }
    }
}
