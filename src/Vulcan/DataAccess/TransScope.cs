﻿using System;
using System.Data;

namespace Vulcan.DataAccess
{
    /// <summary>
    /// 事务管理
    /// </summary>
    public class TransScope : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransScope"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public TransScope(string connectionString)
            : this(connectionString, TransScopeOption.Required)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransScope"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="option">The option.</param>
        public TransScope(string connectionString, TransScopeOption option)
        {
            _connectionManager = ConnectionManager.GetManager(connectionString);
            if (!_connectionManager.IsExistDbTransaction() || option == TransScopeOption.RequiresNew)
            {
                _tran = _connectionManager.BeginTransaction();
                _beginTransactionIsInCurrentTransScope = true;
            }
            else
            {
                _tran = _connectionManager.Transaction;
            }
        }

        private ConnectionManager _connectionManager;
        private IDbTransaction _tran;

        /// <summary>
        /// 是否是该对象开启的事务(哪个对象负责开启则哪个对象负责提交)
        /// </summary>
        private bool _beginTransactionIsInCurrentTransScope = false;

        private bool _completed = false;

        /// <summary>
        /// Commit Transaction
        /// </summary>
        public void Commit()
        {
            if (_beginTransactionIsInCurrentTransScope && _tran != null)
            {
                _tran.Commit();
                _completed = true;
            }
        }

        /// <summary>
        /// rollback  transaction
        /// </summary>
        public void Rollback()
        {
            if (_beginTransactionIsInCurrentTransScope && _tran != null)
            {
                _tran.Rollback();
            }
        }

        /// <summary>
        /// close connection and rollback transaction
        /// </summary>
        public void Close()
        {
            if (!_completed)
            {
                Rollback();
            }

            _connectionManager.Dispose();
        }

        #region IDisposable 成员

        public void Dispose()
        {
            Close();
        }

        #endregion IDisposable 成员
    }
}