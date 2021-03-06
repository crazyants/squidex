﻿// ==========================================================================
//  StringField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Core.Schemas.Validators;

namespace Squidex.Core.Schemas
{
    public sealed class StringField : Field<StringFieldProperties>
    {
        public StringField(long id, string name, StringFieldProperties properties) 
            : base(id, name, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredStringValidator();
            }

            if (Properties.MinLength.HasValue || Properties.MaxLength.HasValue)
            {
                yield return new StringLengthValidator(Properties.MinLength, Properties.MaxLength);
            }

            if (!string.IsNullOrWhiteSpace(Properties.Pattern))
            {
                yield return new PatternValidator(Properties.Pattern, Properties.PatternMessage);
            }

            if (Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<string>(Properties.AllowedValues.ToArray());
            }
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty)
        {
            jsonProperty.Type = JsonObjectType.String;

            jsonProperty.MinLength = Properties.MinLength;
            jsonProperty.MaxLength = Properties.MaxLength;

            if (Properties.AllowedValues != null)
            {
                jsonProperty.EnumerationNames = new Collection<string>(Properties.AllowedValues);
            }
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, !Properties.IsRequired);
        }

        protected override object ConvertValue(JToken value)
        {
            return value.ToString();
        }
    }
}
