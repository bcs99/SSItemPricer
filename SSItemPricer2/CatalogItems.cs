using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using UglyToad.PdfPig;

namespace SSItemPricer2;

public class CatalogItems
{
    private const string DocumentsRoot = @"\\FILESERVER\Spartek\Documentation Department\_Documents";

    private int[] _catalogNumbers = { 10022382, 10023590, 10023591, 10024159, 10025699, 10029471 };

    public List<int> ItemNumbers = new();

    public CatalogItems()
    {
        foreach (var number in _catalogNumbers)
        {
            var documentRoot = Path.Combine(DocumentsRoot, number.ToString());
            var files = Directory.GetFiles(documentRoot, "*.pdf");
            var path = files.Length switch
            {
                1 => files[0],
                0 => throw new Exception($"No PDF found for catalog.\n{documentRoot}"),
                _ => throw new Exception($"Multiple PDFs found for catalog.\n{documentRoot}")
            };

            ReadItemNumbers(path);
        }

        ItemNumbers.Sort();
    }

    private void ReadItemNumbers(string pdfPath)
    {
        using var pdf = PdfDocument.Open(pdfPath);

        foreach (var page in pdf.GetPages())
        {
            if (page.Text.StartsWith("Please wait..."))
                throw new Exception($"PDF cannot be scanned.\n{pdfPath}");

            foreach (Match match in Regex.Matches(page.Text, @"100\d{5}"))
            {
                var matchValue = int.Parse(match.Value);

                if (!ItemNumbers.Contains(matchValue))
                    ItemNumbers.Add(matchValue);
            }
        }
    }
}