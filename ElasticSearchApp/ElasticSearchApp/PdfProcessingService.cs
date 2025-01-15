using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Kernel.Pdf.Canvas.Parser;
using System;
using System.IO;
using System.Text;

namespace ElasticSearchApp
{
    public class PdfProcessingService
    {
        // Extracts text from a PDF file using iTextSharp

        
        public string ExtractTextFromPdf(string pdfPath)
        {
            StringBuilder text = new StringBuilder();

            try
            {
                // Open the PDF document
                using (PdfReader reader = new PdfReader(pdfPath))
                {
                    PdfDocument pdfDoc = new PdfDocument(reader);
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        // Extract text from each page
                        var page = pdfDoc.GetPage(i);
                        var pageText = PdfTextExtractor.GetTextFromPage(page);
                        text.Append(pageText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from {pdfPath}: {ex.Message}");
            }

            return text.ToString();
        }
    }

}
