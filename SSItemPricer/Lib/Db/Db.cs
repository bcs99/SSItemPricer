using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SSItemPricer.Lib.Db;

public interface IConnectionString
{
    string Text { get; }
}

public class Gp : IConnectionString
{
    public string Text =>
        "Data Source=db.sparteksystems.net;" +
        "Initial Catalog=SCANP;" +
        "Persist Security Info=True;" +
        "Connect Timeout=10;" +
        "User ID=jbones;" +
        "Password=spartek";
}

public class Mis : IConnectionString
{
    public string Text =>
        "Server=10.0.0.25,1400;" +
        "Database=MIS;" +
        "Uid=web123;" +
        "Pwd=web123";
}

public static class Db
{
    public static DataTable Read<T>(string query) where T : IConnectionString
    {
        var connectionString = Activator.CreateInstance<T>();

        using var conn = new SqlConnection(connectionString.Text);
        using var cmd = new SqlCommand(query, conn);

        conn.Open();

        var table = new DataTable();

        table.Load(cmd.ExecuteReader());

        return table;
    }
}