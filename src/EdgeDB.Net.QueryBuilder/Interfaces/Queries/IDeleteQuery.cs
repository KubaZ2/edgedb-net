﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    public interface IDeleteQuery<TType> : IMultiCardinalityExecutable<TType>
    {
        IDeleteQuery<TType> Filter(Expression<Func<TType, bool>> filter);
        IDeleteQuery<TType> Filter(Expression<Func<TType, QueryContext, bool>> filter);
        IDeleteQuery<TType> OrderBy(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);
        IDeleteQuery<TType> OrderBy(Expression<Func<TType, QueryContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);
        IDeleteQuery<TType> OrderByDesending(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);
        IDeleteQuery<TType> OrderByDesending(Expression<Func<TType, QueryContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);
        IDeleteQuery<TType> Offset(long offset);
        IDeleteQuery<TType> Offset(Expression<Func<QueryContext, long>> offset);
        IDeleteQuery<TType> Limit(long limit);
        IDeleteQuery<TType> Limit(Expression<Func<QueryContext, long>> limit);
    }
}