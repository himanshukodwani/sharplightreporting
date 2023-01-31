# SharpLightReporting

SharpLightReporting is a .Net reporting system where developers design a report using excel like software and insert supported template markers for dynamic content insertion and calling methods on the datasource.
This dynamic content is defined in a custom data class (IReportData class) where custom properties behave as the source for this dynamic content and custom methods could be used to change the data like NextRecord() etc. resulting in a very easy, simple and flexible reporting system.
End users can modify reporting template when in production use, using their standard spreadsheet program and developers do not need to learn any new report designing system. 
Tip : You may use Reogrid control to view the report or edit the template. Please search unvell.Reogrid on nuget or github. Source code of Reogrid contains a fully functional Reogrid/Spreadsheet editor which you may modify and use in your own .net projects.
Please read template marker reference here : https://github.com/himanshukodwani/sharplightreporting/wiki/Template-Tag-Reference
