using CustomersOpinions.Transactional.Data.CsvRecords;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;
using CustomersOpinions.Transactional.Data.Services.Exceptions;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.State;
using CustomersOpinions.Transactional.Data.Services.Validation;

namespace CustomersOpinions.Transactional.Data.Services.Loaders;

public sealed class CustomersLoader : FileLoader<CustomerCsv, Customer>
{
    private readonly IInsertDao<Customer> _dao;
    private readonly IReferences _references;

    public CustomersLoader(ICsvReader reader, IExistingRecordsDao existing, IInsertDao<Customer> dao, IReferences references)
        : base(reader, existing)
    {
        _dao = dao;
        _references = references;
    }

    public override string FileName => "clients.csv";
    protected override Table Table => Table.Customer;
    protected override string IdField => "IdCliente";
    protected override string? OriginalId(CustomerCsv r) => r.CustomerId;

    protected override void Validate(CustomerCsv r, Validator v)
    {
        v.PositiveInteger(r.CustomerId, "IdCliente");
        v.Required(r.Name, "Nombre", 100);
        v.OptionalEmail(r.Email, 150);
    }

    protected override Customer Transform(CustomerCsv r) => new()
    {
        CustomerId = Conversions.Integer(r.CustomerId!),
        Name = r.Name!.Trim(),
        Email = Conversions.NullIfBlank(r.Email)?.ToLowerInvariant()
    };

    protected override string Id(Customer c) => c.CustomerId.ToString();

    protected override bool Insert(Customer customer, LoadResult result)
    {
        if (customer.Email != null && _references.EmailRegistered(customer.Email))
            throw new ValidationException($"Duplicate email '{customer.Email}'");

        if (!_dao.Insert(customer))
            return false;

        _references.RegisterCustomer(customer.CustomerId, customer.Email);
        return true;
    }
}
