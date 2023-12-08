WITH CTE(RootId, ItemQuantity, ItemID, UseBOM, Price, Labor, Level)
         AS
         (SELECT I.ItemNumber,
                 CAST(1 AS DECIMAL(10, 3)),
                 I.ItemNumber,
                 IV.UseBOM,
                 CAST(IIF(IV.UseBOM = 0, IV.BuyUnitPrice, 0) AS MONEY),
                 CAST(0 AS MONEY),
                 0
          FROM dbo.tblItem I
                   FULL JOIN dbo.tblItemVendor IV ON IV.ItemID = I.ItemNumber AND VendorPriority = 1
                   JOIN dbo.tblECOStatus S ON S.ECOStatusID = I.ECOStatusID
          UNION ALL
          SELECT CTE.RootId,
                 CAST(CTE.ItemQuantity * BI.ItemQuantity AS DECIMAL(10, 3)),
                 BI.ItemID,
                 IV.UseBOM,
                 CAST(CTE.ItemQuantity * BI.ItemQuantity * IIF(IV.UseBOM = 0, IV.BuyUnitPrice, 0) AS MONEY),
                 CAST(BI.ItemQuantity * IIF(CTE.Level = 0 AND BI.ItemId = 10030791, IV.BuyUnitPrice, 0) AS MONEY),
                 CTE.Level + 1
          FROM dbo.tblBOMItems BI
                   JOIN dbo.tblItemVendor IV ON IV.ItemID = BI.ItemID AND IV.VendorPriority = 1
                   JOIN CTE ON CTE.ItemID = BI.BOMID
          WHERE NOT EXISTS(SELECT * FROM dbo.tblBOMItems BI WHERE BI.BOMID = CTE.ItemID AND BI.ItemID = CTE.ItemID)
            AND CTE.UseBOM = 1
            AND BI.ItemQuantity != 0
            AND CTE.Level < 6)
SELECT I.ItemNumber                          AS [Item Number],
       ISNULL(I.ItemMarketingName, ItemName) AS [Item Description],
       ISNULL(S.Price - S.Labor, 0)          AS Parts,
       ISNULL(S.Labor, 0)                    AS Labor,
       ISNULL(S.Price, 0)                    AS Price,
       ISNULL(IV.SetupCost, 0)               AS [Setup Cost],
       ISNULL(IV.PieceCost, 0)               AS [Piece Cost],
       ISNULL(IV.UseBOM, 0)                  AS [Is BOM],
       ISNULL(I.Discontinued, 0)             AS Discontinued,
       ISNULL(ES.Description, '-')           AS Status,
       ISNULL(V.VendorName, '-')             AS [Vendor Name],
       ISNULL(IV.VendorItemNumber, '-')      AS [Vendor Item],
       ISNULL(SC.CategoryName, '-')          AS Category
FROM dbo.tblItem I
         FULL JOIN dbo.tblItemVendor IV on I.ItemNumber = IV.ItemID AND IV.VendorPriority = 1
         FULL JOIN dbo.tblVendor V ON IV.VendorID = V.VendorID
         JOIN dbo.tblECOStatus ES on I.ECOStatusID = ES.ECOStatusID
         FULL JOIN dbo.tblSubCat01 SC on I.Category1ID = SC.CatID
         JOIN (SELECT RootId,
                      SUM(Labor) AS Labor,
                      SUM(Price) AS Price
               FROM CTE
               GROUP BY RootId) AS S ON RootId = I.ItemNumber
ORDER BY I.ItemNumber
