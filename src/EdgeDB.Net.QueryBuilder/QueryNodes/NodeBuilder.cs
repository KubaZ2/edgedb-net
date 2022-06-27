﻿using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class NodeBuilder
    {
        public StringBuilder Query { get; }
        public List<QueryNode> Nodes { get; }
        public NodeContext Context { get; }
        public Dictionary<string, object?> QueryVariables { get; } = new();
        public Dictionary<string, object?> QueryGlobals { get; }
        public bool IsAutoGenerated { get; init; }

        public NodeBuilder(NodeContext context, Dictionary<string, object?> globals, List<QueryNode>? nodes = null)
        {
            Query = new();
            Nodes = nodes ?? new();
            Context = context;
            QueryGlobals = globals;
        }
    }
}