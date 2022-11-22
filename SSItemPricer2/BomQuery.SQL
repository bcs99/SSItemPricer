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
                   JOIN dbo.tblItemVendor IV ON IV.ItemID = I.ItemNumber AND VendorPriority = 1
                   JOIN dbo.tblECOStatus S ON S.ECOStatusID = I.ECOStatusID
          UNION ALL
          SELECT CTE.RootId,
                 CAST(CTE.ItemQuantity * BI.ItemQuantity AS DECIMAL(10, 3)),
                 BI.ItemID,
                 IV.UseBOM,
                 CAST(CTE.ItemQuantity * BI.ItemQuantity *
                      IIF(IV.UseBOM = 0, IV.BuyUnitPrice, 0) AS MONEY),
                 CAST(BI.ItemQuantity *
                      IIF(CTE.Level = 0 AND BI.ItemId = 10030791, IV.BuyUnitPrice, 0) AS MONEY),
                 CTE.Level + 1
          FROM dbo.tblBOMItems BI
                   JOIN dbo.tblItemVendor IV ON IV.ItemID = BI.ItemID AND VendorPriority = 1
                   JOIN CTE ON CTE.ItemID = BI.BOMID
          WHERE CTE.UseBOM = 1
            AND BI.ItemQuantity != 0
            AND CTE.Level < 6)
SELECT I.ItemNumber              AS [Item Number],
       I.ItemName                AS [Item Description],
       BI.ItemQuantity           AS Quantity,
       S.Price - S.Labor         AS Parts,
       S.Labor                   AS Labor,
       S.Price * BI.ItemQuantity AS Price,
       IV.SetupCost              AS [Setup Cost],
       IV.PieceCost              AS [Piece Cost],
       IV.UseBOM                 AS [Is BOM],
       I.Discontinued            AS Discontinued,
       ES.Description            AS Status,
       V.VendorName              AS [Vendor Name]
FROM dbo.tblBOMItems BI
         JOIN dbo.tblItem I ON I.ItemNumber = BI.ItemID
         JOIN dbo.tblItemVendor IV on BI.ItemID = IV.ItemID
         JOIN dbo.tblVendor V ON IV.VendorID = V.VendorID
         JOIN dbo.tblECOStatus ES on I.ECOStatusID = ES.ECOStatusID
         JOIN (SELECT RootId,
                      SUM(Labor) AS Labor,
                      SUM(Price) AS Price
               FROM CTE
               GROUP BY RootId) AS S ON RootId = I.ItemNumber
WHERE IV.VendorPriority = 1
  AND BI.BOMID = ?
ORDER BY I.ItemNumber