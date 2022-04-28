using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Jimbot.Config;
using Jimbot.Logging;
using Ninject;
using SQLite;

namespace Jimbot.Db {
    public class DbRepository : IDisposable {
        private readonly SQLiteConnection db;
        private readonly Logger log;

        public DbRepository(AppConfig cfg, [Named("db")] Logger log) {
            db = new SQLiteConnection(cfg.DbPath);
            this.log = log;
        }

        public void CreateOrMigrateTable<T>() where T : class, new () {
            db.CreateTable<T>();
        }

        public T FindOne<T>(int id) where T : class, new() {
            try {
                return db.Find<T>(id);
            }
            catch (Exception e) {
                log.Warn(e.Message, e);
                return null;
            }
        }

        public T FindOne<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, int>> order = null, bool desc = true) where T : class, new() {
            try {
                // there is Get() but Get() throws invalidop when there are no results.
                // but it is actually expected behaviour, so stick to using find.
                if (order == null) {
                    return db.Find(predicate);
                }
                if (desc) {
                    return db.Table<T>().Where(predicate).OrderByDescending(order).FirstOrDefault();
                }
                else {
                    return db.Table<T>().Where(predicate).OrderBy(order).FirstOrDefault();
                }
                
            }
            catch (Exception e) {
                log.Warn(e.Message, e);
                return null;
            }
        }

        public List<T> FindAll<T>(Expression<Func<T, bool>> predicate) where T : class, new() {
            try {
                return db.Table<T>().Where(predicate).ToList();
            }
            catch (Exception e) {
                log.Warn(e.Message, e);
                return default;
            }
        }

        public void Dispose() {
            db?.Dispose();
        }

        public void Insert<T>(T obj) where T : class, new() {
            db.Insert(obj);
        }

        public void Update<T>(T obj) where T : class, new() {
            db.Update(obj);
        }

        public void InsertOrReplace<T>(T obj) where T : class, new() {
            db.InsertOrReplace(obj);
        }

        public void Delete<T>(T obj) where T : class, new() {
            db.Delete(obj);
        }
    }
}