﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// This defines some context information used to perform query composition. 
    /// </summary>
    public class ODataQueryContext
    {
        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element CLR type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EdmModel that includes the <see cref="IEdmType"/> corresponding to
        /// the given <paramref name="elementClrType"/>.</param>
        /// <param name="elementClrType">The CLR type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        /// <param name="serviceProvider">DI Service Provider <see cref="IServiceProvider"/>.</param>
        public ODataQueryContext(IEdmModel model, Type elementClrType, ODataPath path, IServiceProvider serviceProvider)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (elementClrType == null)
            {
                throw Error.ArgumentNull("elementClrType");
            }

            ElementType = model.GetEdmType(elementClrType);

            if (ElementType == null)
            {
                throw Error.Argument("elementClrType", SRResources.ClrTypeNotInModel, elementClrType.FullName);
            }

            if (serviceProvider == null)
            {
                throw Error.ArgumentNull("serviceProvider");
            }

            ElementClrType = elementClrType;
            Model = model;
            Path = path;
            NavigationSource = GetNavigationSource(Model, ElementType, path);
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element EDM type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EDM model the given EDM type belongs to.</param>
        /// <param name="elementType">The EDM type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        /// <param name="serviceProvider">DI Service Provider <see cref="IServiceProvider"/>.</param>
        public ODataQueryContext(IEdmModel model, IEdmType elementType, ODataPath path, IServiceProvider serviceProvider)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull("serviceProvider");
            }

            Model = model;
            ElementType = elementType;
            Path = path;
            NavigationSource = GetNavigationSource(Model, ElementType, path);
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the given <see cref="IEdmModel"/> that contains the EntitySet.
        /// </summary>
        public IEdmModel Model { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmType"/> of the element.
        /// </summary>
        public IEdmType ElementType { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmNavigationSource"/> that contains the element.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets the CLR type of the element.
        /// </summary>
        public Type ElementClrType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataPath"/>.
        /// </summary>
        public ODataPath Path { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        private static IEdmNavigationSource GetNavigationSource(IEdmModel model, IEdmType elementType, ODataPath odataPath)
        {
            Contract.Assert(model != null);
            Contract.Assert(elementType != null);

            IEdmNavigationSource navigationSource = (odataPath != null) ? odataPath.NavigationSource : null;
            if (navigationSource != null)
            {
                return navigationSource;
            }

            IEdmEntityContainer entityContainer = model.EntityContainer;
            if (entityContainer == null)
            {
                return null;
            }

            List<IEdmEntitySet> matchedNavigationSources =
                entityContainer.EntitySets().Where(e => e.EntityType() == elementType).ToList();

            return (matchedNavigationSources.Count != 1) ? null : matchedNavigationSources[0];
        }
    }
}