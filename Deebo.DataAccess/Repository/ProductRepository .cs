using DeeboStore.DataAccess.Data;
using DeeboStore.Models;
using DeeboStore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeeboStore.DataAccess.Repository
{
    public class ProductRepository : Repository<Product> ,IProductRepository
    {
        private readonly ApplicationDbContext context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(Product product)
        {
            var objFromDb = context.Products.FirstOrDefault(u => u.Id == product.Id);
            if (objFromDb != null)
            {
                objFromDb.Title = product.Title;
                objFromDb.Price = product.Price;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Description = product.Description;
                objFromDb.CategoryId = product.CategoryId;
                objFromDb.ProductImages = product.ProductImages;
                //objFromDb.ProductImages = obj.ProductImages;
            }
        }
    }
}
