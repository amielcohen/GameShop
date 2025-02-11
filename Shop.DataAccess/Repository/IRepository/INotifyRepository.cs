using Shop.DataAccess.Repository.IRepositor;
using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.DataAccess.Repository.IRepository
{
    public interface INotifyRepository: IRepository<Notify>
    {
        void Update(Notify obj);
    }
}
