using System.Diagnostics.Metrics;

namespace BankingProject.Infrastructure.Instrumentation.Metrics
{
    public sealed class CreatedDocumentsIntoDatabaseMetric
    {
        internal const string Name = "infrastructure.documents.saved.count";
        private const string Description = "Quantity of documents created into the database.";
        private const string Unit = "{documents}";

        private readonly Counter<int> _createdDocumentsCounter;

        internal CreatedDocumentsIntoDatabaseMetric(Meter meter)
        {
            _createdDocumentsCounter = meter.CreateCounter<int>(Name, Unit, Description);
        }

        public void CreatedOneDocumentOf(string collectionName)
        {
            _createdDocumentsCounter.Add(
                1,
                new KeyValuePair<string, object?>("Document.CollectionName", collectionName)
            );
        }
    }
}
