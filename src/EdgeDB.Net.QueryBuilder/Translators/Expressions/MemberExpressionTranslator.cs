﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class MemberExpressionTranslator : ExpressionTranslator<MemberExpression>
    {
        public override string? Translate(MemberExpression expression, ExpressionContext context)
        {
            if(expression.Expression is ConstantExpression constant)
            {
                object? value = expression.Member.GetMemberValue(constant.Value);

                var varName = context.AddVariable(value);
                var type = PacketSerializer.GetEdgeQLType(expression.Type);
                return $"<{type}>${varName}";
            }

            return ParseMemberExpression(expression, expression.Expression is not ParameterExpression, context.IncludeSelfReference);
        }

        private static string ParseMemberExpression(MemberExpression expression, bool includeParameter = true, bool includeSelfReference = true)
        {
            List<string?> tree = new()
            {
                expression.Member.GetEdgeDBPropertyName()
            };
            
            if (expression.Expression is MemberExpression innerExp)
                tree.Add(ParseMemberExpression(innerExp));
            if (expression.Expression is ParameterExpression param)
                if(includeSelfReference)
                    tree.Add(includeParameter ? param.Name : string.Empty);

            tree.Reverse();
            return string.Join('.', tree);
        }
    }
}
