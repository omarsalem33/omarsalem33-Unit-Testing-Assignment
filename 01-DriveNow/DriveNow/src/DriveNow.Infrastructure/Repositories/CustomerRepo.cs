using DriveNow.Domain.Entities;
using DriveNow.Infrastructure.Persistence;

namespace DriveNow.Infrastructure.Repositories;

public class CustomerRepo : DriveNow.Domain.Abstractions.ICustomerRepository
{
    public Customer? GetById(int id) =>
        InMemoryDatabase.Customers.FirstOrDefault(customer => customer.Id == id);

    public List<Customer> GetAll() => InMemoryDatabase.Customers.ToList();

    public void Update(Customer customer)
    {
        var index = InMemoryDatabase.Customers.FindIndex(item => item.Id == customer.Id);
        if (index >= 0)
            InMemoryDatabase.Customers[index] = customer;
    }
}


