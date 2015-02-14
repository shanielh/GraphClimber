using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class DescriptorVariable
    {
        private readonly ParameterExpression _reference;
        private readonly BinaryExpression _descriptorDeclaration;

        public DescriptorVariable(IClimbStore store, Expression processor, Expression owner, IStateMember member, Type runtimeType)
        {
            Type descriptorType =
                DescriptorExtensions.GetDescriptorType(member, runtimeType);

            Type memberLocalType =
                typeof(MemberLocal<,>).MakeGenericType(member.MemberType, runtimeType);

            _reference = Expression.Variable(descriptorType, "descriptor");

            object memberLocal = Activator.CreateInstance(memberLocalType, store, member);

            ConstructorInfo constructor = descriptorType.GetConstructors().FirstOrDefault();

            NewExpression creation =
                Expression.New(constructor,
                    processor,
                    owner,
                    Expression.Constant(memberLocal),
                    Expression.Constant(store));

            BinaryExpression assign = Expression.Assign(_reference, creation);

            _descriptorDeclaration = assign;
        }

        public ParameterExpression Reference
        {
            get
            {
                return _reference;
            }
        }

        public Expression Declaration
        {
            get { return _descriptorDeclaration; }
        }
    }
}