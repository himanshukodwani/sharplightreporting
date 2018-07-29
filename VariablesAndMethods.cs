using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        private string RemoveMethodDef(string cellText)
        {
            string modifiedtext = cellText.ToLower();

            int methodDefStartIndex = modifiedtext.IndexOf("<method");
            int methodDefEndIndex = modifiedtext.IndexOf("/>", methodDefStartIndex) + 2;
            modifiedtext = cellText.Remove(methodDefStartIndex, methodDefEndIndex - methodDefStartIndex);
            return modifiedtext;

        }

        private bool HasMethodDefinition(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").Contains("<method"))
            {
                return true;
            }
            return false;
        }

        private string GetMethodName(string cellText)
        {
            string methodName = "";
            if (HasMethodDefinition(cellText))
            {
                string modifiedtext = cellText;

                int methodDefStartIndex = modifiedtext.IndexOf("<method");
                int methodDefEndIndex = modifiedtext.IndexOf("/>", methodDefStartIndex) + 2;
                string internalText = modifiedtext.Substring(methodDefStartIndex,
                                                             methodDefEndIndex - methodDefStartIndex).Replace("<method", "").Replace("/>", "");
                string[] keyVals = internalText.Split(new char[] { ',' }, internalText.Length);
                foreach (string keyVal in keyVals)
                {
                    if (keyVal.Replace(" ", "").StartsWith("name="))
                    {
                        methodName = keyVal.Replace("name=", "");
                        break;
                    }
                }

                return methodName;
            }
            else
            {
                throw new Exception("No method name found");
            }
        }
        
        private void SetCellValuesAsPerFormatDefined(int CurrRow, int CurrColumn, string val)
        {
            if (HasCellFormatDefined(val))
            {

                int FormatDefinitionStartsAt = val.ToLower().IndexOf("<cellformat");
                int FormatDefinitionEndsAt = val.ToLower().IndexOf("/>", FormatDefinitionStartsAt);
                string FormatDefinition = val.ToLower().Substring(FormatDefinitionStartsAt,
                                                                  FormatDefinitionEndsAt + 2 - FormatDefinitionStartsAt);
                val = val.Remove(FormatDefinitionStartsAt, FormatDefinitionEndsAt + 2 - FormatDefinitionStartsAt);
                string valFormat =
                    FormatDefinition.Replace("<", "").Replace("cellformat", "").Replace("=", "").Replace(" ", "").
                        Replace("/>", "");
                switch (valFormat)
                {
                    case "number":
                        {
                            Document.SetCellValue(CurrRow, CurrColumn, long.Parse(val));
                            break;
                        }
                    case "decimal":
                        {
                            Document.SetCellValue(CurrRow, CurrColumn, double.Parse(val));
                            break;
                        }
                    case "currency":
                        {
                            Document.SetCellValue(CurrRow, CurrColumn, decimal.Parse(val));
                            break;
                        }
                    case "datetime":
                        {
                            Document.SetCellValue(CurrRow, CurrColumn, DateTime.Parse(val));
                            break;

                        }
                    case "bool":
                        {
                            Document.SetCellValue(CurrRow, CurrColumn, bool.Parse(val));

                            break;
                        }
                    default:
                        {

                            Document.SetCellValue(CurrRow, CurrColumn, val);
                            break;
                        }

                }

            }
            else
            {
                Document.SetCellValue(CurrRow, CurrColumn, val);
                
            }
        }

        private bool HasCellFormatDefined(string cellText)
        {
            if (cellText.ToLower().Trim().Replace(" ", "").Contains("<cellformat"))
            {
                return true;
            }
            return false;
        }

        private bool HasVariable(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").Contains("<variable"))
            {
                return true;

            }
            return false;
        }
        
        private string ReplaceVriablesWithValue(string cellText)
        {
            if (HasVariable(cellText))
            {
                while (HasVariable(cellText))
                {


                    cellText = ProcessVariableFound(cellText);
                }

            }

            return cellText;

        }

        private string ProcessVariableFound(string cellText)
        {
            int variableStartIndex = cellText.ToLower().IndexOf("<variable");
            int variableEndIndex = cellText.ToLower().Substring(variableStartIndex).IndexOf("/>") +
                                   variableStartIndex + 2;
            string[] KeyVals =
                cellText.Substring(variableStartIndex, variableEndIndex - variableStartIndex).Replace(
                    "<variable", "").Replace("/>", "").Replace(" ", "").Split(new char[] { ',' },
                                                                              cellText.Length - 1);
            string propName = "";
            string retval;
            foreach (var keyVal in KeyVals)
            {
                if (keyVal.ToLower().StartsWith("name="))
                {
                    propName = keyVal.Substring(5, keyVal.Length - 5);
                    break;
                }
            }
            if (!String.IsNullOrEmpty(propName))
            {
                try
                {

                
                    cellText = cellText.Remove(variableStartIndex, variableEndIndex - variableStartIndex);
                    cellText = cellText.Insert(variableStartIndex, GetVariableValue(propName).ToString());
                
                }
                catch
                {

                    NotifyReportLogEvent("Coul not process variable having property name : " + propName);
                }
           }
            return cellText;
        }

        private object GetVariableValue(string variableName)
        {
                 
               
            
                object retval = _reportModelData.GetType().GetProperty(variableName).GetGetMethod().Invoke(_reportModelData, null);
                return retval;
            

        }

        private void CallMethod(string MethodName)
        {
            try
            {
                _reportModelData.GetType().GetMethod(MethodName.Trim()).Invoke(_reportModelData, null);
            }
            catch
            {

                NotifyReportLogEvent("Could not call method :" + MethodName);
            }



        }
    }    
}
