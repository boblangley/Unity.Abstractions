﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Builder;
using Unity.Builder.Policy;
using Unity.Policy;
using Unity.Registration;
using Unity.Utility;

namespace Unity.Injection
{
    /// <summary>
    /// A class that holds the collection of information
    /// for a constructor, so that the container can
    /// be configured to call this constructor.
    /// </summary>
    public class InjectionConstructor : InjectionMember
    {
        private readonly List<InjectionParameterValue> _parameterValues;

        /// <summary>
        /// Create a new instance of <see cref="InjectionConstructor"/> that looks
        /// for a constructor with the given set of parameters.
        /// </summary>
        /// <param name="parameterValues">The values for the parameters, that will
        /// be converted to <see cref="InjectionParameterValue"/> objects.</param>
        public InjectionConstructor(params object[] parameterValues)
        {
            _parameterValues = InjectionParameterValue.ToParameters(parameterValues).ToList();
        }

        /// <summary>
        /// Add policies to the <paramref name="policies"/> to configure the
        /// container to call this constructor with the appropriate parameter values.
        /// </summary>
        /// <param name="serviceType">Interface registered, ignored in this implementation.</param>
        /// <param name="implementationType">Type to register.</param>
        /// <param name="name">Name used to resolve the type object.</param>
        /// <param name="policies">Policy list to add policies to.</param>
        public override void AddPolicies(Type serviceType, Type implementationType, string name, IPolicyList policies)
        {
            ConstructorInfo ctor = FindConstructor(implementationType);
            policies.Set<IConstructorSelectorPolicy>(
                new SpecifiedConstructorSelectorPolicy(ctor, _parameterValues.ToArray()),
                new NamedTypeBuildKey(implementationType, name));
        }

        private ConstructorInfo FindConstructor(Type typeToCreate)
        {
            foreach (var ctor in typeToCreate.GetTypeInfo()
                                             .DeclaredConstructors
                                             .Where(c => c.IsStatic == false && c.IsPublic))
            {
                if (_parameterValues.Matches(ctor.GetParameters().Select(p => p.ParameterType)))
                {
                    return ctor;
                }
            }

            string signature = string.Join(", ", _parameterValues.Select(p => p.ParameterTypeName).ToArray());

            throw new InvalidOperationException(
                string.Format(CultureInfo.CurrentCulture,
                    Constants.NoSuchConstructor,
                    typeToCreate.FullName,
                    signature));
        }
    }
}
