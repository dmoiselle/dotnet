using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Validation;
using System.IO;

namespace Bridge.Web.Utility
{
    /// <summary>
    /// Class for writing Excel 2007/2010 (.xlsx) compatible spreadsheets.
    /// </summary>
    public class ExcelWriter
    {

        /// <summary>
        /// Creates a spreadsheet using a list of objects.  The spreadsheet has one column
        /// for each property of the object.  There is a header row with a name of each
        /// property of the object.  Each object is written as one row containing the values
        /// of the properties for that instance.
        /// </summary>
        /// <typeparam name="T">type of object to put in each row</typeparam>
        /// <param name="rows">collection of objects, each object represents a row, each property a column</param>
        /// <param name="stream">stream to write file to</param>
        static public void WriteSpreadsheet<T>(IEnumerable<T> rows, Stream stream)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                CreateWorkbook(spreadSheet);
                CreateStylesheet(spreadSheet);
                SheetData sheetData = CreateWorksheet(spreadSheet);

                if (rows.Count() > 0)
                {
                    // Create header row
                    Row headerRow = new Row();
                    sheetData.AppendChild(headerRow);

                    // write the names of the object properties in the header row
                    T firstRowData = rows.First();
                    var props = firstRowData.GetType().GetProperties();
                    int rowNum = 1;
                    int colNum = 1;
                    foreach (var prop in props)
                    {
                        headerRow.AppendChild(CreateCell(prop.Name, prop.Name.GetType(), rowNum, colNum++));
                    }
                    ++rowNum;

                    // write out the values for each row
                    foreach (T rowData in rows)
                    {
                        Row row = new Row();
                        sheetData.AppendChild(row);
                        colNum = 1;
                        foreach (var prop in props)
                        {
                            object val = prop.GetValue(rowData, null);
                            row.AppendChild(CreateCell(val, prop.PropertyType, rowNum, colNum++));
                        }
                        ++rowNum;
                    }
                }

                // save worksheet
                spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.Save();

                LinkWorksheetToWorkbook(spreadSheet);

                spreadSheet.WorkbookPart.Workbook.Save();
                spreadSheet.Close();
            }
        }

        private static void LinkWorksheetToWorkbook(SpreadsheetDocument spreadSheet)
        {
            // create the worksheet to workbook relation
            spreadSheet.WorkbookPart.Workbook.AppendChild(new Sheets());
            spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
            {
                Id = spreadSheet.WorkbookPart.GetIdOfPart(spreadSheet.WorkbookPart.WorksheetParts.First()),
                SheetId = 1,
                Name = "Sheet1"
            });
        }

        private static SheetData CreateWorksheet(SpreadsheetDocument spreadSheet)
        {
            // create worksheet
            spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
            spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet();

            // create sheet data
            SheetData sheetData = new SheetData();
            spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.AppendChild(sheetData);
            return sheetData;
        }

        private static void CreateWorkbook(SpreadsheetDocument spreadSheet)
        {
            spreadSheet.AddWorkbookPart();
            spreadSheet.WorkbookPart.Workbook = new Workbook();     // create the worksheet
        }

        static private Stylesheet CreateStylesheet(SpreadsheetDocument spreadSheet)
        {

            Stylesheet stylesheet = new Stylesheet();
            WorkbookStylesPart workbookStylesPart = spreadSheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            workbookStylesPart.Stylesheet = stylesheet;
            Fonts fonts = new Fonts() { Count = (UInt32Value)1U, KnownFonts = true };

            Font font = new Font();
            FontSize fontSize = new FontSize() { Val = 11D };
            Color color = new Color() { Theme = (UInt32Value)1U };
            FontName fontName = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme = new FontScheme() { Val = FontSchemeValues.Minor };

            font.Append(fontSize);
            font.Append(color);
            font.Append(fontName);
            font.Append(fontFamilyNumbering);
            font.Append(fontScheme);

            fonts.Append(font);

            Fills fills = new Fills() { Count = (UInt32Value)2U };

            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill() { PatternType = PatternValues.None };

            fill1.Append(patternFill1);

            Fill fill2 = new Fill();
            PatternFill patternFill2 = new PatternFill() { PatternType = PatternValues.Gray125 };

            fill2.Append(patternFill2);

            fills.Append(fill1);
            fills.Append(fill2);

            Borders borders = new Borders() { Count = (UInt32Value)1U };

            Border border = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border.Append(leftBorder1);
            border.Append(rightBorder1);
            border.Append(topBorder1);
            border.Append(bottomBorder1);
            border.Append(diagonalBorder1);

            borders.Append(border);

            CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = (UInt32Value)1U };
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U };

            cellStyleFormats1.Append(cellFormat1);

            CellFormats cellFormats1 = new CellFormats() { Count = (UInt32Value)2U };
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };
            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };

            cellFormats1.Append(cellFormat2);
            cellFormats1.Append(cellFormat3);

            CellStyles cellStyles1 = new CellStyles() { Count = (UInt32Value)1U };
            CellStyle cellStyle1 = new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U };

            cellStyles1.Append(cellStyle1);
            DifferentialFormats differentialFormats1 = new DifferentialFormats() { Count = (UInt32Value)0U };
            TableStyles tableStyles1 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleLight16" };

            stylesheet.Append(fonts);
            stylesheet.Append(fills);
            stylesheet.Append(borders);
            stylesheet.Append(cellStyleFormats1);
            stylesheet.Append(cellFormats1);
            stylesheet.Append(cellStyles1);
            stylesheet.Append(differentialFormats1);
            stylesheet.Append(tableStyles1);

            return stylesheet;
        }

        static private Cell CreateCell(object val, Type valType, int rowNum, int colNum)
        {
            Cell cell = new Cell();
            CellValues cellType = GetExcelType(valType);
            string valAsString = (val == null) ? "" : val.ToString();
            cell.CellValue = new CellValue();
            if (cellType == CellValues.Date)
            {
                valAsString = ((DateTime)val).ToOADate().ToString();
                cell.StyleIndex = 1;
            }
            if (cellType == CellValues.String)
            {
                cell.DataType = CellValues.InlineString;
                InlineString inlineString = new InlineString();
                Text t = new Text();
                t.Text = valAsString;
                inlineString.AppendChild(t);
                cell.AppendChild(inlineString);
            }
            else if (cellType == CellValues.Boolean)
            {
                cell.DataType = CellValues.Boolean;
                cell.CellValue.Text = ((bool) val) ? "1" : "0";
            }
            else
            {
                // number and date have a null type per the spec
                cell.CellValue.Text = valAsString;
            }
            cell.CellReference = GetColumnName(colNum) + rowNum;

            return cell;
        }

        private static string GetColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modifier;

            while (dividend > 0)
            {
                modifier = (dividend - 1) % 26;
                columnName =
                    Convert.ToChar(65 + modifier).ToString() + columnName;
                dividend = (int)((dividend - modifier) / 26);
            }

            return columnName;
        }

        static private CellValues GetExcelType(Type type)
        {
            if (type == null)
            {
                return CellValues.String;
            }

            if (type.BaseType != null && type.BaseType.Name == "Enum")
            {
                return CellValues.String;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return CellValues.Boolean;
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return CellValues.Number;
                case TypeCode.DateTime:
                    return CellValues.Date;
                case TypeCode.Char:
                case TypeCode.String:
                    return CellValues.String;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return GetExcelType(Nullable.GetUnderlyingType(type));
                    }
                    return CellValues.String;
            }
            return CellValues.String;

        }
    }
}