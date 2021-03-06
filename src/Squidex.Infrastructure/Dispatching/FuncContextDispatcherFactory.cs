// ==========================================================================
//  FuncContextDispatcherFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;

// ReSharper disable UnusedMember.Local

namespace Squidex.Infrastructure.Dispatching
{
    internal static class FuncContextDispatcherFactory
    {
        public static Tuple<Type, Func<TTarget, object, TContext, TOut>> CreateFuncHandler<TTarget, TContext, TOut>(MethodInfo methodInfo)
        {
            var inputType = methodInfo.GetParameters()[0].ParameterType;

            var factoryMethod =
                typeof(FuncContextDispatcherFactory)
                    .GetMethod("Factory", BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(typeof(TTarget), inputType, typeof(TContext), methodInfo.ReturnType);

            var handler = factoryMethod.Invoke(null, new object[] { methodInfo });

            return new Tuple<Type, Func<TTarget, object, TContext, TOut>>(inputType, (Func<TTarget, object, TContext, TOut>)handler);
        }

        private static Func<TTarget, object, TContext, TOut> Factory<TTarget, TIn, TContext, TOut>(MethodInfo methodInfo)
        {
            var type = typeof(Func<TTarget, TIn, TContext, TOut>);

            var handler = (Func<TTarget, TIn, TContext, TOut>)methodInfo.CreateDelegate(type);

            return (target, input, context) => handler(target, (TIn)input, context);
        }
    }
}