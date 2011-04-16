using System;
using NHibernateWorkshop.Crud;
using NHibernateWorkshop.SessionFactoryBuilders;
using Northwind.Builders;
using Northwind.Entities;
using NUnit.Framework;

namespace NHibernateWorkshop.CrudFixtures
{
    [TestFixture]
    public class ProductSourceCrudFixture : CrudFixture<ProductSource, Guid> 
    {
        protected override ProductSource BuildEntity()
        {
            var productSource = new ProductSourceBuilder().Build();
            // there are no cascades defined for the following assocations, so we have to manually turn these
            // transient instances to persistent ones
            Session.Save(productSource.Product);
            return productSource;
        }

        protected override void ModifyEntity(ProductSource entity)
        {
            entity.Cost = entity.Cost * 2;
        }

        protected override void AssertAreEqual(ProductSource expectedEntity, ProductSource actualEntity)
        {
            Assert.AreEqual(expectedEntity.Cost, actualEntity.Cost);
            Assert.AreEqual(expectedEntity.Product, actualEntity.Product);
            Assert.AreEqual(expectedEntity.Supplier, actualEntity.Supplier);
        }

        protected override void AssertValidId(ProductSource entity)
        {
            Assert.AreNotEqual(Guid.Empty, entity.Id);
        }
    }
}