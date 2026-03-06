using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Clientes;

internal sealed class CustomerRepository
{
    public bool DeleteOrDeactivate(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(1) FROM Factura WHERE ClienteId = @id;";
        checkCommand.Parameters.AddWithValue("@id", id);
        var hasInvoices = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

        using var command = connection.CreateCommand();
        if (hasInvoices)
        {
            command.CommandText = "UPDATE Cliente SET Activo = 0 WHERE Id = @id;";
        }
        else
        {
            command.CommandText = "DELETE FROM Cliente WHERE Id = @id;";
        }

        command.Parameters.AddWithValue("@id", id);
        return command.ExecuteNonQuery() > 0;
    }

    public List<CustomerGridRowDto> GetGridRows(string? search = null)
    {
        var rows = new List<CustomerGridRowDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var sql = @"SELECT c.Id, c.Nombre, IFNULL(c.Direccion, ''), c.Nit, IFNULL(c.Email, ''), IFNULL(c.Telefono, ''),
       IFNULL(SUM(CASE WHEN f.Estado <> 'Anulada' THEN f.Saldo ELSE 0 END), 0) AS Balance,
       IFNULL(COUNT(CASE WHEN f.Estado <> 'Anulada' THEN f.Id END), 0) AS NumeroFacturas
FROM Cliente c
LEFT JOIN Factura f ON f.ClienteId = c.Id
WHERE c.Activo = 1";

        if (!string.IsNullOrEmpty(search?.Trim()))
        {
            sql += @" AND (c.Nombre LIKE @filter OR c.Nit LIKE @filter OR c.Codigo LIKE @filter OR c.Email LIKE @filter)";
            command.Parameters.AddWithValue("@filter", $"%{search.Trim()}%");
        }

        sql += " GROUP BY c.Id, c.Nombre, c.Direccion, c.Nit, c.Email, c.Telefono ORDER BY c.Nombre;";
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new CustomerGridRowDto
            {
                Id = reader.GetInt64(0),
                Cliente = reader.GetString(1),
                NitCedula = reader.GetString(3),
                Contacto = reader.GetString(4),
                Balance = Convert.ToDecimal(reader.GetDouble(6)),
                NumeroFacturas = Convert.ToInt32(reader.GetInt64(7))
            });
        }

        return rows;
    }

    public List<CustomerDto> GetAll(string? search)
    {
        var customers = new List<CustomerDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, Codigo, Nombre, Nit, IFNULL(Direccion, ''), IFNULL(Telefono, ''), IFNULL(Email, ''), TipoIva, Activo
FROM Cliente
WHERE (@search IS NULL OR trim(@search) = '' OR Nombre LIKE @filter OR Nit LIKE @filter OR Codigo LIKE @filter)
ORDER BY Nombre;";

        command.Parameters.AddWithValue("@search", (object?)search ?? DBNull.Value);
        command.Parameters.AddWithValue("@filter", $"%{search?.Trim()}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            customers.Add(new CustomerDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                Nit = reader.GetString(3),
                Direccion = reader.GetString(4),
                Telefono = reader.GetString(5),
                Email = reader.GetString(6),
                TipoIva = reader.GetString(7),
                Activo = reader.GetInt64(8) == 1
            });
        }

        return customers;
    }

    public string GenerateNextCode()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT IFNULL(MAX(Id), 0) + 1 FROM Cliente;";

        var nextValue = Convert.ToInt32(command.ExecuteScalar());
        return $"CLI-{nextValue:0000}";
    }

    public bool ExistsDuplicate(string codigo, string nit, long? excludeId)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(1)
FROM Cliente
WHERE (Codigo = @codigo OR Nit = @nit)
  AND (@excludeId IS NULL OR Id <> @excludeId);";

        command.Parameters.AddWithValue("@codigo", codigo.Trim());
        command.Parameters.AddWithValue("@nit", nit.Trim());
        command.Parameters.AddWithValue("@excludeId", excludeId.HasValue ? excludeId.Value : DBNull.Value);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public void Insert(CustomerDto customer)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Cliente(Codigo, Nombre, Nit, Direccion, Telefono, Email, TipoIva, Activo)
VALUES (@codigo, @nombre, @nit, @direccion, @telefono, @email, @tipoIva, @activo);";

        Bind(command, customer);
        command.ExecuteNonQuery();
    }

    public CustomerDto? GetById(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, Codigo, Nombre, Nit, IFNULL(Direccion, ''), IFNULL(Telefono, ''), IFNULL(Email, ''), TipoIva, Activo
FROM Cliente
WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new CustomerDto
        {
            Id = reader.GetInt64(0),
            Codigo = reader.GetString(1),
            Nombre = reader.GetString(2),
            Nit = reader.GetString(3),
            Direccion = reader.GetString(4),
            Telefono = reader.GetString(5),
            Email = reader.GetString(6),
            TipoIva = reader.GetString(7),
            Activo = reader.GetInt64(8) == 1
        };
    }

    public void Update(CustomerDto customer)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Cliente
SET Codigo = @codigo,
    Nombre = @nombre,
    Nit = @nit,
    Direccion = @direccion,
    Telefono = @telefono,
    Email = @email,
    TipoIva = @tipoIva,
    Activo = @activo
WHERE Id = @id;";

        command.Parameters.AddWithValue("@id", customer.Id);
        Bind(command, customer);
        command.ExecuteNonQuery();
    }

    private static void Bind(Microsoft.Data.Sqlite.SqliteCommand command, CustomerDto customer)
    {
        command.Parameters.AddWithValue("@codigo", customer.Codigo.Trim());
        command.Parameters.AddWithValue("@nombre", customer.Nombre.Trim());
        command.Parameters.AddWithValue("@nit", customer.Nit.Trim());
        command.Parameters.AddWithValue("@direccion", customer.Direccion.Trim());
        command.Parameters.AddWithValue("@telefono", customer.Telefono.Trim());
        command.Parameters.AddWithValue("@email", customer.Email.Trim());
        command.Parameters.AddWithValue("@tipoIva", customer.TipoIva.Trim());
        command.Parameters.AddWithValue("@activo", customer.Activo ? 1 : 0);
    }
}
