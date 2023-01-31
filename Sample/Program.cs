// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using SharpLightReporting_SalesReporting;

Console.WriteLine("Hello, World!");
SharpLightReporting.ReportEngine reportEngine = new SharpLightReporting.ReportEngine();
reportEngine.NotifyReportLogEvent += (x => Console.WriteLine(x));
reportEngine.ProcessReport("SalesReportTemplate.xlsx", "SalesReport.xlsx", new SalesReportModel());
Process.Start("SalesReport.xlsx");