using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace aspnetapp.Controllers
{
    public partial class FilterBuilder
    {
        private static readonly ConcurrentDictionary<FilterTypes, MethodCallExpression> ExpressionCache = new ConcurrentDictionary<FilterTypes,MethodCallExpression>();
        static FilterBuilder()
        {
            FillBasic();
            FillGenerated();
        }

        private static void FillBasic()
        {
            Cache[FilterTypes.empty] = (a, q) => true;
            var filterMethods = typeof(FilterBuilder)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes<FilterAttribute>().Any());

            foreach (var method in filterMethods)
            {
                foreach (var filter in method.GetCustomAttributes<FilterAttribute>())
                {
                    var expression = CreateCallExpression(method);
                    if (!ExpressionCache.TryAdd(filter.Type, expression))
                    {
                        throw new InvalidOperationException($"Duplicated filter type {filter.Type}");
                    }

                    Cache[filter.Type] = Expression
                        .Lambda<Predicate>(expression, AccountParameter, QueryParameter)
                        .Compile(false);
                }
            }

        }

        private static readonly ParameterExpression AccountParameter = Expression.Parameter(typeof(Account));
        private static readonly ParameterExpression QueryParameter = Expression.Parameter(typeof(AbstractQuery));

        private static MethodCallExpression CreateCallExpression(MethodInfo method)
        {
            return Expression.Call(method, AccountParameter, QueryParameter);
        }

        private static Predicate CreateDelegate(FilterTypes filters)
        {

            //Sort predicates by heaviness

            Expression resultExpression = null;
            FilterTypes sexStatusFilter = (filters & FilterTypes.sex_status_all_types);
            if (sexStatusFilter != FilterTypes.empty)
            {
                resultExpression = ExpressionCache[sexStatusFilter];
            }

            filters = filters.ResetFlags(sexStatusFilter);

            foreach (FilterTypes filterType in filters.EnumerateSetFlags())
            {
                var expression = ExpressionCache[filterType];

                if (resultExpression == null)
                {
                    resultExpression = expression;
                }
                else
                {
                    resultExpression = Expression.AndAlso(resultExpression, expression);
                }
            }

            return Expression
                .Lambda<Predicate>(resultExpression, AccountParameter, QueryParameter)
                .Compile(false);
        }
    }
}