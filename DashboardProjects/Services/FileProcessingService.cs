using Microsoft.Extensions.Logging;
using System.IO;

namespace DashboardProjects.Services
{
    public class FileProcessingService
    {
        private readonly DataInitializationService _dataInitializationService;
        private readonly ILogger<FileProcessingService> _logger;

        public FileProcessingService(DataInitializationService dataInitializationService, ILogger<FileProcessingService> logger)
        {
            _dataInitializationService = dataInitializationService;
            _logger = logger;
        }

        public async Task ProcessFileAsync(string filePath)
        {
            if (!Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid file type. Only .xlsx files are supported.");
            }

            try
            {
                _logger.LogInformation("Начало обработки файла: {filePath}", filePath);
                await _dataInitializationService.ReadAndSaveExcelData(filePath);
                _logger.LogInformation("Файл успешно обработан: {filePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке файла: {filePath}", filePath);
                throw;
            }
        }
    }
}
