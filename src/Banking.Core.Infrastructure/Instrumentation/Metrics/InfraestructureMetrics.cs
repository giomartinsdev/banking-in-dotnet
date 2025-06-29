using System.Diagnostics.Metrics;

namespace BankingProject.Infrastructure.Instrumentation.Metrics
{
    public static class InfrastructureMetrics
    {
        public const string MeterName = "Banking.Core.Infrastructure";
        internal static readonly Meter Meter = new(MeterName);

        public static readonly CreatedDocumentsIntoDatabaseMetric CreatedDocumentsIntoDatabaseMetric = new(Meter);
        
        static InfrastructureMetrics()
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) => Meter.Dispose();
        }
    }
}
