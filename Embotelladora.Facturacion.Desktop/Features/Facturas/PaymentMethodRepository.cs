using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class PaymentMethodRepository
{
    public List<PaymentMethodDto> GetAll()
    {
        var result = new List<PaymentMethodDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT mp.Id,
       mp.Nombre,
       mp.Activo,
       (SELECT COUNT(*) FROM Factura f WHERE f.MetodoPagoId = mp.Id) AS UsoFacturas,
       (SELECT COUNT(*) FROM Pago p WHERE p.MetodoPagoId = mp.Id) AS UsoPagos
FROM MetodoPago mp
ORDER BY mp.Nombre;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new PaymentMethodDto
            {
                Id = reader.GetInt64(0),
                Nombre = reader.GetString(1),
                Activo = reader.GetInt32(2) == 1,
                UsoFacturas = reader.GetInt32(3),
                UsoPagos = reader.GetInt32(4)
            });
        }

        return result;
    }

    public bool ExistsByName(string nombre, long? excludeId = null)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM MetodoPago WHERE UPPER(Nombre) = UPPER(@nombre)";
        command.Parameters.AddWithValue("@nombre", nombre.Trim());

        if (excludeId.HasValue)
        {
            command.CommandText += " AND Id <> @id";
            command.Parameters.AddWithValue("@id", excludeId.Value);
        }

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public long Create(string nombre)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO MetodoPago(Nombre, Activo)
VALUES(@nombre, 1);
SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@nombre", nombre.Trim());

        return Convert.ToInt64(command.ExecuteScalar());
    }

    public void UpdateName(long id, string nombre)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE MetodoPago SET Nombre = @nombre WHERE Id = @id;";
        command.Parameters.AddWithValue("@nombre", nombre.Trim());
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    public void SetActive(long id, bool activo)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE MetodoPago SET Activo = @activo WHERE Id = @id;";
        command.Parameters.AddWithValue("@activo", activo ? 1 : 0);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    public void Delete(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM MetodoPago WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    public bool HasUsage(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    (SELECT COUNT(*) FROM Factura WHERE MetodoPagoId = @id)
  + (SELECT COUNT(*) FROM Pago WHERE MetodoPagoId = @id);";
        command.Parameters.AddWithValue("@id", id);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
