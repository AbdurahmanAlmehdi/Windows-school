using System.ComponentModel;
using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

// Owns `invoices` and the child `invoice_lines` table.
public sealed class InvoiceRepository
{
    private readonly SqlDb _db;
    public InvoiceRepository(SqlDb db) { _db = db; }

    private const string SelectInvoices = @"
        SELECT invoice_id, invoice_number, stay_id, guest_id, room_id,
               invoice_date, payment_status, payment_method, payment_date
        FROM   dbo.invoices
        ORDER  BY invoice_date;";

    private const string SelectLines = @"
        SELECT line_id, invoice_id, description, quantity, unit_price, category
        FROM   dbo.invoice_lines;";

    private const string DeleteLines    = @"DELETE FROM dbo.invoice_lines;";
    private const string DeleteInvoices = @"DELETE FROM dbo.invoices;";

    private const string InsertInvoice = @"
        INSERT INTO dbo.invoices
            (invoice_id, invoice_number, stay_id, guest_id, room_id,
             invoice_date, payment_status, payment_method, payment_date)
        VALUES
            (@id, @number, @stay, @guest, @room,
             @date, @status, @method, @paid);";

    private const string UpsertInvoice = @"
        IF EXISTS (SELECT 1 FROM dbo.invoices WHERE invoice_id = @id)
            UPDATE dbo.invoices
               SET invoice_number = @number, stay_id = @stay, guest_id = @guest, room_id = @room,
                   invoice_date = @date, payment_status = @status,
                   payment_method = @method, payment_date = @paid
             WHERE invoice_id = @id;
        ELSE
            INSERT INTO dbo.invoices
                (invoice_id, invoice_number, stay_id, guest_id, room_id,
                 invoice_date, payment_status, payment_method, payment_date)
            VALUES
                (@id, @number, @stay, @guest, @room,
                 @date, @status, @method, @paid);";

    private const string DeleteLinesByInvoice = @"DELETE FROM dbo.invoice_lines WHERE invoice_id = @id;";
    private const string DeleteInvoice        = @"DELETE FROM dbo.invoices WHERE invoice_id = @id;";

    private const string InsertLine = @"
        INSERT INTO dbo.invoice_lines
            (line_id, invoice_id, description, quantity, unit_price, category)
        VALUES
            (@id, @invoice, @description, @qty, @price, @category);";

    public List<Invoice> GetAll(
        IReadOnlyDictionary<Guid, Stay>  staysById,
        IReadOnlyDictionary<Guid, Guest> guestsById,
        IReadOnlyDictionary<Guid, Room>  roomsById)
    {
        using var c = _db.Open();

        var invoices = new List<Invoice>();
        var byId = new Dictionary<Guid, Invoice>();
        int maxNumber = 1000;

        using (var cmd = new SqlCommand(SelectInvoices, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var stayId  = r.GetGuid(2);
                var guestId = r.GetGuid(3);
                var roomId  = r.GetGuid(4);

                if (!staysById.TryGetValue(stayId, out var stay))
                    throw new InvalidDataException($"Invoice references unknown stay {stayId}.");
                if (!guestsById.TryGetValue(guestId, out var guest))
                    throw new InvalidDataException($"Invoice references unknown guest {guestId}.");
                if (!roomsById.TryGetValue(roomId, out var room))
                    throw new InvalidDataException($"Invoice references unknown room {roomId}.");

                var number = r.GetString(1);
                var inv = new Invoice(number)
                {
                    Id            = r.GetGuid(0),
                    Stay          = stay,
                    Guest         = guest,
                    Room          = room,
                    InvoiceDate   = r.GetDateTime(5),
                    PaymentStatus = Enum.Parse<PaymentStatus>(r.GetString(6)),
                    PaymentMethod = r.IsDBNull(7) ? null : Enum.Parse<PaymentMethod>(r.GetString(7)),
                    PaymentDate   = r.IsDBNull(8) ? null : r.GetDateTime(8),
                    Lines         = new BindingList<InvoiceLine>()
                };
                invoices.Add(inv);
                byId[inv.Id] = inv;

                // Track the highest INV-#### we've seen so the static counter
                // continues from the right place after load.
                if (number.StartsWith("INV-", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(number.Substring(4), out var n)
                    && n > maxNumber)
                    maxNumber = n;
            }
        }

        using (var cmd = new SqlCommand(SelectLines, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var invoiceId = r.GetGuid(1);
                if (!byId.TryGetValue(invoiceId, out var inv)) continue;

                inv.Lines.Add(new InvoiceLine
                {
                    Id          = r.GetGuid(0),
                    Description = r.GetString(2),
                    Quantity    = r.GetInt32(3),
                    UnitPrice   = r.GetDecimal(4),
                    Category    = Enum.Parse<InvoiceLineCategory>(r.GetString(5))
                });
            }
        }

        Invoice.AdvanceSequenceTo(maxNumber + 1);
        return invoices;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        using (var cmd = new SqlCommand(DeleteLines, c, tx))    cmd.ExecuteNonQuery();
        using (var cmd = new SqlCommand(DeleteInvoices, c, tx)) cmd.ExecuteNonQuery();
    }

    public void Insert(Invoice invoice, SqlConnection c, SqlTransaction tx)
    {
        WriteParent(InsertInvoice, invoice, c, tx);
        InsertChildren(invoice, c, tx);
    }

    public void Upsert(Invoice invoice, SqlConnection c, SqlTransaction tx)
    {
        WriteParent(UpsertInvoice, invoice, c, tx);
        using (var cmd = new SqlCommand(DeleteLinesByInvoice, c, tx))
        {
            cmd.Parameters.AddWithValue("@id", invoice.Id);
            cmd.ExecuteNonQuery();
        }
        InsertChildren(invoice, c, tx);
    }

    public void Delete(Invoice invoice, SqlConnection c, SqlTransaction tx)
    {
        // invoice_lines cascades on invoice delete.
        using var cmd = new SqlCommand(DeleteInvoice, c, tx);
        cmd.Parameters.AddWithValue("@id", invoice.Id);
        cmd.ExecuteNonQuery();
    }

    private static void WriteParent(string sql, Invoice invoice, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(sql, c, tx);
        cmd.Parameters.AddWithValue("@id",     invoice.Id);
        cmd.Parameters.AddWithValue("@number", invoice.InvoiceNumber);
        cmd.Parameters.AddWithValue("@stay",   invoice.Stay.Id);
        cmd.Parameters.AddWithValue("@guest",  invoice.Guest.Id);
        cmd.Parameters.AddWithValue("@room",   invoice.Room.Id);
        cmd.Parameters.AddWithValue("@date",   invoice.InvoiceDate);
        cmd.Parameters.AddWithValue("@status", invoice.PaymentStatus.ToString());
        cmd.Parameters.AddWithValue("@method",
            invoice.PaymentMethod.HasValue ? (object)invoice.PaymentMethod.Value.ToString() : DBNull.Value);
        cmd.Parameters.AddWithValue("@paid",
            invoice.PaymentDate.HasValue ? (object)invoice.PaymentDate.Value : DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    private static void InsertChildren(Invoice invoice, SqlConnection c, SqlTransaction tx)
    {
        foreach (var line in invoice.Lines)
        {
            using var cmd = new SqlCommand(InsertLine, c, tx);
            cmd.Parameters.AddWithValue("@id",          line.Id);
            cmd.Parameters.AddWithValue("@invoice",     invoice.Id);
            cmd.Parameters.AddWithValue("@description", line.Description);
            cmd.Parameters.AddWithValue("@qty",         line.Quantity);
            cmd.Parameters.AddWithValue("@price",       line.UnitPrice);
            cmd.Parameters.AddWithValue("@category",    line.Category.ToString());
            cmd.ExecuteNonQuery();
        }
    }
}
