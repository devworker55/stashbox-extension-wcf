using System.Collections.Generic;

namespace stashbox.extension.wcf.sample.domain
{
    public interface IProductRepository
    {
        Product GetById(int productId);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly List<Product> _products;

        public ProductRepository()
        {
            _products = new List<Product>
            {
                new Product {Condition = Condition.New, Name = "R710", Description = "Dell Mid-Range 2U All Purpose Server", Id = 1, Price = 2599m},
                new Product {Condition = Condition.Refurbished, Name = "R610", Description = "Dell Entry Level Compute 2U Server", Id = 2, Price = 1649m},
                new Product {Condition = Condition.Used, Name = "R510", Description = "Dell Mid-Range 2U Storage Server", Id = 3, Price = 1209m}
            };
        }
        public Product GetById(int productId)
        {
            return _products.Find(p => p.Id == productId);
        }
    }
}