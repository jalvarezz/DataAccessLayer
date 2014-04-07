using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Framework
{
	public class GenericRepository<TEntity> : IDisposable, IRepository<TEntity> where TEntity : class
	{
		protected internal DbContext _context;
		protected internal readonly DbSet<TEntity> _dbSet;
		protected bool IsInitialized;

		protected GenericRepository(DbContext context)
		{
			_context = context;
			_dbSet = _context.Set<TEntity>();
		}

		public virtual TEntity GetByID(object id)
		{
			return _dbSet.Find(id);
		}

		public void Insert(TEntity entity)
		{
			_dbSet.Add(entity);
		}
		
		public void Delete(object id)
		{
			_dbSet.Remove(GetByID(id));
		}
		
		public void Delete(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }

            _dbSet.Remove(entityToDelete);
        }

		public void Update(TEntity entity)
		{
			_context.Entry(entity).State = EntityState.Modified;
		}

		public Task SaveAsync()
		{
			return _context.SaveChangesAsync();
		}

		public void Save()
		{
			_context.SaveChanges();
		}

		/// <summary>
		///     Retrieves a list of TEntities, filtered, sorted and projected to TResult.
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="transform">The transform.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="orderBy">The order by.</param>
		/// <param name="pageSize">The nullable <see cref="int"/> page size.</param>
		/// <param name="currentPage">The <see cref="int"/> current page.</param>
		/// <returns>The <see cref="IEnumerable{TResult}"/> result set.</returns>
		public virtual IEnumerable<TResult> Get<TResult>(
			 Func<IQueryable<TEntity>, IQueryable<TResult>> transform,
			 Expression<Func<TEntity, bool>> filter = null,
			 Func<IQueryable<TResult>, IOrderedQueryable<TResult>> orderBy = null,
			 int? pageSize = null,
			 int currentPage = 0)
		{
			// _dbSet is DbSet<TEntity> property of a DbContext
			var query = (filter == null) ? _dbSet : _dbSet.Where(filter);

			var notSortedResults = transform(query);

			var sortedResults = orderBy == null ? (IEnumerable<TResult>)notSortedResults : orderBy(notSortedResults).ToList();

			if (pageSize.HasValue)
			{
				var excludedRows = (currentPage - 1) * pageSize.Value;
				sortedResults = sortedResults.Skip(excludedRows).Take(pageSize.Value);
			}

			return sortedResults;
		}
		
		/// <summary>
		///     Retrieves the quantity of a filtered set of TEntities.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns>TEntity count.</returns>
		public virtual int GetCount(Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null ? this._dbSet.Count() : this._dbSet.Where(filter).Count();
        }
		
		/// <summary>
		///		Method to Execute a Stored Procedure and project the result to a TResult type.
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="transform">The transform.</param>
		/// <param name="parameters">The Stored Procedure parameters.</param>
		/// <returns>The <see cref="IEnumerable{TResult}"/> result set.</returns>
		public virtual IEnumerable<TResult> ExecuteStoredProc<TResult>(Func<IEnumerable<TEntity>, IEnumerable<TResult>> transform, object[] parameters)
        {
            Type contextType = typeof(PPEntities);
            string procedureName = typeof(TEntity).Name.Replace("_Result", "");

			var procedureResult = contextType.GetMethod(procedureName).Invoke(_context, parameters) as IEnumerable<TEntity>;

			if (procedureResult == null) return null;

			var result = transform(procedureResult);

			return result.ToList();

            return null;
        }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_context.Dispose();
			}
		}
	}
}