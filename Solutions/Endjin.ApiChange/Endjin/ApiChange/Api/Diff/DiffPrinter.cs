// <copyright file="DiffPrinter.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.ApiChange.Api.Diff
{
    using System;
    using System.IO;
    using Endjin.ApiChange.Api.Introspection;
    using Mono.Cecil;

    public class DiffPrinter
    {
        private readonly TextWriter output;

        /// <summary>
        ///     Print diffs to console.
        /// </summary>
        public DiffPrinter()
        {
            this.output = Console.Out;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiffPrinter" /> class.
        /// </summary>
        /// <param name="outputStream">The output stream to print the change diff.</param>
        public DiffPrinter(TextWriter outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }

            this.output = outputStream;
        }

        internal void Print(AssemblyDiffCollection diff)
        {
            this.PrintAddedRemovedTypes(diff.AddedRemovedTypes);

            if (diff.ChangedTypes.Count > 0)
            {
                foreach (TypeDiff typeChange in diff.ChangedTypes)
                {
                    this.PrintTypeChanges(typeChange);
                }
            }
        }

        private void PrintTypeChanges(TypeDiff typeChange)
        {
            this.output.WriteLine("\t" + typeChange.TypeV1.Print());
            if (typeChange.HasChangedBaseType)
            {
                this.output.WriteLine(
                    "\t\tBase type changed: {0} -> {1}",
                    typeChange.TypeV1.IsNotNull(() =>
                        typeChange.TypeV1.BaseType.IsNotNull(() => typeChange.TypeV1.BaseType.FullName)),
                    typeChange.TypeV2.IsNotNull(() =>
                        typeChange.TypeV2.BaseType.IsNotNull(() => typeChange.TypeV2.BaseType.FullName)));
            }

            if (typeChange.Interfaces.Count > 0)
            {
                foreach (DiffResult<TypeReference> addedItf in typeChange.Interfaces.Added)
                {
                    this.output.WriteLine("\t\t+ interface: {0}", addedItf.ObjectV1.FullName);
                }

                foreach (DiffResult<TypeReference> removedItd in typeChange.Interfaces.Removed)
                {
                    this.output.WriteLine("\t\t- interface: {0}", removedItd.ObjectV1.FullName);
                }
            }

            foreach (DiffResult<EventDefinition> addedEvent in typeChange.Events.Added)
            {
                this.output.WriteLine("\t\t+ {0}", addedEvent.ObjectV1.Print());
            }

            foreach (DiffResult<EventDefinition> remEvent in typeChange.Events.Removed)
            {
                this.output.WriteLine("\t\t- {0}", remEvent.ObjectV1.Print());
            }

            foreach (DiffResult<FieldDefinition> addedField in typeChange.Fields.Added)
            {
                this.output.WriteLine("\t\t+ {0}", addedField.ObjectV1.Print(FieldPrintOptions.All));
            }

            foreach (DiffResult<FieldDefinition> remField in typeChange.Fields.Removed)
            {
                this.output.WriteLine("\t\t- {0}", remField.ObjectV1.Print(FieldPrintOptions.All));
            }

            foreach (DiffResult<MethodDefinition> addedMethod in typeChange.Methods.Added)
            {
                this.output.WriteLine("\t\t+ {0}", addedMethod.ObjectV1.Print(MethodPrintOption.Full));
            }

            foreach (DiffResult<MethodDefinition> remMethod in typeChange.Methods.Removed)
            {
                this.output.WriteLine("\t\t- {0}", remMethod.ObjectV1.Print(MethodPrintOption.Full));
            }
        }

        private void PrintAddedRemovedTypes(DiffCollection<TypeDefinition> diffCollection)
        {
            if (diffCollection.RemovedCount > 0)
            {
                this.output.WriteLine("\tRemoved {0} public type/s", diffCollection.RemovedCount);
                foreach (DiffResult<TypeDefinition> remType in diffCollection.Removed)
                {
                    this.output.WriteLine("\t\t- {0}", remType.ObjectV1.Print());
                }
            }

            if (diffCollection.AddedCount > 0)
            {
                this.output.WriteLine("\tAdded {0} public type/s", diffCollection.AddedCount);
                foreach (DiffResult<TypeDefinition> addedType in diffCollection.Added)
                {
                    this.output.WriteLine("\t\t+ {0}", addedType.ObjectV1.Print());
                }
            }
        }
    }
}