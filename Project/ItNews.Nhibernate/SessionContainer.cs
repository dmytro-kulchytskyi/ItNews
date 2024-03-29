﻿using ItNews.Business;
using ItNews.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ItNews.Nhibernate
{
    public class SessionContainer : IDisposable
    {
        private const string CurrentSessionContainerKey = "CurrentSessionContainer";

        private RequestDataStorage requestDataStorage;

        private readonly SessionContainer parent;

        public SessionContainer()
        {
            requestDataStorage = DependencyResolver.Current.GetService<RequestDataStorage>();

            parent = requestDataStorage.GetValue<SessionContainer>(CurrentSessionContainerKey);
            requestDataStorage.SetValue(CurrentSessionContainerKey, this);

            Session = IsBaseContainer ? DependencyResolver.Current.GetService<ISessionFactory>().OpenSession() : parent.Session;
        }
        
        public ISession Session { get;  }

        private bool IsBaseContainer => parent == null;

        public static SessionContainer Open() => 
            new SessionContainer();

        public void Dispose()
        {
            if (IsBaseContainer)
                Session.Dispose();

            requestDataStorage.SetValue(CurrentSessionContainerKey, parent);
        }
    }
}
