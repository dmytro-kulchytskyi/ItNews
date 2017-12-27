﻿using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using System.Configuration;

namespace ItNews.Nhibernate
{
    public class Configuration
    {
        private static ISessionFactory ConfigureSessionFactory()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ToString();
            var sessionFactory = Fluently.Configure().Database(MsSqlConfiguration.MsSql2008.ConnectionString(connectionString).ShowSql())
                                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Configuration>())
                                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(true, true))
                                .BuildSessionFactory();
            return sessionFactory;
        }

        public static void RegisterDependencies(IKernel kernel)
        {
            kernel.Bind<ISessionFactory>().ToConstant(ConfigureSessionFactory());
        }
    }
}