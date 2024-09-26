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
    public class ProductImageRepository : Repository<ProductImage> , IProductImageRepository
    {
        private readonly ApplicationDbContext context;
        public ProductImageRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public void Update(ProductImage image)
        {
            context.ProductImages.Update(image);
        }
    }
}
