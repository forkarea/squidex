﻿// ==========================================================================
//  PropertiesTypeAccessor.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PinkParrot.Infrastructure.Reflection
{
    public sealed class PropertiesTypeAccessor
    {
        private static readonly ConcurrentDictionary<Type, PropertiesTypeAccessor> AccessorCache = new ConcurrentDictionary<Type, PropertiesTypeAccessor>();
        private readonly Dictionary<string, IPropertyAccessor> accessors = new Dictionary<string, IPropertyAccessor>();
        private readonly List<PropertyInfo> properties = new List<PropertyInfo>();
        
        public IEnumerable<PropertyInfo> Properties
        {
            get
            {
                return properties;
            }
        }

        private PropertiesTypeAccessor(Type type)
        {
            var allProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in allProperties)
            {
                accessors[property.Name] = new PropertyAccessor(type, property);

                properties.Add(property);
            }
        }

        public static PropertiesTypeAccessor Create(Type targetType)
        {
            Guard.NotNull(targetType, nameof(targetType));

            return AccessorCache.GetOrAdd(targetType, x => new PropertiesTypeAccessor(x));
        }

        public void SetValue(object target, string propertyName, object value)
        {
            Guard.NotNull(target, "target");

            var accessor = FindAccessor(propertyName);

            accessor.Set(target, value);
        }

        public object GetValue(object target, string propertyName)
        {
            Guard.NotNull(target, nameof(target));

            var accessor = FindAccessor(propertyName);

            return accessor.Get(target);
        }

        private IPropertyAccessor FindAccessor(string propertyName)
        {
            Guard.NotNullOrEmpty(propertyName, nameof(propertyName));

            IPropertyAccessor accessor;

            if (!accessors.TryGetValue(propertyName, out accessor))
            {
                throw new ArgumentException("Property does not exist.", nameof(propertyName));
            }

            return accessor;
        }
    }
}