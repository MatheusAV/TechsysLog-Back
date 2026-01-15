namespace TechsysLog.Infrastructure.Persistence.Mongo
{
    public sealed class MongoDbSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
    }
}
