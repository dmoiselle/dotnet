using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Bridge.Web.Utility;

namespace Bridge.Web.Controllers
{

    /// <summary>
    /// Extension methods to support Excel export from a controller using
    /// Excel method similar to existing controller methods like Content, View, JSON...
    /// </summary>
    public static class ExcelControllerExtensions
    {
        
        /// <summary>
        /// Generate Excel spreadsheet for download as attachment.
        /// Spreadsheet will contain a single worksheet with a column
        /// for each property of the type T and a row for each of the
        /// instances in the IEnumerable<T>.  Will generate a header row
        /// with the names of each of the properties as the column headers.
        /// For example in your controller you might have:
        ///     var rows = from myObj in myCollection select new { Name=myObj.name, ID=myObj.id };
        ///     return this.Excel(rows, "ExcelTest.xlsx");
        ///  This will create a spreadsheet with two columns (Name and ID) and one row
        ///  containing the name and id of each object in myCollection.
        /// </summary>
        /// <typeparam name="T">Type of object to export, each row reprsents on</typeparam>
        /// <param name="controller">controller reference for extension method</param>
        /// <param name="rows">collection of objects, each object represents a row, each property a column</param>
        /// <param name="excelFileName">default file name for attachment</param>
        /// <returns></returns>
        public static ActionResult Excel<T>(this Controller controller,
            IEnumerable<T> rows, string excelFileName  )
        {
            MemoryStream stream = new MemoryStream();
            ExcelWriter.WriteSpreadsheet(rows, stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = excelFileName };
        }
    }
  
}