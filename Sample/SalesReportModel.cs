using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLightReporting_SalesReporting
{
    public class SalesReportModel : SharpLightReporting.IReportModel
    {
        //Constructor to initialize properties, collections and fetch data to fill the model

        #region Ctor

        public SalesReportModel()
        {
            this.CompanyName = "Reporting Company LLC.";
            this.ReportDate = DateTime.Now;
            this.FillData();
        }

        #endregion Ctor

        //Properties with same name as variables in ReportTemplate

        #region ReportFields

        public string CompanyName { get; set; }
        public DateTime ReportDate { get; set; }
        public string CityName { get { return this.DataCache[CurrentCityIndex].CityName; } }
        public string CityPictureLink { get { return this.DataCache[CurrentCityIndex].CityPic; } }

        public decimal TotalCurrentMonth
        {
            get
            {
                decimal val = 0.0M;
                foreach (var mon in this.DataCache[CurrentCityIndex].MonthlySales)
                {
                    val = val + mon.TotalSales;
                }
                return val;
            }
        }

        public string PictureFormat
        {
            get
            {
                return "jpeg";
            }
        }

        public string Month { get { return this.DataCache[CurrentCityIndex].MonthlySales[CurrentMonthIndexWithinCurrentCity].MonthName; } }
        public decimal SaleAmount { get { return this.DataCache[CurrentCityIndex].MonthlySales[CurrentMonthIndexWithinCurrentCity].TotalSales; } }

        #endregion ReportFields

        //Number of time a region is repeated is not know at design time so this properties will serve as variables within repeat tags

        #region RepeatTagsDynamicValues

        public string CityCount { get { if (this.DataCache.Count > 0) { return this.DataCache.Count.ToString(); } else { return 1.ToString(); } } }

        public int MonthCount
        {
            get
            {
                if (this.DataCache.Count > 0)
                {
                    return this.DataCache[CurrentCityIndex].MonthlySales.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion RepeatTagsDynamicValues

        //Methods with same name as methods in ReportTemplate

        #region MethodsAndSupportingFields

        private int CurrentCityIndex = 0;
        private int CurrentMonthIndexWithinCurrentCity = 0;

        public void NextCity()
        {
            if (!(this.CurrentCityIndex >= DataCache.Count - 1))
            {
                CurrentCityIndex++;
                // Everytime city changes start from first month
                this.CurrentMonthIndexWithinCurrentCity = 0;
            }
        }

        public void NextMonth()
        {
            if (!(this.CurrentMonthIndexWithinCurrentCity >= this.DataCache[CurrentCityIndex].MonthlySales.Count - 1))
            {
                CurrentMonthIndexWithinCurrentCity++;
            }
        }

        #endregion MethodsAndSupportingFields

        //We are using this method to fill the model with dummy values. In production one may use database calls etc.

        #region FetchingDataLogic

        private List<CityData> DataCache = new List<CityData>();

        private void FillData()
        {
            //You may usually FetchData from a database but for the purpose of demo we will create are own dummy data

            var city1 = new CityData();
            city1.CityName = "Paris";
            city1.CityPic = "https://www.100resilientcities.org/wp-content/uploads/2017/06/Paris-hero-crop-e1523025669824.jpg";
            city1.MonthlySales.Add(new MonthlySalesData() { MonthName = "January", TotalSales = 200000 });
            city1.MonthlySales.Add(new MonthlySalesData() { MonthName = "February", TotalSales = 150000 });
            city1.MonthlySales.Add(new MonthlySalesData() { MonthName = "March", TotalSales = 75000 });
            city1.MonthlySales.Add(new MonthlySalesData() { MonthName = "April", TotalSales = 100000 });
            city1.MonthlySales.Add(new MonthlySalesData() { MonthName = "May", TotalSales = 250000 });
            city1.MonthlySales.Add(new MonthlySalesData() { MonthName = "June", TotalSales = 225000 });
            this.DataCache.Add(city1);

            var city2 = new CityData();
            city2.CityName = "London";
            city2.CityPic = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6d/City_of_London_skyline_from_London_City_Hall_-_Sept_2015_-_Crop_Aligned.jpg/1200px-City_of_London_skyline_from_London_City_Hall_-_Sept_2015_-_Crop_Aligned.jpg";
            city2.MonthlySales.Add(new MonthlySalesData() { MonthName = "January", TotalSales = 100000 });
            city2.MonthlySales.Add(new MonthlySalesData() { MonthName = "February", TotalSales = 250000 });
            city2.MonthlySales.Add(new MonthlySalesData() { MonthName = "March", TotalSales = 35000 });
            city2.MonthlySales.Add(new MonthlySalesData() { MonthName = "April", TotalSales = 200000 });
            city2.MonthlySales.Add(new MonthlySalesData() { MonthName = "May", TotalSales = 270000 });
            city2.MonthlySales.Add(new MonthlySalesData() { MonthName = "June", TotalSales = 255000 });
            this.DataCache.Add(city2);

            var city3 = new CityData();
            city3.CityName = "New York";
            city3.CityPic = "https://media.architecturaldigest.com/photos/5699802bc6772b7614567435/4:3/w_768/new-york-city-guide.jpg";
            city3.MonthlySales.Add(new MonthlySalesData() { MonthName = "January", TotalSales = 30000 });
            city3.MonthlySales.Add(new MonthlySalesData() { MonthName = "February", TotalSales = 150000 });
            city3.MonthlySales.Add(new MonthlySalesData() { MonthName = "March", TotalSales = 100000 });
            city3.MonthlySales.Add(new MonthlySalesData() { MonthName = "April", TotalSales = 90000 });
            city3.MonthlySales.Add(new MonthlySalesData() { MonthName = "May", TotalSales = 25000 });
            city3.MonthlySales.Add(new MonthlySalesData() { MonthName = "June", TotalSales = 235000 });
            this.DataCache.Add(city3);

            var city4 = new CityData();
            city4.CityName = "Dubai";
            city4.CityPic = "http://goldentreasuretourism.com/blog/wp-content/uploads/2016/05/dubai-city-tour-golden-treasure-tourism.jpg";
            city4.MonthlySales.Add(new MonthlySalesData() { MonthName = "January", TotalSales = 45000 });
            city4.MonthlySales.Add(new MonthlySalesData() { MonthName = "February", TotalSales = 190000 });
            city4.MonthlySales.Add(new MonthlySalesData() { MonthName = "March", TotalSales = 95500 });
            city4.MonthlySales.Add(new MonthlySalesData() { MonthName = "April", TotalSales = 122000 });
            city4.MonthlySales.Add(new MonthlySalesData() { MonthName = "May", TotalSales = 150000 });
            city4.MonthlySales.Add(new MonthlySalesData() { MonthName = "June", TotalSales = 325000 });
            this.DataCache.Add(city4);

            var city5 = new CityData();
            city5.CityName = "Delhi";
            city5.CityPic = "http://ste.india.com/sites/default/files/2017/10/13/631893-india-gate.jpg";
            city5.MonthlySales.Add(new MonthlySalesData() { MonthName = "January", TotalSales = 600000 });
            city5.MonthlySales.Add(new MonthlySalesData() { MonthName = "February", TotalSales = 726000 });
            city5.MonthlySales.Add(new MonthlySalesData() { MonthName = "March", TotalSales = 400000 });
            city5.MonthlySales.Add(new MonthlySalesData() { MonthName = "April", TotalSales = 50000 });
            city5.MonthlySales.Add(new MonthlySalesData() { MonthName = "May", TotalSales = 245000 });
            city5.MonthlySales.Add(new MonthlySalesData() { MonthName = "June", TotalSales = 467000 });
            this.DataCache.Add(city5);
        }

        #endregion FetchingDataLogic
    }

    #region DTOs

    public class CityData
    {
        public CityData()
        {
            this.MonthlySales = new List<MonthlySalesData>();
            this.CityName = "";
            this.CityName = "";
        }

        public string CityName { get; set; }
        public string CityPic { get; set; }

        public List<MonthlySalesData> MonthlySales { get; set; }
    }

    public class MonthlySalesData
    {
        public string MonthName { get; set; }
        public decimal TotalSales { get; set; }
    }

    #endregion DTOs
}