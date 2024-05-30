using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using SSItemPricer.Lib.Db;
using UglyToad.PdfPig;

namespace SSItemPricer.Models;

public class CatalogItems
{
    private const string DocumentsRoot = @"\\FILESERVER\Spartek\Documentation Department\_Documents";

    public readonly List<int> ItemNumbers = new();

    public CatalogItems()
    {
        var catalogNumbers = Db.Read<Mis>(
                """
                SELECT ItemID
                FROM tblDocumentItem DI
                         JOIN tblItem I ON I.ItemNumber = DI.ItemID
                         JOIN tblSubCat02 SC ON SC.CatID = I.Category2ID
                WHERE SC.CategoryName = 'Catalogs'
                """)
            .AsEnumerable()
            .Select(r => r["ItemID"].ToString() ?? string.Empty);

        foreach (var number in catalogNumbers)
        {
            var documentRoot = Path.Combine(DocumentsRoot, number);

            if (Directory.Exists(documentRoot) == false)
                continue;

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