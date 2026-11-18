using System.Data;
using System.Security.Cryptography;
using System.Text;
using IMS.Domain;
using IMS.Services;
using Microsoft.Data.SqlClient;

namespace IMS.Data;

/// <summary>Persists bounded batches so large archives are not retained in memory.</summary>
public sealed class SqlScanDataWriter(SqlServerConnectionFactory connectionFactory) : IScanDataWriter
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public Task WriteFilesAsync(long scanId, IReadOnlyCollection<FileSnapshotRecord> files, CancellationToken cancellationToken = default)
    {
        var table = new DataTable();
        table.Columns.Add("ScanId", typeof(long));
        table.Columns.Add("RelativePath", typeof(string));
        table.Columns.Add("RelativePathHash", typeof(byte[]));
        table.Columns.Add("FileName", typeof(string));
        table.Columns.Add("Extension", typeof(string));
        table.Columns.Add("FileSizeBytes", typeof(long));
        table.Columns.Add("CreatedAtUtc", typeof(DateTime));
        table.Columns.Add("ModifiedAtUtc", typeof(DateTime));
        table.Columns.Add("SHA256", typeof(byte[]));
        table.Columns.Add("FileAttributes", typeof(int));
        foreach (var file in files)
            table.Rows.Add(scanId, file.RelativePath, SHA256.HashData(Encoding.UTF8.GetBytes(file.RelativePath)),
                file.FileName, file.Extension, file.FileSizeBytes, file.CreatedAtUtc is null ? DBNull.Value : file.CreatedAtUtc.Value,
                file.ModifiedAtUtc is null ? DBNull.Value : file.ModifiedAtUtc.Value, file.Sha256, file.FileAttributes);
        return BulkCopyAsync(table, "dbo.Files", cancellationToken);
    }

    public Task WriteDirectoriesAsync(long scanId, IReadOnlyCollection<DirectorySnapshotRecord> directories, CancellationToken cancellationToken = default)
    {
        var table = new DataTable();
        table.Columns.Add("ScanId", typeof(long));
        table.Columns.Add("RelativePath", typeof(string));
        table.Columns.Add("RelativePathHash", typeof(byte[]));
        table.Columns.Add("FileCount", typeof(long));
        table.Columns.Add("DirectoryCount", typeof(long));
        table.Columns.Add("TotalSizeBytes", typeof(long));
        foreach (var directory in directories)
            table.Rows.Add(scanId, directory.RelativePath, SHA256.HashData(Encoding.UTF8.GetBytes(directory.RelativePath)),
                directory.FileCount, directory.DirectoryCount, directory.TotalSizeBytes);
        return BulkCopyAsync(table, "dbo.Directories", cancellationToken);
    }

    public async Task WriteErrorsAsync(long scanId, IReadOnlyCollection<ScanErrorRecord> errors, CancellationToken cancellationToken = default)
    {
        if (errors.Count == 0) return;
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        foreach (var error in errors)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT dbo.ScanErrors (ScanId, RelativePath, ErrorType, ErrorMessage, OccurredAtUtc) VALUES (@ScanId, @Path, @Type, @Message, @At);";
            command.Parameters.Add("@ScanId", SqlDbType.BigInt).Value = scanId;
            command.Parameters.Add("@Path", SqlDbType.NVarChar, 2000).Value = (object?)error.RelativePath ?? DBNull.Value;
            command.Parameters.Add("@Type", SqlDbType.NVarChar, 100).Value = error.ErrorType;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, -1).Value = error.ErrorMessage;
            command.Parameters.Add("@At", SqlDbType.DateTime2).Value = error.OccurredAtUtc;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task BulkCopyAsync(DataTable table, string destination, CancellationToken cancellationToken)
    {
        if (table.Rows.Count == 0) return;
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            { DestinationTableName = destination, BatchSize = 500, BulkCopyTimeout = 0 };
        foreach (DataColumn column in table.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        await bulkCopy.WriteToServerAsync(table, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
