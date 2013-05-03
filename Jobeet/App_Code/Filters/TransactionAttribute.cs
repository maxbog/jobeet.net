using System.Data;
using System.Web.Mvc;
using NHibernate;

namespace Jobeet.Filters
{
	public class TransactionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
			ITransaction currentTransaction = DependencyResolver.Current.GetService<ISession>().Transaction;

			try
			{
				if (currentTransaction.IsActive)
					if (filterContext.Exception != null)
						currentTransaction.Rollback();
					else
						currentTransaction.Commit();
			}
			finally
			{
				currentTransaction.Dispose();
			}
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			DependencyResolver.Current.GetService<ISession>().BeginTransaction(IsolationLevel.ReadCommitted);
		}
	}
}