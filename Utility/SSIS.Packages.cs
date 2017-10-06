using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SqlServer.Dts.Runtime;

namespace Bridge.Web.Utility
{
    public class SSIS
    {

        string pkgLocation;

        Package pkg;
        Application app;
        DTSExecResult pkgResults;
        public bool ExecPackage(string Path)
        {
            //pkgLocation = @"C:\Users\Peter\Documents\BIA\SOURCE\Dev\BRIDGE\Training\dotnet\Bridge.Data.Migration\Recruitment\CandidatesMigration.dtsx";

            //@"C:\Program Files\Microsoft SQL Server\100\Samples\Integration Services" +
            //@"\Package Samples\CalculatedColumns Sample\CalculatedColumns\CalculatedColumns.dtsx";

            pkgLocation = Path;
            app = new Application();
            pkg = app.LoadPackage(pkgLocation, null);
            pkgResults = pkg.Execute();

            //Console.WriteLine(pkgResults.ToString());
            //Console.ReadKey();

            return pkgResults.ToString() == "Success" ? true : false;
        }
    }
}