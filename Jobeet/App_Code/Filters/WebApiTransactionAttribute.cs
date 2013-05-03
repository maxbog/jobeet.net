using System.Data;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using NHibernate;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace Jobeet.Filters
{
	public class WebApiTransactionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			base.OnActionExecuting(actionContext);
			DependencyResolver.Current.GetService<ISession>().BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			base.OnActionExecuted(actionExecutedContext);
			ITransaction currentTransaction = DependencyResolver.Current.GetService<ISession>().Transaction;

			try
			{
				if (currentTransaction.IsActive)
					if (actionExecutedContext.Exception != null)
						currentTransaction.Rollback();
					else
						currentTransaction.Commit();
			}
			finally
			{
				currentTransaction.Dispose();
			}
		}
	}
}