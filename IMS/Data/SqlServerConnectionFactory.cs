using IMS.Configuration;
using Microsoft.Data.SqlClient;
using AppConfig = IMS.Configuration.ApplicationConfiguration;

namespace IMS.Data;

public sealed class SqlServerConnectionFactory(AppConfig configuration)
{
    private readonly AppConfig _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public SqlConnection CreateConnection(bool connectToMaster = false)
    {
        var database = _configuration.Database;
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = database.Server,
            InitialCatalog = connectToMaster ? "master" : database.DatabaseName,
            IntegratedSecurity = database.UseIntegratedSecurity,
            Encrypt = true,
            TrustServerCertificate = IsLocalServer(database.Server),
            ConnectTimeout = 15,
            ApplicationName = "Archive Auditor"
        };

        if (!database.UseIntegratedSecurity)
        {
            builder.UserID = database.UserName;
            builder.Password = database.Password;
        }

        return new SqlConnection(builder.ConnectionString);
    }

    private static bool IsLocalServer(string server)
    {
        var host = server.Trim();
        if (host.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)) return true;
        host = host.Split('\\', ',')[0].Trim().Trim('[', ']');
        return host is "." or "(local)" or "localhost" or "127.0.0.1" or "::1" 
            || host.StartsWith("192.168.", StringComparison.Ordinal) 
            || string.Equals(host, Environment.MachineName, StringComparison.OrdinalIgnoreCase);

    }
}
