using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;


namespace ExcelLayer
{
    public class Reader
    {

        public object[][] GetExcelData(string fullPath)
        {
            Excel.Application excelapp = new Excel.Application();
            excelapp.Visible = false;

            Excel.Workbook excelappworkbook = excelapp.Workbooks.Open(
                fullPath,
                Type.Missing, Type.Missing, true, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);

            Excel.Worksheet excelworksheet = (Excel.Worksheet)excelappworkbook.Worksheets.get_Item(1);
            Excel.Range range = excelworksheet.UsedRange;
            object[,] worksheetValuesArray = range.get_Value(Type.Missing);

            int numberOfRows = range.Rows.Count;
            int numberOfColumns = range.Columns.Count;
            object[][] objectTable = new object[numberOfRows][];


            for (int row = 1; row < (worksheetValuesArray.GetLength(0) + 1); row++)
            {
                objectTable[row - 1] = new object[numberOfColumns];
                for (int col = 1; col < (worksheetValuesArray.GetLength(1) + 1); col++)
                {
                    objectTable[row - 1][col - 1] = (object)worksheetValuesArray[row, col];
                }
            }
            excelappworkbook.Close(false, Type.Missing, Type.Missing);
            excelapp.Quit();

            return objectTable;
        }
    }
}
