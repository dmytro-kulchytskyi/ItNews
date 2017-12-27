﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItNews.Business
{
    public interface IUnitOfWork : IDisposable
    {
        IUnitOfWork BeginTransaction();
        void RollbackTransaction();
        void CommitTransaction();
    }
}