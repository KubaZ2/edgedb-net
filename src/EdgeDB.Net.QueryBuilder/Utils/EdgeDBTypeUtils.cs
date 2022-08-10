﻿using EdgeDB.Serializer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     A class of utility functions for edgedb types.
    /// </summary>
    internal static class EdgeDBTypeUtils
    {
        private static readonly ConcurrentDictionary<Type, EdgeDBTypeInfo> _typeCache = new();

        /// <summary>
        ///     Represents type info about a compatable edgedb type.
        /// </summary>
        internal class EdgeDBTypeInfo
        {
            /// <summary>
            ///     The dotnet type of the edgedb type.
            /// </summary>
            public readonly Type DotnetType;

            /// <summary>
            ///     The name of the edgedb type.
            /// </summary>
            public readonly string EdgeDBType;

            /// <summary>
            ///     Whether or not the type is an array.
            /// </summary>
            public readonly bool IsArray;

            /// <summary>
            ///     The child of the current type.
            /// </summary>
            public readonly EdgeDBTypeInfo? Child;

            /// <summary>
            ///     Constructs a new <see cref="EdgeDBTypeInfo"/>.
            /// </summary>
            /// <param name="dotnetType">The dotnet type.</param>
            /// <param name="edgedbType">The edgedb type.</param>
            /// <param name="isArray">Whether or not the type is an array.</param>
            /// <param name="child">The child type.</param>
            public EdgeDBTypeInfo(Type dotnetType, string edgedbType, bool isArray, EdgeDBTypeInfo? child)
            {
                DotnetType = dotnetType;
                EdgeDBType = edgedbType;
                IsArray = isArray;
                Child = child;
            }

            /// <summary>
            ///     Turns the current <see cref="EdgeDBTypeInfo"/> to the equivalent edgedb type.
            /// </summary>
            /// <returns>
            ///     The equivalent edgedb type.
            /// </returns>
            public override string ToString()
            {
                if (IsArray)
                    return $"array<{Child}>";
                return EdgeDBType;
            }
        }

        /// <summary>
        ///     Gets either a scalar type name or edgedb type name for the current type.
        /// </summary>
        /// <example>
        ///     <c>string</c> -> <c>std::str</c>.
        /// </example>
        /// <param name="type">The dotnet type to get the equivalent edgedb type.</param>
        /// <returns>
        ///     The equivalent edgedb type.
        /// </returns>
        public static string GetEdgeDBScalarOrTypename(Type type)
        {
            if (TryGetScalarType(type, out var info))
                return info.ToString();

            return type.GetEdgeDBTypeName();
        }

        /// <summary>
        ///     Attempts to get a scalar type for the given dotnet type.
        /// </summary>
        /// <param name="type">The dotnet type to get the scalar type for.</param>
        /// <param name="info">The out parameter containing the type info.</param>
        /// <returns>
        ///     <see langword="true"/> if the edgedb scalar type could be found; otherwise <see langword="false"/>.
        /// </returns>
        public static bool TryGetScalarType(Type type, [MaybeNullWhen(false)] out EdgeDBTypeInfo info)
        {
            if (_typeCache.TryGetValue(type, out info))
                return true;

            info = null;

            Type? enumerableType = type.GetInterfaces().FirstOrDefault(x => x.Name == "IEnumerable`1");

            EdgeDBTypeInfo? child = null;
            var hasChild = enumerableType != null && TryGetScalarType(enumerableType.GenericTypeArguments[0], out child);
            var scalar = PacketSerializer.GetEdgeQLType(type);

            if (scalar != null)
                info = new(type, scalar, false, child);
            else if (hasChild)
                info = new(type, "array", true, child);

            return info != null && _typeCache.TryAdd(type, info);
        }

        /// <summary>
        ///     Checks whether or not a type is a valid link type.
        /// </summary>
        /// <param name="type">The type to check whether or not its a link.</param>
        /// <param name="isMultiLink">
        ///     The out parameter which is <see langword="true"/> 
        ///     if the type is a 'multi link'; otherwise a 'single link'.
        /// </param>
        /// <param name="innerLinkType">The inner type of the multi link if <paramref name="isMultiLink"/> is <see langword="true"/>; otherwise <see langword="null"/>.</param>
        /// <returns>
        ///     <see langword="true"/> if the given type is a link; otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsLink(Type type, out bool isMultiLink, [MaybeNullWhen(false)] out Type? innerLinkType)
        {
            innerLinkType = null;
            isMultiLink = false;

            Type? enumerableType = null;
            if (type != typeof(string) && (enumerableType = type.GetInterfaces().FirstOrDefault(x => ReflectionUtils.IsSubTypeOfGenericType(typeof(IEnumerable<>), x))) != null)
            {
                innerLinkType = enumerableType.GenericTypeArguments[0];
                isMultiLink = true;
                var result = IsLink(innerLinkType, out _, out var linkType);
                innerLinkType ??= linkType;
                return result;
            }

            return TypeBuilder.IsValidObjectType(type);
        }
    }
}