

using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;
using System.Linq.Expressions;

namespace EduCore.Persistence.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}