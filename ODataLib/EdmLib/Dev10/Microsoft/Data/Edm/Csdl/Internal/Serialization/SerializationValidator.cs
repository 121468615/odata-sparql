//   Copyright 2011 Microsoft Corporation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Validation.Internal;

namespace Microsoft.Data.Edm.Csdl.Internal.Serialization
{
    internal static class SerializationValidator
    {
        #region Additional Rules

        /// <summary>
        /// Validates that a type reference refers to a type that can be represented in CSDL.
        /// </summary>
        private static readonly ValidationRule<IEdmTypeReference> TypeReferenceTargetMustHaveValidName =
            new ValidationRule<IEdmTypeReference>(
                (context, typeReference) =>
                {
                    IEdmSchemaType schemaType = typeReference.Definition as IEdmSchemaType;
                    if (schemaType != null)
                    {
                        if (!EdmUtil.IsQualifiedName(schemaType.FullName()))
                        {
                            context.AddError(
                                typeReference.Location(),
                                EdmErrorCode.ReferencedTypeMustHaveValidName,
                                Strings.Serializer_ReferencedTypeMustHaveValidName(schemaType.FullName()));
                        }
                    }
                });

        /// <summary>
        /// Validates that a type reference refers to a type that can be represented in CSDL.
        /// </summary>
        private static readonly ValidationRule<IEdmEntityReferenceType> EntityReferenceTargetMustHaveValidName =
            new ValidationRule<IEdmEntityReferenceType>(
                (context, entityReference) =>
                {
                    if (!EdmUtil.IsQualifiedName(entityReference.EntityType.FullName()))
                    {
                        context.AddError(
                            entityReference.Location(),
                            EdmErrorCode.ReferencedTypeMustHaveValidName,
                            Strings.Serializer_ReferencedTypeMustHaveValidName(entityReference.EntityType.FullName()));
                    }
                });

        /// <summary>
        /// Validates that an entity set refers to a type that can be represented in CSDL.
        /// </summary>
        private static readonly ValidationRule<IEdmEntitySet> EntitySetTypeMustHaveValidName =
            new ValidationRule<IEdmEntitySet>(
                (context, set) =>
                {
                    if (!EdmUtil.IsQualifiedName(set.ElementType.FullName()))
                    {
                        context.AddError(
                            set.Location(),
                            EdmErrorCode.ReferencedTypeMustHaveValidName,
                            Strings.Serializer_ReferencedTypeMustHaveValidName(set.ElementType.FullName()));
                    }
                });

        /// <summary>
        /// Validates that a structured type's base type can be represented in CSDL.
        /// </summary>
        private static readonly ValidationRule<IEdmStructuredType> StructuredTypeBaseTypeMustHaveValidName =
            new ValidationRule<IEdmStructuredType>(
                (context, type) =>
                {
                    IEdmSchemaType schemaBaseType = type.BaseType as IEdmSchemaType;
                    if (schemaBaseType != null)
                    {
                        if (!EdmUtil.IsQualifiedName(schemaBaseType.FullName()))
                        {
                            context.AddError(
                                type.Location(),
                                EdmErrorCode.ReferencedTypeMustHaveValidName,
                                Strings.Serializer_ReferencedTypeMustHaveValidName(schemaBaseType.FullName()));
                        }
                    }
                });

        /// <summary>
        /// Validates that association names can be represented in CSDL.
        /// </summary>
        private static readonly ValidationRule<IEdmNavigationProperty> NavigationPropertyVerifyAssociationName =
            new ValidationRule<IEdmNavigationProperty>(
                (context, property) =>
                {
                    if (!EdmUtil.IsQualifiedName(context.Model.GetAssociationFullName(property)))
                    {
                        context.AddError(
                            property.Location(),
                            EdmErrorCode.ReferencedTypeMustHaveValidName,
                            Strings.Serializer_ReferencedTypeMustHaveValidName(context.Model.GetAssociationFullName(property)));
                    }
                });

        /// <summary>
        /// Validates that vocabulary annotations serialized out of line have a serializable target name.
        /// </summary>
        private static readonly ValidationRule<IEdmVocabularyAnnotation> VocabularyAnnotationOutOfLineMustHaveValidTargetName =
            new ValidationRule<IEdmVocabularyAnnotation>(
                (context, annotation) =>
                {
                    if (annotation.GetSerializationLocation(context.Model) == EdmVocabularyAnnotationSerializationLocation.OutOfLine && !EdmUtil.IsQualifiedName(annotation.TargetString()))
                    {
                        context.AddError(
                            annotation.Location(),
                            EdmErrorCode.InvalidName,
                            Strings.Serializer_OutOfLineAnnotationTargetMustHaveValidName(EdmUtil.FullyQualifiedName(annotation.Target)));
                    }
                });

        /// <summary>
        /// Validates that vocabulary annotations have a serializable term name.
        /// </summary>
        private static readonly ValidationRule<IEdmVocabularyAnnotation> VocabularyAnnotationMustHaveValidTermName =
            new ValidationRule<IEdmVocabularyAnnotation>(
                (context, annotation) =>
                {
                    if (!EdmUtil.IsQualifiedName(annotation.Term.FullName()))
                    {
                        context.AddError(
                            annotation.Location(),
                            EdmErrorCode.InvalidName,
                            Strings.Serializer_OutOfLineAnnotationTargetMustHaveValidName(annotation.Term.FullName()));
                    }
                });

        #endregion

        private static ValidationRuleSet serializationRuleSet = new ValidationRuleSet(new ValidationRule[]
            {
                TypeReferenceTargetMustHaveValidName,
                EntityReferenceTargetMustHaveValidName,
                EntitySetTypeMustHaveValidName,
                StructuredTypeBaseTypeMustHaveValidName,
                VocabularyAnnotationOutOfLineMustHaveValidTargetName,
                VocabularyAnnotationMustHaveValidTermName,
                NavigationPropertyVerifyAssociationName,
                ValidationRules.FunctionImportEntitySetExpressionIsInvalid,
                ValidationRules.FunctionImportParametersCannotHaveModeOfNone,
                ValidationRules.FunctionOnlyInputParametersAllowedInFunctions,
                ValidationRules.TypeMustNotHaveKindOfNone,
                ValidationRules.PrimitiveTypeMustNotHaveKindOfNone,
                ValidationRules.PropertyMustNotHaveKindOfNone,
                ValidationRules.TermMustNotHaveKindOfNone,
                ValidationRules.SchemaElementMustNotHaveKindOfNone,
                ValidationRules.EntityContainerElementMustNotHaveKindOfNone,
                ValidationRules.EnumMustHaveIntegerUnderlyingType,
                ValidationRules.EnumMemberValueMustHaveSameTypeAsUnderlyingType
            });

        public static IEnumerable<EdmError> GetSerializationErrors(this IEdmModel root)
        {
            IEnumerable<EdmError> errors;
            root.Validate(serializationRuleSet, out errors);
            errors = errors.Where(SignificantToSerialization);
            return errors;
        }

        internal static bool SignificantToSerialization(EdmError error)
        {
            if (ValidationHelper.IsInterfaceCritical(error))
            {
                return true;
            }

            switch (error.ErrorCode)
            {
                case EdmErrorCode.InvalidName:
                case EdmErrorCode.NameTooLong:
                case EdmErrorCode.InvalidNamespaceName:
                case EdmErrorCode.SystemNamespaceEncountered:
                case EdmErrorCode.RowTypeMustNotHaveBaseType:
                case EdmErrorCode.ReferencedTypeMustHaveValidName:
                case EdmErrorCode.FunctionImportEntitySetExpressionIsInvalid:
                case EdmErrorCode.FunctionImportParameterIncorrectType:
                case EdmErrorCode.OnlyInputParametersAllowedInFunctions:
                case EdmErrorCode.InvalidFunctionImportParameterMode:
                case EdmErrorCode.TypeMustNotHaveKindOfNone:
                case EdmErrorCode.PrimitiveTypeMustNotHaveKindOfNone:
                case EdmErrorCode.PropertyMustNotHaveKindOfNone:
                case EdmErrorCode.TermMustNotHaveKindOfNone:
                case EdmErrorCode.SchemaElementMustNotHaveKindOfNone:
                case EdmErrorCode.EntityContainerElementMustNotHaveKindOfNone:
                case EdmErrorCode.BinaryValueCannotHaveEmptyValue:
                case EdmErrorCode.EnumMustHaveIntegerUnderlyingType:
                case EdmErrorCode.EnumMemberTypeMustMatchEnumUnderlyingType:
                    return true;
            }

            return false;
        }
    }
}
