﻿using EdgeDB.Interfaces.Queries;
using EdgeDB.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a 'SELECT' node
    /// </summary>
    internal class SelectNode : QueryNode<SelectContext>
    {
        /// <summary>
        ///     The max recursion depth for generating default shapes.
        /// </summary>
        public const int MAX_DEPTH = 1;

        /// <inheritdoc/>
        public SelectNode(NodeBuilder builder) : base(builder) { }

        /// <summary>
        ///     Gets a shape for the given type.
        /// </summary>
        /// <param name="type">The type to get the shape for.</param>
        /// <param name="currentDepth">The current depth of the shape.</param>
        /// <returns>The shape of the given type.</returns>
        private string GetShape(Type type, int currentDepth = 0)
        {
            // get all properties that dont have the 'EdgeDBIgnore' attribute
            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);

            // map each property to its shape form
            var propertyNames = properties.Select(x =>
            {
                // get the edgedb name equivalent
                var name = x.GetEdgeDBPropertyName();

                // if its a link, build a nested shape if we're not past our max depth
                if (QueryUtils.IsLink(x.PropertyType, out var isArray, out var innerType))
                {
                    var shapeType = isArray ? innerType! : x.PropertyType;
                    if(currentDepth < MAX_DEPTH)
                        return $"{name}: {GetShape(shapeType, currentDepth + 1)}";
                    return null;
                }
                else // return just the name
                    return name;
            }).Where(x => x != null);

            // join our properties by commas and wrap it in braces
            return $"{{ {string.Join(", ", propertyNames)} }}";
        }

        /// <summary>
        ///     Gets the default shape for the current contextual type.
        /// </summary>
        /// <returns>The default shape for the current contextual type.</returns>
        private string GetDefaultShape()
            => GetShape(OperatingType);

        /// <summary>
        ///     Gets the shape based on the context of the current node.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private string GetShape()
        {
            // if no user-defined shape was passed in, generate the default shape
            if(Context.Shape == null)
            {
                return GetDefaultShape();
            }

            // generate the shape based on the contexts' expression.
            return $"{{ {ExpressionTranslator.Translate(Context.Shape, Builder.QueryVariables, Context, Builder.QueryGlobals)} }}";
        }

        public override void Visit()
        {
            // if our shape is 'new {...}' or null then parse the shape
            if (Context.Shape?.Body is NewExpression or MemberInitExpression || Context.Shape is null)
            {
                var shape = GetShape();

                if (Context.IsFreeObject)
                    Query.Append($"select {shape}");
                else
                    Query.Append($"select {Context.SelectName ?? OperatingType.GetEdgeDBTypeName()} {shape}");
            }
            else
            {
                // else we can just translate the shape and append it.
                Query.Append($"select {ExpressionTranslator.Translate(Context.Shape, Builder.QueryVariables, Context, Builder.QueryGlobals)}");
            }
        }

        /// <summary>
        ///     Adds a filter to the select node.
        /// </summary>
        /// <param name="expression">The filter predicate to add.</param>
        public void Filter(LambdaExpression expression)
        {
            var parsedExpression = ExpressionTranslator.Translate(expression, Builder.QueryVariables, Context, Builder.QueryGlobals);
            Query.Append($" filter {parsedExpression}");
        }

        /// <summary>
        ///     Adds a ordery by statement to the select node.
        /// </summary>
        /// <param name="asc">
        ///     <see langword="true"/> if the ordered result should be ascending first.
        /// </param>
        /// <param name="selector">The lambda property selector on which to order by.</param>
        /// <param name="nullPlacement">The <see langword="null"/> placement for null values.</param>
        public void OrderBy(bool asc, LambdaExpression selector, OrderByNullPlacement? nullPlacement)
        {
            var parsedExpression = ExpressionTranslator.Translate(selector, Builder.QueryVariables, Context, Builder.QueryGlobals);
            var direction = asc ? "asc" : "desc";
            Query.Append($" order by {parsedExpression} {direction}{(nullPlacement.HasValue ? $" {nullPlacement.Value.ToString().ToLowerInvariant()}" : "")}");
        }

        /// <summary>
        ///     Adds a offest statement to the select node.
        /// </summary>
        /// <param name="offset">The number of elements to offset by.</param>
        internal void Offset(long offset)
        {
            Query.Append($" offset {offset}");
        }

        /// <summary>
        ///     Adds a offest statement to the select node.
        /// </summary>
        /// <param name="exp">The expression returing the number of elements to offset by.</param>
        internal void OffsetExpression(LambdaExpression exp)
        {
            Query.Append($" offset {exp}");
        }

        /// <summary>
        ///     Adds a limit statement to the select node.
        /// </summary>
        /// <param name="limit">The number of element to limit to.</param>
        internal void Limit(long limit)
        {
            Query.Append($" limit {limit}");
        }

        /// <summary>
        ///     Adds a limit statement to the select node.
        /// </summary>
        /// <param name="exp">The expression returing the number of elements to limit to.</param>
        internal void LimitExpression(LambdaExpression exp)
        {
            Query.Append($" limit {exp}");
        }
    }
}
