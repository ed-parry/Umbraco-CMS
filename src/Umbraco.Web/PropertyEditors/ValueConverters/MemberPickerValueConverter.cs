﻿using System;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MemberPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MemberPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.MemberPickerAlias)
                || propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.MemberPicker2Alias);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (IPublishedContent);

        public override object ConvertSourceToInter(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
                return attemptConvertInt.Result;
            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        public override object ConvertInterToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
                return null;

            if (UmbracoContext.Current != null)
            {
                IPublishedContent member;
                if (source is int id)
                {
                    member = _publishedSnapshotAccessor.PublishedSnapshot.MemberCache.GetById(id);
                    if (member != null)
                        return member;
                }
                else
                {
                    var sourceUdi = source as GuidUdi;
                    if (sourceUdi == null) return null;
                    member = _publishedSnapshotAccessor.PublishedSnapshot.MemberCache.GetByProviderKey(sourceUdi.Guid);
                    if (member != null)
                        return member;
                }
            }

            return source;
        }
    }
}
