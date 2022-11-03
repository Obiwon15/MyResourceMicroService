using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resourceedge.Appraisal.API.Interfaces;

namespace Resourceedge.Appraisal.API.Services.CronJobServices
{
    public class DeactivateAppraisalService : IHostedService, IDisposable
    {
        private readonly IAppraisalConfig _appraisalConfig;
        private readonly ILogger<ActivateAppraisalService> _logger;
        private Timer _timer;

        public DeactivateAppraisalService(IAppraisalConfig appraisalConfig, ILogger<ActivateAppraisalService> logger)
        {
            _appraisalConfig = appraisalConfig;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TimeSpan interval = TimeSpan.FromHours(24);

            //calculate time to run the first time & delay to set the timer
            var nextRunTime = DateTime.Today.AddDays(1).AddHours(0);
            var currentTime = DateTime.Now;
            var firstInterval = nextRunTime.Subtract(currentTime);

            void DeActivateAppraisal()
            {
                var t1 = Task.Delay(firstInterval, cancellationToken);
                t1.Wait(cancellationToken);

                _appraisalConfig.DeActivateAppraisals(null);

                //now schedule it to be called every 24 hours for future
                _timer = new Timer(_appraisalConfig.DeActivateAppraisals, null, TimeSpan.Zero, interval);
            }

            // no need to await this call here because this task is scheduled to run much much later.
            Task.Run(DeActivateAppraisal, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
