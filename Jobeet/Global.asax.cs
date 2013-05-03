using System.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Configuration = NHibernate.Cfg.Configuration;

namespace Jobeet
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			BootstrapAutofac();
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}

		private static void BootstrapAutofac()
		{
			var builder = new ContainerBuilder();
			RegisterDatabase(builder);
			builder.RegisterControllers(typeof(MvcApplication).Assembly);
			IContainer container = builder.Build();
			container.ActivateGlimpse();

			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
		}

		private static void RegisterDatabase(ContainerBuilder builder)
		{
			builder.Register(x =>
			{
				ISessionFactory sessionFactory = Fluently.Configure()
					.Database(ConfigureDatabase)
					.Mappings(ConfigureMappings)
					.ExposeConfiguration(ModifyConfiguration)
					.BuildSessionFactory();
				NHibernate.Glimpse.Plugin.RegisterSessionFactory(sessionFactory);
				return sessionFactory;
			});

			builder.Register(x => x.Resolve<ISessionFactory>().OpenSession())
				.InstancePerHttpRequest();
		}

		private static void ModifyConfiguration(Configuration c)
		{
			c.Properties["format_sql"] = "true";
			c.Properties["generata_statistics"] = "true";
		}

		private static void ConfigureMappings(MappingConfiguration m)
		{
			m.FluentMappings.AddFromAssemblyOf<MvcApplication>();
		}

		private static MsSqlConfiguration ConfigureDatabase()
		{
			return MsSqlConfiguration.MsSql2008.ConnectionString(ConfigurationManager.ConnectionStrings["jobeet-db"].ConnectionString);
		}
	}
}