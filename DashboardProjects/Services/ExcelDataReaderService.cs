using DataAccess.Models;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.IO;

namespace DashboardProjects.Services
{
    public class ExcelDataReaderService
    {
        private readonly ILogger<ExcelDataReaderService> _logger;

        public ExcelDataReaderService(ILogger<ExcelDataReaderService> logger)
        {
            _logger = logger;
        }

        public async Task<List<Transaction>> ReadExcelDataAsync(string filePath)
        {
            var data = new List<Transaction>();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (var row = 2; row <= rowCount; row++)
                {
                    var transaction = new Transaction
                    {
                        Date = worksheet.Cells[row, 1].GetValue<DateTime>(),
                        Type = worksheet.Cells[row, 2].GetValue<string>(),
                        Amount = worksheet.Cells[row, 3].GetValue<decimal>(),
                        Category = worksheet.Cells[row, 4].GetValue<string>()
                    };

                    data.Add(transaction);
                }

                _logger.LogInformation($"Данные из файла {filePath} успешно прочитаны.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при чтении данных из Excel файла.");
            }

            return await Task.FromResult(data);
        }
    }
}
