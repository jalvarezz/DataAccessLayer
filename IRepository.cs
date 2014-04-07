using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess.Framework
{
	public interface IRepository<TEntity> where TEntity : class
	{
		// Get Entity by ID
		TEntity GetByID(object id);
		
		// Insert Entity
		void Insert(TEntity entity);

		// Delete Entity by ID
		void Delete(object id);
		
		// Delete Entity
        void Delete(TEntity entityToDelete);
		
		// Update Entity
        void Update(TEntity entityToUpdate);

		void Save();
		// Super Get
		IEnumerable<TResult> Get<TResult>(
			Func<IQueryable<TEntity>, IQueryable<TResult>> transform,
			Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TResult>, IOrderedQueryable<TResult>> orderBy = null,
			int? pageSize = null,
			int currentPage = 0);
		
		// Entities Count
		int GetCount(Expression<Func<TEntity, bool>> filter = null);

		// Method to Execute a Stored Procedure
        IEnumerable<TResult> ExecuteStoredProc<TResult>(Func<IEnumerable<TEntity>, IEnumerable<TResult>> transform, object[] parameters);
	}
}