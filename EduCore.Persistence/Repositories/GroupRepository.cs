using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;
using EduCore.Domain.Entities;

namespace EduCore.Persistence.Repositories
{
    public class GroupRepository : GenericRepository<Group>, IGroupRepository
    {

        public GroupRepository(ApplicationDbContext context) : base(context)
        {
            
        }
    }
}
