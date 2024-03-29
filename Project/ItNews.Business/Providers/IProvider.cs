﻿using ItNews.Business.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItNews.Business.Providers
{
    public interface IProvider<T> 
        where T : class, IEntity
    {
        IUnitOfWork GetUnitOfWork();
        Task<IList<T>> Get(IEnumerable<string> ids);
        Task<T> Get(string id);
        Task<T> SaveOrUpdate(T instance);
        Task Delete(T instance);
        Task<IList<T>> GetList();
        Task<int> GetCount();
    }
}
