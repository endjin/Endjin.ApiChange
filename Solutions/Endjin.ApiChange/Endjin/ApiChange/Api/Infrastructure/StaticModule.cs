// <copyright file="StaticModule.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Infrastructure
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Permissions;

    internal class StaticModule
    {
        private const string ModuleAssemblyName = "AloisDynamicCaster";

        private static Module myUnsafeModule;

        public static Module UnsafeModule
        {
            get
            {
                if (myUnsafeModule == null)
                {
                    var assemblyName = new AssemblyName(ModuleAssemblyName);
                    var aBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                    ModuleBuilder mBuilder = aBuilder.DefineDynamicModule(ModuleAssemblyName);
                    // set SkipVerification=true on our assembly to prevent VerificationExceptions which warn
                    // about unsafe things but we want to do unsafe things after all.
                    Type secAttrib = typeof(SecurityPermissionAttribute);
                    ConstructorInfo secCtor = secAttrib.GetConstructor(new[] { typeof(SecurityAction) });
                    var attribBuilder = new CustomAttributeBuilder(
                        secCtor, 
                        new object[] { SecurityAction.Assert },
                        new[] { secAttrib.GetProperty("SkipVerification", BindingFlags.Instance | BindingFlags.Public) },
                        new object[] { true });

                    aBuilder.SetCustomAttribute(attribBuilder);

                    TypeBuilder tb = mBuilder.DefineType("MyDynamicType", TypeAttributes.Public);
                    myUnsafeModule = tb.CreateTypeInfo().Module;
                }

                return myUnsafeModule;
            }
        }
    }
}