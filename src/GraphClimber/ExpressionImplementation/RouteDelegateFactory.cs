using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class RouteDelegateFactory
    {
        private readonly IClimbStore _climbStore;
        private readonly Type _processorType;
        private readonly SpecialMethodMutator _specialMutator;
        private readonly IMethodMapper _methodMapper;

        public RouteDelegateFactory(Type processorType, IMethodMapper methodMapper, IClimbStore climbStore)
        {
            _processorType = processorType;
            _climbStore = climbStore;
            _specialMutator = new SpecialMethodMutator(processorType);
            _methodMapper = methodMapper;
        }

        public RouteDelegate GetRouteDelegate(IStateMember member, Type runtimeMemberType,
                                              IStateMemberProvider stateMemberProvider)
        {
            ParameterExpression processor = Expression.Parameter(typeof (object), "processor");
            ParameterExpression owner = Expression.Parameter(typeof (object), "owner");
            ParameterExpression skipSpecialMethods = Expression.Parameter(typeof (bool), "skipSpecialMethods");
            ParameterExpression elementIndex = Expression.Parameter(typeof(int[]), "elementIndex");

            var castedProcessor =
                processor.Convert(_processorType);

            DescriptorWriter descriptorWriter = new DescriptorWriter(_climbStore);

            DescriptorVariable descriptor =
                descriptorWriter.GetDescriptor(processor, owner, elementIndex, member, runtimeMemberType, stateMemberProvider);

            MethodInfo methodToCall =
                _methodMapper.GetMethod(_processorType, member, runtimeMemberType, true);

            MethodCallExpression callProcess =
                Expression.Call(castedProcessor, methodToCall, descriptor.Reference);

            Expression callProcessWithSpecialMethods =
                _specialMutator.Mutate(callProcess, castedProcessor, owner, member, descriptor.Reference,
                                       EmptyIndex.Constant);

            BlockExpression body =
                Expression.Block(new[] {descriptor.Reference},
                    descriptor.Declaration,
                    Expression.Condition(skipSpecialMethods,
                        callProcess,
                        callProcessWithSpecialMethods));

            Expression<RouteDelegate> lambda =
                Expression.Lambda<RouteDelegate>(body,
                    "Route_" + runtimeMemberType.Name,
                    new[] {processor, owner, skipSpecialMethods, elementIndex});

            return lambda.Compile();
        }
    }
}