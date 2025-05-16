using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace PassagePlanner
{
    // MyDocumentViewer is used in order to get Landscape orientation
    public class MyDocumentViewer : DocumentViewer
    {
        protected override void OnPrintCommand()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();

                // Let user select the pages to print (if he/she does not want to print ALL pages).
                printDialog.UserPageRangeEnabled = true;

                FixedDocument doc = this.Document as FixedDocument;

                // Set the default page orientation based on the desired output.
                printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;

                if (printDialog.ShowDialog() == true)
                {
                    if (printDialog.PageRangeSelection == PageRangeSelection.AllPages) //easy... just use the dialog
                    {
                        printDialog.PrintDocument(doc.DocumentPaginator, "My Document");
                    }
                    else //PageRangeSelection.UserPages... only do a range... trickier...
                    {
                        //make a copy of the fixed document (might be a better way to copy, but this works) ... 
                        // ... or else it gives an error when printing if viewing at same time (because you are assigning two parents)
                        string timeStamp = DateTime.Now.DayOfYear.ToString() + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
                        string pack = "pack://temp" + timeStamp + ".xps";
                        MemoryStream ms = new MemoryStream();
                        Package package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
                        PackageStore.AddPackage(new Uri(pack), package);
                        XpsDocument xpsDoc = new XpsDocument(package, CompressionOption.SuperFast, pack);
                        XpsDocumentWriter xpsDocumentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                        xpsDocumentWriter.Write(doc);
                        FixedDocumentSequence fdsCopy = xpsDoc.GetFixedDocumentSequence();

                        //write only the page visuals we want to print to the print queue
                        XpsDocumentWriter xdw = PrintQueue.CreateXpsDocumentWriter(printDialog.PrintQueue);
                        VisualsToXpsDocument vtxd = (VisualsToXpsDocument)xdw.CreateVisualsCollator();
                        for (int i = printDialog.PageRange.PageFrom - 1; i < printDialog.PageRange.PageTo; i++)
                        {
                            Visual v = fdsCopy.DocumentPaginator.GetPage(i).Visual;
                            ContainerVisual cv = new ContainerVisual();
                            cv.Children.Add(v);
                            vtxd.Write(cv, printDialog.PrintTicket);
                            cv.Children.Remove(v); //remove so that if they print again it will not try to set a second parent
                        }
                        vtxd.EndBatchWrite();

                        //clean up
                        xpsDoc.Close();
                        ms.Close();
                        ms.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        }
    }
}
