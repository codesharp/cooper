//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using Cooper.Model.Tasks;

namespace Cooper.Repositories
{
    public class TaskRepository : CodeSharp.Core.Castles.NHibernateRepositoryBase<long, Task>, ITaskRepository
    {
    }
}
