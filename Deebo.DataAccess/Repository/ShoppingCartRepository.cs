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
    public class ShoppingCartRepository : Repository<ShoppingCart> , IShoppingCartRepository
    {
        private readonly ApplicationDbContext context;
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public void Update(ShoppingCart shoppingcart)
        {
            context.ShoppingCarts.Update(shoppingcart);
        }
    }
}
