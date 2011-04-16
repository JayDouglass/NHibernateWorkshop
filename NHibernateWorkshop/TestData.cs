using System.Linq;
using NHibernate;
using Northwind.Components;
using Northwind.Entities;
using Northwind.Enums;
using QuickGenerate;
using QuickGenerate.Meta;
using QuickGenerate.Primitives;

namespace NHibernateWorkshop
{
    public static class TestData
    {
        private static DomainGenerator WithAddress(DomainGenerator generator)
        {
            return generator
                .With<Address>(options => options.For(address => address.Street, new StringGenerator(1, 100)))
                .With<Address>(options => options.For(address => address.City, new StringGenerator(1, 100)))
                .With<Address>(options => options.For(address => address.Country, new StringGenerator(1, 100)));
        }

        private static DomainGenerator EmployeeGenerator(ISession session)
        {
            return WithAddress(new DomainGenerator())
                .With<Employee>(options => options.Ignore(employee => employee.Id))
                .OneToOne<Employee, Address>((employee, address) => employee.Address = address)
                .With<Employee>(options => options.For(employee => employee.FirstName, new StringGenerator(1, 50)))
                .With<Employee>(options => options.For(employee => employee.LastName, new StringGenerator(1, 75)))
                .With<Employee>(options => options.For(employee => employee.Title, new StringGenerator(1, 50)))
                .With<Employee>(options => options.For(employee => employee.Phone, new StringGenerator(1, 15)));
        }

        public static void Create(ISession session)
        {
            var customers = WithAddress(new DomainGenerator())
                .With<Customer>(options => options.Ignore(customer => customer.Id))
                .OneToOne<Customer, Address>((customer, address) => customer.Address = address)
                .With<Customer>(options => options.For(customer => customer.Name, new StringGenerator(5, 100)))
                .With<Customer>(options => options.For(customer => customer.DiscountPercentage, new DoubleGenerator(0, 25)))
                .ForEach<Customer>(customer => session.Save(customer))
                .Many<Customer>(20, 40)
                .ToArray();

            var customers2 = customers.Where(c => c.Name.Length < 5);

            var managers = EmployeeGenerator(session)
                .ForEach<Employee>(employee => session.Save(employee))
                .Many<Employee>(2);

            var employees = EmployeeGenerator(session)
                .ForEach<Employee>(employee => Maybe.Do(() => managers.PickOne().AddSubordinate(employee)))
                .ForEach<Employee>(employee => session.Save(employee))
                .Many<Employee>(20)
                .ToArray();

            var suppliers = WithAddress(new DomainGenerator())
                .With<Supplier>(options => options.Ignore(supplier => supplier.Id))
                .OneToOne<Supplier, Address>((supplier, address) => supplier.Address = address)
                .With<Supplier>(options => options.For(supplier => supplier.Website, new StringGenerator(1, 100)))
                .Many<Supplier>(20)
                .ToArray();

            var products = new DomainGenerator()
                .With<ProductSource>(options => options.Ignore(productsource => productsource.Id))
                .ForEach<ProductSource>(productsource => session.Save(productsource))
                .With<Product>(options => options.Ignore(product => product.Id))
                .With<Product>(options => options.Ignore(product => product.Version))
                .With<Product>(options => options.For(
                    product => product.Category,
                    ProductCategory.Beverages,
                    ProductCategory.Condiments,
                    ProductCategory.DairyProducts,
                    ProductCategory.Produce))
                .With<Product>(g => g.Method<double>(1, 5, (product, d) => product.AddSource(suppliers.PickOne(), d)))
                .With<Product>(options => options.For(product => product.Name, new StringGenerator(1, 50)))
                .ForEach<Product>(product => session.Save(product))
                .Many<Product>(30)
                .ToArray();

            WithAddress(new DomainGenerator())
                .With<OrderItem>(options => options.Ignore(item => item.Id))
                .With<OrderItem>(options => options.For(item => item.Product, products))
                .With<Order>(options => options.Ignore(order => order.Id))
                .OneToMany<Order, OrderItem>(1, 5, (order, item) => order.AddItem(item))
                .With<Order>(options => options.For(order => order.Customer, customers))
                .With<Order>(options => options.For(order => order.Employee, employees))
                .OneToOne<Order, Address>((order, address) => order.DeliveryAddress = address)
                .ForEach<Order>(order => session.Save(order))
                .Many<Order>(50);

            session.Flush();
        }
    }
}