﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces
{
    public interface IMultiCardinalityExecutable<TType> : IQueryBuilder
    {
        Task<IReadOnlyCollection<TType?>> ExecuteAsync(IEdgeDBQueryable edgedb, CancellationToken token = default);
    }
}