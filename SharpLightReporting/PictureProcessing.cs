using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using SpreadsheetLight.Drawing;

namespace SharpLightReporting
{
    public partial class ReportEngine
    {
        public void AddPicture(string pictureName, SLPicture picture, double rowPos, double colPos)
        {
            
            this.ReportPictures.Add(new ReportPictureItem()
            {
                PictureName = pictureName,
                PictureColPosition = colPos,
                PictureRowPosition = rowPos,
                ReportPicture = picture
            });

        }

        private void PutAllPicturesOnReport()
        {
            foreach (var picItem in ReportPictures)
            {
                if (picItem.PicData != null && picItem.PictureFormat != null)
                {
                    picItem.ReportPicture = new SLPicture(picItem.PicData, picItem.PictureFormat);
                    picItem.ReportPicture.SetPosition(picItem.PictureRowPosition - 1 + picItem.AddToRowPosition, picItem.PictureColPosition - 1 + picItem.AddToColPosition);
                    picItem.ReportPicture.ResizeInEMU(picItem.PictureWidthEMU, picItem.PictureHeightEMU);
                    try
                    {
                        Document.InsertPicture(picItem.ReportPicture);
                    }
                    catch
                    {

                        NotifyReportLogEvent("Could ot insert the picture");
                    }
                    
                }
            }
        }

        private void AdjustPicturePositionOnRowRemoved(int rowRemovedAt, int noOfRowsRemoved)
        {
            foreach (var pic in ReportPictures)
            {
                if (pic.PictureRowPosition >= rowRemovedAt)
                {
                    pic.PictureRowPosition = pic.PictureRowPosition - noOfRowsRemoved;
                }
            }
        }

        private void AdjustPicturePositionOnColumnRemoved(int colRemovedAt, int noOfColsRemoved)
        {
            foreach (var pic in ReportPictures)
            {
                if (pic.PictureColPosition >= colRemovedAt)
                {
                    pic.PictureColPosition = pic.PictureColPosition - noOfColsRemoved;
                }
            }
        }

        private void AdjustPicturePositionOnColInsert(int colInsert, int noOfColsInserted)
        {
            foreach (var pic in ReportPictures)
            {
                if (pic.PictureColPosition >= colInsert)
                {
                    pic.PictureColPosition = pic.PictureColPosition + noOfColsInserted;
                }
            }
        }

        private void AdjustPicturePositionOnColShift(int colInsert, int noOfColsInserted, int rowTop, int rowBottom)
        {
            foreach (var pic in ReportPictures)
            {
                if (pic.PictureColPosition >= colInsert &&
                    (pic.PictureRowPosition >= rowTop && pic.PictureRowPosition <= rowBottom))
                {
                    pic.PictureColPosition = pic.PictureColPosition + noOfColsInserted;
                }
            }

        }

        private void ProcessPicture(string cellText)
        {
            if (HasPictureDefinition(cellText))
            {
                int picDefStart = cellText.ToLower().IndexOf(@"<pic");
                int picDefEnd = cellText.ToLower().IndexOf(@"/>", picDefStart);
                string picDefinition = cellText.Substring(picDefStart , picDefEnd - picDefStart);
                picDefinition = picDefinition.Replace(@"<pic", "").Replace(@"/>", "");
                string[] picAtts = picDefinition.Split(new char[] { ',' }, picDefinition.Length);
                var picItem = new ReportPictureItem();
                picItem.PictureRowPosition = this.CurrentRow;
                picItem.PictureColPosition = this.CurrentColumn;
                picItem.AddToColPosition = 0.0;
                picItem.AddToRowPosition = 0.0;
                string picreftype = "";
                string picref = "";
                
                foreach (string picAtt in picAtts)
                {

                    try
                    {

                    

                    if (picAtt.ToLower().Replace(" ", "").StartsWith("name="))
                    {
                        picItem.PictureName = picAtt.ToLower().Replace(" ", "").Replace("name=", "");
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("heightemu="))
                    {
                        picItem.PictureHeightEMU = long.Parse(picAtt.ToLower().Replace(" ", "").Replace("heightemu=", ""));
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("widthemu="))
                    {
                        picItem.PictureWidthEMU = long.Parse(picAtt.ToLower().Replace(" ", "").Replace("widthemu=", ""));
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("heightcm="))
                    {
                        picItem.PictureHeightEMU = long.Parse((double.Parse(picAtt.ToLower().Replace(" ", "").Replace("heightcm=", "")) * 360000).ToString());
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("widthcm="))
                    {
                        picItem.PictureWidthEMU = long.Parse((double.Parse(picAtt.ToLower().Replace(" ", "").Replace("widthcm=", "")) * 360000).ToString());
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("heightinch="))
                    {
                        picItem.PictureHeightEMU = long.Parse((double.Parse(picAtt.ToLower().Replace(" ", "").Replace("heightinch=", "")) * 914400).ToString());
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("widthinch="))
                    {
                        picItem.PictureWidthEMU = long.Parse((double.Parse(picAtt.ToLower().Replace(" ", "").Replace("widthinch=", "")) * 914400).ToString());
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("addtocolpos="))
                    {
                        picItem.AddToColPosition = double.Parse(picAtt.ToLower().Replace(" ", "").Replace("addtocolpos=", "").ToString());
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("addtorowpos="))
                    {
                        picItem.AddToRowPosition = double.Parse(picAtt.ToLower().Replace(" ", "").Replace("addtorowpos=", "").ToString());
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("ref="))
                    {
                        var temp = picAtt.Replace(" ", "");
                        int startInd = picAtt.ToLower().Replace(" ", "").IndexOf("ref=");

                        temp = temp.Remove(startInd, 4);

                        picref = temp;

                        
                       

                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("reftype="))
                    {
                        picreftype = picAtt.ToLower().Replace(" ", "").Replace("reftype=", "");
                        
                    }
                    if (picAtt.ToLower().Replace(" ", "").StartsWith("format="))
                    {
                       


                            if (picAtt.ToLower().Replace(" ", "").EndsWith("jpeg") ||
                               (picAtt.ToLower().Replace(" ", "").EndsWith("jpg")))
                            {
                                picItem.PictureFormat = ImagePartType.Jpeg;
                            }
                            else if (picAtt.ToLower().Replace(" ", "").EndsWith("wmf"))
                            {
                                picItem.PictureFormat = ImagePartType.Wmf;
                            }
                            else if (picAtt.ToLower().Replace(" ", "").EndsWith("tif") ||
                               picAtt.ToLower().Replace(" ", "").EndsWith("tiff"))
                            {
                                picItem.PictureFormat = ImagePartType.Tiff;
                            }else if (picAtt.ToLower().Replace(" ", "").EndsWith("png"))
                            {
                                picItem.PictureFormat = ImagePartType.Png;
                            }else if (picAtt.ToLower().Replace(" ", "").EndsWith("pcx"))
                            {
                                picItem.PictureFormat = ImagePartType.Pcx;
                            }else if (picAtt.ToLower().Replace(" ", "").EndsWith("ico"))
                            {
                                picItem.PictureFormat = ImagePartType.Icon;
                            }else if (picAtt.ToLower().Replace(" ", "").EndsWith("gif"))
                            {
                                picItem.PictureFormat = ImagePartType.Gif;
                            }else if (picAtt.ToLower().Replace(" ", "").EndsWith("emf"))
                            {
                                picItem.PictureFormat = ImagePartType.Emf;
                            }
                            else
                            {
                                NotifyReportLogEvent("Picture format is not supported.");
                                return;
                            }
                           

                       
                    }

                    }
                    catch
                    {

                        NotifyReportLogEvent("Was not able to process picture attribute. " + picAtt);
                        return;
                    }
                }
                if (!String.IsNullOrEmpty(picreftype) && !String.IsNullOrEmpty(picref))
                {
                    if (picreftype.ToLower().Replace(" ", "") == PictureSourceTypeEnum.fileref.ToString() || picreftype.ToLower().Replace(" ", "") == "file")
                    {
                       
                       

                            if (File.Exists(picref))
                            {

                                
                                if (picref.ToLower().Replace(" ", "").EndsWith("jpeg") ||
                                    picref.ToLower().Replace(" ", "").EndsWith("jpg"))
                                {
                                    picItem.PictureFormat = ImagePartType.Jpeg;
                                }
                                if (picref.ToLower().Replace(" ", "").EndsWith("wmf"))
                                {
                                    picItem.PictureFormat = ImagePartType.Wmf;
                                }
                                if (picref.ToLower().Replace(" ", "").EndsWith("tif") ||
                                    picref.ToLower().Replace(" ", "").EndsWith("tiff"))
                                {
                                    picItem.PictureFormat = ImagePartType.Tiff;
                                }
                                if (picref.ToLower().Replace(" ", "").EndsWith("png"))
                                {
                                    picItem.PictureFormat = ImagePartType.Png;
                                }
                                if (picref.ToLower().Replace(" ", "").EndsWith("pcx"))
                                {
                                    picItem.PictureFormat = ImagePartType.Pcx;
                                }
                                if (picref.ToLower().Replace(" ", "").EndsWith("ico"))
                                {
                                    picItem.PictureFormat = ImagePartType.Icon;
                                }
                                if (picref.ToLower().Replace(" ", "").EndsWith("gif"))
                                {
                                    picItem.PictureFormat = ImagePartType.Gif;
                                }
                                if (picref.ToLower().Replace(" ", "").EndsWith("emf"))
                                {
                                    picItem.PictureFormat = ImagePartType.Emf;
                                }
                                if (picItem.PictureFormat != null)
                                {
                                    picItem.PicData = File.ReadAllBytes(picref);
                                }
                                else
                                {
                                    NotifyReportLogEvent("Picture format is not supported.");
                                    return; 
                                }
                            }
                            else
                            {
                                NotifyReportLogEvent("Picture referenced file not found : " + picref);
                            }
                        
                    }
                    if (picreftype.ToLower().Replace(" ", "") == PictureSourceTypeEnum.webref.ToString() || picreftype.ToLower() == "web")
                    {
                       

                           

                            ImagePartType imagePartType = ImagePartType.Bmp;
                            if (picref.ToLower().Replace(" ", "").EndsWith("jpeg") ||
                               picref.ToLower().Replace(" ", "").EndsWith("jpg"))
                            {
                                picItem.PictureFormat = ImagePartType.Jpeg;
                            }
                            if (picref.ToLower().Replace(" ", "").EndsWith("wmf"))
                            {
                                picItem.PictureFormat = ImagePartType.Wmf;
                            }
                            if (picref.ToLower().Replace(" ", "").EndsWith("tif") ||
                               picref.ToLower().Replace(" ", "").EndsWith("tiff"))
                            {
                                picItem.PictureFormat = ImagePartType.Tiff;
                            }
                            if (picref.ToLower().Replace(" ", "").EndsWith("png"))
                            {
                                picItem.PictureFormat = ImagePartType.Png;
                            }
                            if (picref.ToLower().Replace(" ", "").EndsWith("pcx"))
                            {
                                picItem.PictureFormat = ImagePartType.Pcx;
                            }
                            if (picref.ToLower().Replace(" ", "").EndsWith("ico"))
                            {
                                picItem.PictureFormat = ImagePartType.Icon;
                            }
                            if (picref.ToLower().Replace(" ", "").EndsWith("gif"))
                            {
                                picItem.PictureFormat = ImagePartType.Gif;
                            }
                            if (picref.ToLower().Replace(" ", "").EndsWith("emf"))
                            {
                                picItem.PictureFormat = ImagePartType.Emf;
                            }
                            if (picItem.PictureFormat != null)
                            {
                            try
                            {
                                WebClient client = new WebClient();
                                picItem.PicData = client.DownloadData(picref);
                            }
                            catch { }
                            }
                            else
                            {
                                NotifyReportLogEvent("Picture format is not supported.");
                                return;
                            }
                      


                    }
                    if (picreftype.ToLower() == PictureSourceTypeEnum.prop.ToString() || picreftype.ToLower() == "property" || picreftype.ToLower() == "var" || picreftype.ToLower() == "variable")
                    {
                        try
                        {
                            bool propFound = false;
                            foreach (var prop in this._reportModelData.GetType().GetProperties())
                            {
                                if(prop.Name.Trim() == picref.Trim())
                                {
                                    picItem.PicData =
                                        (byte[]) prop.GetValue(this._reportModelData, null);
                                    propFound = true;
                                    break;
                                }
                            }
                            if(!propFound)
                            {
                                NotifyReportLogEvent("Property with the name : " + picref + "was not found");
                            }
                            
                        }
                        catch
                        {
                            NotifyReportLogEvent("Was not able to process property : " + picref );
                            return;
                        }
                    }
                }

                this.ReportPictures.Add(picItem);
            }

        }

        private bool HasPictureDefinition(string cellText)
        {
            if (cellText.ToLower().Replace(" ", "").Contains("<pic"))
            {
                return true;
            }
            return false;
        }
    }

    public class ReportPictureItem
    {
        public string PictureName { get; set; }
        public SLPicture ReportPicture { get; set; }
        public double PictureRowPosition { get; set; }
        public double PictureColPosition { get; set; }
        public long PictureWidthEMU { get; set; }
        public long PictureHeightEMU { get; set; }
        public byte[] PicData { get; set; }
        public ImagePartType PictureFormat { get; set; }
        public double AddToColPosition { get; set; }
        public double AddToRowPosition { get; set; }



    }
    public enum PictureSourceTypeEnum
    {
        fileref, webref, prop
    }
}
