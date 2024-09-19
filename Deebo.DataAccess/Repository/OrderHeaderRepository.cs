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
    public class OrderHeaderRepository : Repository<OrderHeader> , IOrderHeaderRepository
    {
        private readonly ApplicationDbContext context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public void Update(OrderHeader orderHeader)
        {
            context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderstatus, string? paymentstatus = null)
        {
            var orderfromdb = context.OrderHeaders.FirstOrDefault(e=>e.Id == id);
            if (orderfromdb != null)
            {
                orderfromdb.OrderStatus = orderstatus;
                if (!string.IsNullOrEmpty(paymentstatus))
                {
                    orderfromdb.PaymentStatus = paymentstatus;
                }
            }
        }
    }
}
