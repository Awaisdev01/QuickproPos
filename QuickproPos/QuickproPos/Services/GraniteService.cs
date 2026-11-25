using QuickproPos.Data.Account;
using QuickproPos.Data.InventoryModel;
using QuickproPos.Data.Setting;
using QuickproPos.Data.SettingView;
using SQLite;

namespace QuickproPos.Services
{
    public class GraniteService
    {
        private readonly SQLiteAsyncConnection _database;

        public GraniteService(SQLiteAsyncConnection database)
        {
            _database = database;
        }
        // Method to check if a name exists in the database
        public async Task<bool> CheckNameAsync(string name)
        {
            // Query the database asynchronously
            var query = await _database.Table<GraniteQuotation>()
                                        .Where(progm => progm.QuotationNo == name)
                                        .CountAsync();
            return query > 0;
        }
        public async Task<List<ProductView>> GetAllByGraniteQuotationMasterDetails(int GraniteQuotationId)
        {
            var tilesDetailsList = await _database.Table<GraniteQuotationDetails>()
                                                      .Where(x => x.GraniteQuotationId == GraniteQuotationId)
                                                      .ToListAsync();

            var products = await _database.Table<Product>().ToListAsync();

            var productViewList = (from detail in tilesDetailsList
                                   join product in products on detail.ProductId equals product.ProductId into productGroup
                                   from product in productGroup.DefaultIfEmpty() // Handle cases where no matching product is found
                                   select new ProductView
                                   {
                                       GraniteQuotationDetailsId = detail.GraniteQuotationDetailsId,
                                       ProductId = detail.ProductId,
                                       ProductName = product?.ProductName ?? "Unknown", // Default to "Unknown" if product is null
                                       ProductCode = product?.ProductCode ?? "N/A",   // Default to "N/A" if product is null
                                       Qty = detail.Qty,
                                       UnitId = detail.UnitId,
                                       SalesRate = detail.Rate,
                                       Rate = detail.Rate,
                                       SizeLength = detail.SizeLength,
                                       SizeWidth = detail.SizeWidth,
                                       TotalSqFt = detail.TotalSqFt,
                                       MeasurementSrNo = detail.MeasurementSrNo,
                                       Amount = detail.Amount,
                                       Description = detail.Description
                                   }).ToList();

            return productViewList;
        }
        public async Task<List<ProductView>> GetAllByGraniteQuotationToSalesDetails(int GraniteQuotationId)
        {
            var tilesDetailsList = await _database.Table<GraniteQuotationDetails>()
                                                      .Where(x => x.GraniteQuotationId == GraniteQuotationId)
                                                      .ToListAsync();

            // Query the Unit table
            var units = await _database.Table<Unit>().ToListAsync();

            var products = await _database.Table<Product>().ToListAsync();

            var productViewList = (from detail in tilesDetailsList
                                   join unit in units on detail.UnitId equals unit.UnitId into unitGroup
                                   from unit in unitGroup.DefaultIfEmpty() // Handle cases where no matching unit is found
                                   join product in products on detail.ProductId equals product.ProductId into productGroup
                                   from product in productGroup.DefaultIfEmpty() // Handle cases where no matching product is found
                                   select new ProductView
                                   {
                                       GraniteQuotationDetailsId = detail.GraniteQuotationDetailsId,
                                       ProductId = detail.ProductId,
                                       ProductName = product?.ProductName ?? "Unknown", // Default to "Unknown" if product is null
                                       ProductCode = product?.ProductCode ?? "N/A",   // Default to "N/A" if product is null
                                       Qty = detail.Qty,
                                       UnitId = detail.UnitId,
                                       UnitName = unit?.UnitName ?? "Unknown", // Default to "Unknown" if unit is null
                                       SalesRate = detail.Rate,
                                       TotalSqFt = detail.TotalSqFt,
                                       TaxId = product.TaxId,
                                       Amount = detail.Amount
                                   }).ToList();

            return productViewList;
        }
        public async Task<List<ProductView>> GetAllByGraniteQuotationToSalesDetailsSum(int GraniteQuotationId)
        {
            var tilesDetailsList = await _database.Table<GraniteQuotationDetails>()
                                                  .Where(x => x.GraniteQuotationId == GraniteQuotationId)
                                                  .ToListAsync();

            var units = await _database.Table<Unit>().ToListAsync();
            var products = await _database.Table<Product>().ToListAsync();

            var productViewList = (from detail in tilesDetailsList
                                   join unit in units on detail.UnitId equals unit.UnitId into unitGroup
                                   from unit in unitGroup.DefaultIfEmpty() // Handle cases where no matching unit is found
                                   join product in products on detail.ProductId equals product.ProductId into productGroup
                                   from product in productGroup.DefaultIfEmpty() // Handle cases where no matching product is found
                                   select new ProductView
                                   {
                                       GraniteQuotationDetailsId = detail.GraniteQuotationDetailsId,
                                       ProductId = detail.ProductId,
                                       ProductName = product?.ProductName ?? "Unknown", // Default to "Unknown" if product is null
                                       ProductCode = product?.ProductCode ?? "N/A",   // Default to "N/A" if product is null
                                       Qty = detail.TotalSqFt, // Using TotalSqFt here
                                       UnitId = detail.UnitId,
                                       UnitName = unit?.UnitName ?? "Unknown", // Default to "Unknown" if unit is null
                                       TaxId = product?.TaxId ?? 0 // Default TaxId if product is null
                                   }).ToList();

            // Group by ProductId and compute the sum of TotalSqFt
            var groupedProductViewList = productViewList
                .GroupBy(pv => pv.ProductId)
                .Select(group => new ProductView
                {
                    ProductId = group.Key,
                    ProductName = group.First().ProductName,
                    ProductCode = group.First().ProductCode,
                    UnitId = group.First().UnitId,
                    UnitName = group.First().UnitName,
                    TaxId = group.First().TaxId,
                    Qty = group.Sum(pv => pv.Qty) // Sum TotalSqFt
                })
                .ToList();

            return groupedProductViewList;
        }

        public async Task<List<GraniteQuotationMasterView>> GraniteQuotationSearchAsync(DateTime fromDate, DateTime toDate)
        {
            var quotationMasters = await _database
    .Table<GraniteQuotation>()
    .Where(p => p.Date >= fromDate && p.Date <= toDate)
    .ToListAsync();





            // Step 3: Load Related Data from InvoiceSetting and AccountLedger
            var invoiceSettings = await _database.Table<InvoiceSetting>().ToListAsync();
            var accountLedgers = await _database.Table<AccountLedger>().ToListAsync();

            // Step 4: Join Data Manually in Memory
            var result = (from pm in quotationMasters
                          join iset in invoiceSettings on pm.VoucherTypeId equals iset.VoucherTypeId
                          join al in accountLedgers on pm.LedgerId equals al.LedgerId
                          select new GraniteQuotationMasterView
                          {
                              GraniteQuotationId = pm.GraniteQuotationId,
                              QuotationNo = pm.QuotationNo,
                              Date = pm.Date,
                              TotalSqft = pm.TotalSqft,
                              TotalQty = pm.TotalQty,
                              DiscountAmount = pm.DiscountAmount,
                              TotalAmount = pm.TotalAmount,
                              AdvanceAmount = pm.AdvanceAmount,
                              GrandTotal = pm.GrandTotal,
                              Narration = pm.Narration,
                              VoucherTypeId = pm.VoucherTypeId,
                              VoucherTypeName = iset.VoucherTypeName,
                              LedgerName = al.LedgerName
                          }).ToList();

            return result;
        }
        public async Task<GraniteQuotation> GetByIdAsync(int id)
        {
            return await _database.FindAsync<GraniteQuotation>(id);
        }
        public async Task<int> SaveAsync(GraniteQuotation model)
        {

            try
            {
                await _database.InsertAsync(model);
                int id = model.GraniteQuotationId;

                if (id > 0)
                {
                    var ledgerPostings = new List<LedgerPosting>();
                    var stockPostings = new List<StockPosting>();
                    var GraniteQuotationDetails = new List<GraniteQuotationDetails>();

                    foreach (var item in model.listOrder)
                    {
                        var details = new GraniteQuotationDetails
                        {
                            GraniteQuotationId = model.GraniteQuotationId,
                            ProductId = item.ProductId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            SizeLength = item.SizeLength,
                            SizeWidth = item.SizeWidth,
                            MeasurementSrNo = item.MeasurementSrNo,
                            TotalSqFt = item.TotalSqFt,
                            Rate = item.Rate,
                            Amount = item.Amount,
                            Description = item.Description
                        };

                        GraniteQuotationDetails.Add(details); // Add to list for bulk insert
                    }

                    // Bulk insert SalesDetails and StockPostings
                    await _database.InsertAllAsync(GraniteQuotationDetails);
                    return model.GraniteQuotationId;  // Return GraniteQuotationId
                }
                else
                {
                    throw new Exception("Failed to insert Granite Quotation. Please try again.");
                }
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                throw new Exception("An error occurred while saving the data. Please try again.", ex);
            }
        }
        public async Task<bool> UpdateAsync(GraniteQuotation model)
        {

            try
            {
                await _database.UpdateAsync(model);

                foreach (var item in model.listOrder)
                {
                    GraniteQuotationDetails details;
                    if (item.GraniteQuotationDetailsId == 0)
                    {
                        details = new GraniteQuotationDetails
                        {
                            GraniteQuotationId = model.GraniteQuotationId,
                            ProductId = item.ProductId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            SizeLength = item.SizeLength,
                            SizeWidth = item.SizeWidth,
                            MeasurementSrNo = item.MeasurementSrNo,
                            TotalSqFt = item.TotalSqFt,
                            Rate = item.Rate,
                            Amount = item.Amount,
                            Description = item.Description
                        };

                        await _database.InsertAsync(details);


                    }
                    else
                    {
                        details = new GraniteQuotationDetails
                        {
                            GraniteQuotationDetailsId = item.GraniteQuotationDetailsId,
                            GraniteQuotationId = model.GraniteQuotationId,
                            ProductId = item.ProductId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            SizeLength = item.SizeLength,
                            SizeWidth = item.SizeWidth,
                            MeasurementSrNo = item.MeasurementSrNo,
                            TotalSqFt = item.TotalSqFt,
                            Rate = item.Rate,
                            Amount = item.Amount,
                            Description = item.Description
                        };

                        await _database.UpdateAsync(details);
                    }
                }

                foreach (var deleteItem in model.listDelete)
                {
                    var purchaseDetail = await _database.FindAsync<GraniteQuotationDetails>(deleteItem.GraniteQuotationDetailsId);
                    if (purchaseDetail != null)
                    {
                        await _database.DeleteAsync(purchaseDetail); // Deletes the SalesDetails record
                    }
                }
                return true;  // Successfully updated
            }
            catch (Exception ex)
            {
                // Rollback the transaction if any error occurs
                throw new Exception("An error occurred while saving the data. Please try again.", ex);
            }
        }
        public async Task<bool> DeleteTilesQuotationAsync(GraniteQuotationMasterView model)
        {
            try
            {
                var SalesDetailsToDelete = await _database.Table<GraniteQuotationDetails>()
                                                             .Where(pd => pd.GraniteQuotationId == model.GraniteQuotationId)
                                                             .ToListAsync();

                if (SalesDetailsToDelete.Any())
                {
                    foreach (var detail in SalesDetailsToDelete)
                    {
                        await _database.DeleteAsync(detail);
                    }
                }

                var SalesMaster = await _database.Table<GraniteQuotation>()
                                                     .FirstOrDefaultAsync(pm => pm.GraniteQuotationId == model.GraniteQuotationId);

                if (SalesMaster != null)
                {
                    await _database.DeleteAsync(SalesMaster);
                }


                return true; // Successfully deleted
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the quotation. Please try again.", ex);
            }
        }
        public async Task<string> GetSerialNo()
        {
            // Prepare the query
            const string query = @"
        SELECT IFNULL(MAX(CAST(SerialNo AS INTEGER) + 1), 1) AS VoucherNo
        FROM GraniteQuotation";

            // Execute the query and get the result
            var result = await _database.ExecuteScalarAsync<string>(query);

            // Convert result to string and return
            return result;
        }
    }
}
