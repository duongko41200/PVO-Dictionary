using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using pvo_dictionary_api.Common;
using EFCore.BulkExtensions;
using pvo_dictionary_api.Models;

namespace pvo_dictionary_api.Respositories
{
    public class BaseRespository<T> : IRespositoryBase<T> where T : class, new()
    {
        protected DbSet<T> Model { get; set; }
        protected AppDbContext DbContext { get; set; }
        protected ApiOption ApiConfig { get; set; }
        public BaseRespository(ApiOption apiConfig, AppDbContext databaseContext)
        {
            DbContext = databaseContext;
            Model = databaseContext.Set<T>();
            ApiConfig = apiConfig;
        }

        /// <summary>
        ///     Retrieve all data of repository
        /// </summary>
        /// <returns>IQueryable<T></returns>
        public IQueryable<T> GetAll()
        {
            return Model.AsNoTracking();
        }

        /// <summary>
        ///     Retrieve all data of repository by Condition
        /// </summary>
        /// <param name="expression">Condition</param>
        /// <returns>IQueryable<T></returns>
        public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression)
        {
            return Model.Where(expression).AsNoTracking();
        }

        /// <summary>
        ///     Find data by id
        /// </summary>
        /// <param name="id">Table's primary key</param>
        /// <returns>Object</returns>
        public T GetById(object id)
        {
            try
            {
                var model = Model.Find(id);
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        

        /// <summary>
        ///     Save a new entity in repository
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Object</returns>
        public T Create(T entity)
        {
            try
            {
                var model = Model.Add(entity);
                return model.Entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Update a entity in repository by entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>T</returns>
        public T UpdateByEntity(T entity)
        {
            try
            {
                Model.Update(entity);
                return entity;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

      

        /// <summary>
        ///    Delete a entity in repository by entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Bool</returns>
        public bool DeleteByEntity(T entity)
        {
            try
            {
                Model.Remove(entity);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        ///     Save change to database
        /// </summary>
        public void SaveChange()
        {
            DbContext.SaveChanges();
        }

    }


}

