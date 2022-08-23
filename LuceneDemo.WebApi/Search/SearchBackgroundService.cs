namespace LuceneDemo.WebApi.Search
{
    internal class SearchBackgroundService : BackgroundService
    {
        private readonly ILogger<SearchBackgroundService> _logger;
        private readonly ICustomersSearchEngine _customerSearchEngine;

        private const int REPEAT_FREQ_IN_SECONDS = 10; // Sets the interval in seconds between repeated executions

        public SearchBackgroundService(ILogger<SearchBackgroundService> logger, ICustomersSearchEngine customerSearchEngine)
        {
            this._logger = logger;
            this._customerSearchEngine = customerSearchEngine;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogInformation("Starting {jobName} ...", nameof(SearchBackgroundService));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await this._customerSearchEngine.RebuildIndexAsync();
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "An error occured when trying to rebild the search index for customers in service {jobName}.", nameof(SearchBackgroundService));
                }

                // Repeat task every 10 seconds
                await Task.Delay(REPEAT_FREQ_IN_SECONDS * 1000, stoppingToken);
            }

            this._logger.LogInformation("Stopping {jobName} ...", nameof(SearchBackgroundService));
        }
    }
}
