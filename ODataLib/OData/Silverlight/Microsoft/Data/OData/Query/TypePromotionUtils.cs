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

namespace Microsoft.Data.OData.Query
{
    #region Namespaces
#if ODATALIB_QUERY
    using System.Collections.Generic;
#endif
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if ODATALIB_QUERY
    using System.Linq;
#endif
    using Microsoft.Data.Edm;
#if ODATALIB_QUERY
    using Microsoft.Data.Edm.Library;
#endif
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Helper methods for promoting argument types of operators and function calls.
    /// </summary>
    internal static class TypePromotionUtils
    {
        #if ODATALIB_QUERY
        /// <summary>Function signatures for logical operators (and, or).</summary>
        private static readonly FunctionSignature[] logicalSignatures = new FunctionSignature[]
        {
            new FunctionSignature(EdmCoreModel.Instance.GetBoolean(false), EdmCoreModel.Instance.GetBoolean(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetBoolean(true), EdmCoreModel.Instance.GetBoolean(true)),
        };

        /// <summary>Function signatures for the 'not' operator.</summary>
        private static readonly FunctionSignature[] notSignatures = new FunctionSignature[]
        {
            new FunctionSignature(EdmCoreModel.Instance.GetBoolean(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetBoolean(true)),
        };

        /// <summary>Function signatures for arithmetic operators (add, sub, mul, div, mod).</summary>
        private static readonly FunctionSignature[] arithmeticSignatures = new FunctionSignature[]
        {
            new FunctionSignature(EdmCoreModel.Instance.GetInt32(false), EdmCoreModel.Instance.GetInt32(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt32(true), EdmCoreModel.Instance.GetInt32(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt64(false), EdmCoreModel.Instance.GetInt64(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt64(true), EdmCoreModel.Instance.GetInt64(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetSingle(false), EdmCoreModel.Instance.GetSingle(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetSingle(true), EdmCoreModel.Instance.GetSingle(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetDouble(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetDouble(true), EdmCoreModel.Instance.GetDouble(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetDecimal(false), EdmCoreModel.Instance.GetDecimal(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetDecimal(true), EdmCoreModel.Instance.GetDecimal(true)),
        };

        /// <summary>Function signatures for relational operators (eq, ne, lt, le, gt, ge).</summary>
        private static readonly FunctionSignature[] relationalSignatures = new FunctionSignature[]
        {
            new FunctionSignature(EdmCoreModel.Instance.GetInt32(false), EdmCoreModel.Instance.GetInt32(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt32(true), EdmCoreModel.Instance.GetInt32(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt64(false), EdmCoreModel.Instance.GetInt64(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt64(true), EdmCoreModel.Instance.GetInt64(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetSingle(false), EdmCoreModel.Instance.GetSingle(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetSingle(true), EdmCoreModel.Instance.GetSingle(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetDouble(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetDouble(true), EdmCoreModel.Instance.GetDouble(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetDecimal(false), EdmCoreModel.Instance.GetDecimal(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetDecimal(true), EdmCoreModel.Instance.GetDecimal(true)),

            new FunctionSignature(EdmCoreModel.Instance.GetString(true), EdmCoreModel.Instance.GetString(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetBinary(true), EdmCoreModel.Instance.GetBinary(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetBoolean(false), EdmCoreModel.Instance.GetBoolean(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetBoolean(true), EdmCoreModel.Instance.GetBoolean(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetGuid(false), EdmCoreModel.Instance.GetGuid(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetGuid(true), EdmCoreModel.Instance.GetGuid(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTime, false), EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTime, false)),
            new FunctionSignature(EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTime, true), EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTime, true)),
        };

        /// <summary>Function signatures for the 'negate' operator.</summary>
        private static readonly FunctionSignature[] negationSignatures = new FunctionSignature[]
        {
            new FunctionSignature(EdmCoreModel.Instance.GetInt32(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt32(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt64(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetInt64(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetSingle(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetSingle(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetDouble(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetDouble(true)),
            new FunctionSignature(EdmCoreModel.Instance.GetDecimal(false)),
            new FunctionSignature(EdmCoreModel.Instance.GetDecimal(true)),
        };

        /// <summary>Numeric type kinds.</summary>
        private enum NumericTypeKind
        {
            /// <summary>A type that is not numeric.</summary>
            NotNumeric = 0,

            /// <summary>A type that is a char, single, double or decimal.</summary>
            NotIntegral = 1,

            /// <summary>A type that is a signed integral.</summary>
            SignedIntegral = 2,

            /// <summary>A type that is an unsigned integral.</summary>
            UnsignedIntegral = 3,
        }

        /// <summary>Checks that the operands (possibly promoted) are valid for the specified operation.</summary>
        /// <param name="operatorKind">The operator kind to promote the operand types for.</param>
        /// <param name="left">Type reference of left operand.</param>
        /// <param name="right">Type reference of right operand.</param>
        /// <returns>True if all argument types could be promoted; otherwise false.</returns>
        internal static bool PromoteOperandTypes(BinaryOperatorKind operatorKind, ref IEdmTypeReference left, ref IEdmTypeReference right)
        {
            DebugUtils.CheckNoExternalCallers();

            // The types for the operands can be null
            // if they (a) represent the null literal or (b) represent an open type/property.
            // TODO: We currently do not support open properties/types so if both argument types are null 
            //       we have null literals on both sides and cannot promote arguments;
            //       Review when we support open properties/types.
            if (left == null && right == null)
            {
                // if we find null literals on both sides we cannot promote; the result type will also be null
                return true;
            }

            FunctionSignature[] signatures = GetFunctionSignatures(operatorKind);

            IEdmTypeReference[] argumentTypes = new IEdmTypeReference[] { left, right };
            bool success = FindBestSignature(signatures, argumentTypes) == 1;

            if (success)
            {
                left = argumentTypes[0];
                right = argumentTypes[1];

                if (left == null)
                {
                    left = right;
                }
                else if (right == null)
                {
                    right = left;
                }
            }

            return success;
        }

        /// <summary>Checks that the operands (possibly promoted) are valid for the specified operation.</summary>
        /// <param name="operatorKind">The operator kind to promote the operand types for.</param>
        /// <param name="typeReference">Type of the operand.</param>
        /// <returns>True if the type could be promoted; otherwise false.</returns>
        internal static bool PromoteOperandType(UnaryOperatorKind operatorKind, ref IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();

            // The type for the operands can be null
            // if it (a) represents the null literal or (b) represents an open type/property.
            // TODO: We currently do not support open properties/types so if argument type is null 
            //       we have a null literal and cannot promote the argument type;
            //       Review when we support open properties/types.
            if (typeReference == null)
            {
                // if we find a null literal we cannot promote; the result type will also be null
                return true;
            }

            FunctionSignature[] signatures = GetFunctionSignatures(operatorKind);

            IEdmTypeReference[] argumentTypes = new IEdmTypeReference[] { typeReference };
            bool success = FindBestSignature(signatures, argumentTypes) == 1;

            if (success)
            {
                typeReference = argumentTypes[0];
            }

            return success;
        }

        /// <summary>Finds the best fitting function for the specified arguments.</summary>
        /// <param name="functions">Functions to consider.</param>
        /// <param name="argumentTypes">Types of the arguments for the function.</param>
        /// <returns>The best fitting function; null if none found or ambiguous.</returns>
        internal static FunctionSignature FindBestFunctionSignature(FunctionSignature[] functions, IEdmTypeReference[] argumentTypes)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(functions != null, "functions != null");
            Debug.Assert(argumentTypes != null, "argumentTypes != null");
            List<FunctionSignature> applicableFunctions = new List<FunctionSignature>(functions.Length);

            // Build a list of applicable functions (and cache their promoted arguments).
            foreach (FunctionSignature candidate in functions)
            {
                if (candidate.ArgumentTypes.Length != argumentTypes.Length)
                {
                    continue;
                }

                bool argumentsMatch = true;
                for (int i = 0; i < candidate.ArgumentTypes.Length; i++)
                {
                    if (!CanPromoteTo(argumentTypes[i], candidate.ArgumentTypes[i]))
                    {
                        argumentsMatch = false;
                        break;
                    }
                }

                if (argumentsMatch)
                {
                    applicableFunctions.Add(candidate);
                }
            }

            // Return the best applicable function.
            if (applicableFunctions.Count == 0)
            {
                // No matching function.
                return null;
            }
            else if (applicableFunctions.Count == 1)
            {
                return applicableFunctions[0];
            }
            else
            {
                // Find a single function which is better than all others.
                int bestFunctionIndex = -1;
                for (int i = 0; i < applicableFunctions.Count; i++)
                {
                    bool betterThanAllOthers = true;
                    for (int j = 0; j < applicableFunctions.Count; j++)
                    {
                        if (i != j && MatchesArgumentTypesBetterThan(argumentTypes, applicableFunctions[j].ArgumentTypes, applicableFunctions[i].ArgumentTypes))
                        {
                            betterThanAllOthers = false;
                            break;
                        }
                    }

                    if (betterThanAllOthers)
                    {
                        if (bestFunctionIndex == -1)
                        {
                            bestFunctionIndex = i;
                        }
                        else
                        {
                            // Ambiguous.
                            return null;
                        }
                    }
                }

                if (bestFunctionIndex == -1)
                {
                    return null;
                }

                return applicableFunctions[bestFunctionIndex];
            }
        }

        /// <summary>Finds the exact fitting function for the specified arguments.</summary>
        /// <param name="functions">Functions to consider.</param>
        /// <param name="argumentTypes">Types of the arguments for the function.</param>
        /// <returns>The exact fitting function; null if no exact match was found.</returns>
        internal static FunctionSignature FindExactFunctionSignature(FunctionSignature[] functions, IEdmTypeReference[] argumentTypes)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(functions != null, "functions != null");
            Debug.Assert(argumentTypes != null, "argumentTypes != null");

            for (int functionIndex = 0; functionIndex < functions.Length; functionIndex++)
            {
                FunctionSignature functionSignature = functions[functionIndex];
                bool matchFound = true;

                if (functionSignature.ArgumentTypes.Length != argumentTypes.Length)
                {
                    continue;
                }

                for (int argumentIndex = 0; argumentIndex < argumentTypes.Length; argumentIndex++)
                {
                    IEdmTypeReference functionSignatureArgumentType = functionSignature.ArgumentTypes[argumentIndex];
                    IEdmTypeReference argumentType = argumentTypes[argumentIndex];
                    Debug.Assert(functionSignatureArgumentType.IsODataPrimitiveTypeKind(), "Only primitive arguments are supported for functions.");

                    if (!argumentType.IsODataPrimitiveTypeKind())
                    {
                        matchFound = false;
                        break;
                    }

                    if (!argumentType.IsEquivalentTo(functionSignatureArgumentType))
                    {
                        matchFound = false;
                        break;
                    }
                }

                if (matchFound)
                {
                    return functionSignature;
                }
            }

            return null;
        }

#endif
        /// <summary>Checks whether the source type is compatible with the target type.</summary>
        /// <param name="sourceReference">Source type.</param>
        /// <param name="targetReference">Target type.</param>
        /// <returns>true if source can be used in place of target; false otherwise.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "One method to describe all rules around converts.")]
        internal static bool CanConvertTo(IEdmTypeReference sourceReference, IEdmTypeReference targetReference)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(sourceReference != null, "sourceReference != null");
            Debug.Assert(targetReference != null, "targetReference != null");

            //// Copy of the ResourceQueryParser.ExpressionParser.IsCompatibleWith method.

            if (sourceReference.IsEquivalentTo(targetReference))
            {
                return true;
            }

            if (targetReference.IsODataComplexTypeKind() || targetReference.IsODataEntityTypeKind())
            {
                // for structured types, use IsAssignableFrom
                return EdmLibraryExtensions.IsAssignableFrom(
                    (IEdmStructuredType)targetReference.Definition,
                    (IEdmStructuredType)sourceReference.Definition);
            }

            // NOTE: this is to deal with open types that are not yet supported.
            if (IsOpenPropertyType(targetReference))
            {
                return true;
            }

            //// This rule stops the parser from considering nullable types as incompatible
            //// with non-nullable types. We have however implemented this rule because we just
            //// access underlying rules. C# requires an explicit .Value access, and EDM has no
            //// nullablity on types and (at the model level) implements null propagation.
            ////
            //// if (sourceReference.IsNullable && !targetReference.IsNullable)
            //// {
            ////     return false;
            //// }

            IEdmPrimitiveTypeReference sourcePrimitiveTypeReference = sourceReference.AsPrimitiveOrNull();
            IEdmPrimitiveTypeReference targetPrimitiveTypeReference = targetReference.AsPrimitiveOrNull();

            if (sourcePrimitiveTypeReference == null || targetPrimitiveTypeReference == null)
            {
                return false;
            }

            return MetadataUtilsCommon.CanConvertPrimitiveTypeTo(sourcePrimitiveTypeReference.PrimitiveDefinition(), targetPrimitiveTypeReference.PrimitiveDefinition());
        }

#if ODATALIB_QUERY
        /// <summary>
        /// Gets the correct set of function signatures for type promotion for a given binary operator.
        /// </summary>
        /// <param name="operatorKind">The operator kind to get the signatures for.</param>
        /// <returns>The set of signatures for the specified <paramref name="operatorKind"/>.</returns>
        private static FunctionSignature[] GetFunctionSignatures(BinaryOperatorKind operatorKind)
        {
            switch (operatorKind)
            {
                case BinaryOperatorKind.Or:     // fall through
                case BinaryOperatorKind.And:
                    return logicalSignatures;

                case BinaryOperatorKind.Equal:              // fall through
                case BinaryOperatorKind.NotEqual:           // fall through
                case BinaryOperatorKind.GreaterThan:        // fall through
                case BinaryOperatorKind.GreaterThanOrEqual: // fall through
                case BinaryOperatorKind.LessThan:           // fall through
                case BinaryOperatorKind.LessThanOrEqual:
                    return relationalSignatures;

                case BinaryOperatorKind.Add:                // fall through
                case BinaryOperatorKind.Subtract:           // fall through
                case BinaryOperatorKind.Multiply:           // fall through
                case BinaryOperatorKind.Divide:             // fall through
                case BinaryOperatorKind.Modulo:             // fall through
                    return arithmeticSignatures;

                default:
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.TypePromotionUtils_GetFunctionSignatures_Binary_UnreachableCodepath));
            }
        }

        /// <summary>
        /// Gets the correct set of function signatures for type promotion for a given binary operator.
        /// </summary>
        /// <param name="operatorKind">The operator kind to get the signatures for.</param>
        /// <returns>The set of signatures for the specified <paramref name="operatorKind"/>.</returns>
        private static FunctionSignature[] GetFunctionSignatures(UnaryOperatorKind operatorKind)
        {
            switch (operatorKind)
            {
                case UnaryOperatorKind.Negate:
                    return negationSignatures;

                case UnaryOperatorKind.Not:
                    return notSignatures;
                default:
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.TypePromotionUtils_GetFunctionSignatures_Unary_UnreachableCodepath));
            }
        }

        /// <summary>Finds the best methods for the specified arguments given a candidate method enumeration.</summary>
        /// <param name="signatures">The candidate function signatures.</param>
        /// <param name="argumentTypes">The argument type references to match.</param>
        /// <returns>The number of "best match" methods.</returns>
        private static int FindBestSignature(FunctionSignature[] signatures, IEdmTypeReference[] argumentTypes)
        {
            Debug.Assert(signatures != null, "signatures != null");
            Debug.Assert(argumentTypes != null, "argumentTypes != null");
            Debug.Assert(argumentTypes.All(t => t == null || t.IsODataPrimitiveTypeKind()), "All argument types must be primitive or null.");
            Debug.Assert(signatures.All(s => s.ArgumentTypes != null && s.ArgumentTypes.All(t => t.IsODataPrimitiveTypeKind())), "All signatures must have only primitive argument types.");

            List<FunctionSignature> applicableSignatures = signatures.Where(signature => IsApplicable(signature, argumentTypes)).ToList();
            if (applicableSignatures.Count > 1)
            {
                applicableSignatures = FindBestApplicableSignatures(applicableSignatures, argumentTypes);
            }

            int result = applicableSignatures.Count;
            if (result == 1)
            {
                // TODO: deal with the situation that we started off with all non-open types
                //       and end up with all open types; see RequestQueryParser.FindBestMethod. 
                //       Ignored for now since we don't support open types yet.
                FunctionSignature signature = applicableSignatures[0];
                for (int i = 0; i < argumentTypes.Length; i++)
                {
                    argumentTypes[i] = signature.ArgumentTypes[i];
                }

                result = 1;
            }
            else if (result > 1)
            {
                // We may have the case for operators (which C# doesn't) in which we have a nullable operand
                // and a non-nullable operand. We choose to convert the one non-null operand to nullable in that
                // case (the binary expression will lift to null).
                if (argumentTypes.Length == 2 && result == 2)
                {
                    if (applicableSignatures[0].ArgumentTypes[0].Definition.IsEquivalentTo(applicableSignatures[1].ArgumentTypes[0].Definition))
                    {
                        FunctionSignature nullableMethod =
                            applicableSignatures[0].ArgumentTypes[0].IsNullable ?
                            applicableSignatures[0] :
                            applicableSignatures[1];
                        argumentTypes[0] = nullableMethod.ArgumentTypes[0];
                        argumentTypes[1] = nullableMethod.ArgumentTypes[1];

                        // TODO: why is this necessary? We keep it here for now since the product has it but assert
                        //       that nothing new was found.
                        int signatureCount = FindBestSignature(signatures, argumentTypes);
                        Debug.Assert(signatureCount == 1, "signatureCount == 1");
                        Debug.Assert(argumentTypes[0] == nullableMethod.ArgumentTypes[0], "argumentTypes[0] == nullableMethod.ArgumentTypes[0]");
                        Debug.Assert(argumentTypes[1] == nullableMethod.ArgumentTypes[1], "argumentTypes[1] == nullableMethod.ArgumentTypes[1]");
                        return signatureCount;
                    }
                }
            }

            return result;
        }

        /// <summary>Checks whether the specified method is applicable given the argument expressions.</summary>
        /// <param name="signature">The candidate function signature to check.</param>
        /// <param name="argumentTypes">The argument types to match.</param>
        /// <returns>An applicable function signature if all argument types can be promoted; 'null' otherwise.</returns>
        private static bool IsApplicable(FunctionSignature signature, IEdmTypeReference[] argumentTypes)
        {
            Debug.Assert(signature != null, "signature != null");
            Debug.Assert(argumentTypes != null, "argumentTypes != null");

            if (signature.ArgumentTypes.Length != argumentTypes.Length)
            {
                return false;
            }

            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                if (!CanPromoteTo(argumentTypes[i], signature.ArgumentTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Promotes the specified expression to the given type if necessary.</summary>
        /// <param name="sourceType">The actual argument type.</param>
        /// <param name="targetType">The required type to promote to.</param>
        /// <returns>True if the <paramref name="sourceType"/> could be promoted; otherwise false.</returns>
        private static bool CanPromoteTo(IEdmTypeReference sourceType, IEdmTypeReference targetType)
        {
            Debug.Assert(targetType != null, "targetType != null");
            Debug.Assert(sourceType == null || sourceType.IsODataPrimitiveTypeKind(), "Type promotion only supported for primitive types.");
            Debug.Assert(targetType.IsODataPrimitiveTypeKind(), "Type promotion only supported for primitive types.");

            if (sourceType == null)
            {
                // This indicates that a null literal or an open type has been specified.
                // For null literals we can promote to the required target type if it is nullable
                // TODO: review this once open types are supported.
                return targetType.IsNullable;
            }

            if (sourceType.IsEquivalentTo(targetType))
            {
                return true;
            }

            if (CanConvertTo(sourceType, targetType))
            {
                return true;
            }

            // Allow promotion from nullable to non-nullable by directly accessing underlying value.
            if (sourceType.IsNullable && targetType.IsODataValueType())
            {
                IEdmTypeReference nonNullableSourceType = sourceType.Definition.ToTypeReference(false);
                if (CanConvertTo(nonNullableSourceType, targetType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Finds the best applicable methods from the specified array that match the arguments.</summary>
        /// <param name="signatures">The candidate function signatures.</param>
        /// <param name="argumentTypes">The argument types to match.</param>
        /// <returns>Best applicable methods.</returns>
        private static List<FunctionSignature> FindBestApplicableSignatures(List<FunctionSignature> signatures, IEdmTypeReference[] argumentTypes)
        {
            Debug.Assert(signatures != null, "signatures != null");

            List<FunctionSignature> result = new List<FunctionSignature>();
            foreach (FunctionSignature method in signatures)
            {
                bool betterThanAllOthers = true;
                foreach (FunctionSignature otherMethod in signatures)
                {
                    if (otherMethod != method && MatchesArgumentTypesBetterThan(argumentTypes, otherMethod.ArgumentTypes, method.ArgumentTypes))
                    {
                        betterThanAllOthers = false;
                        break;
                    }
                }

                if (betterThanAllOthers)
                {
                    result.Add(method);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether the <paramref name="firstCandidate"/> type list has better argument matching against the <paramref name="argumentTypes"/>
        /// than the <paramref name="secondCandidate"/> type list.
        /// </summary>
        /// <param name="argumentTypes">Actual arguments types.</param>
        /// <param name="firstCandidate">First type list to check.</param>
        /// <param name="secondCandidate">Second type list to check.</param>
        /// <returns>
        /// True if <paramref name="firstCandidate"/> has better parameter matching than <paramref name="secondCandidate"/>; otherwise false.
        /// </returns>
        private static bool MatchesArgumentTypesBetterThan(IEdmTypeReference[] argumentTypes, IEdmTypeReference[] firstCandidate, IEdmTypeReference[] secondCandidate)
        {
            Debug.Assert(argumentTypes != null, "argumentTypes != null");
            Debug.Assert(firstCandidate != null, "firstCandidate != null");
            Debug.Assert(argumentTypes.Length == firstCandidate.Length, "argumentTypes.Length == firstCandidate.Length");
            Debug.Assert(secondCandidate != null, "secondCandidate != null");
            Debug.Assert(argumentTypes.Length == secondCandidate.Length, "argumentTypes.Length == secondCandidate.Length");

            bool better = false;

            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                if (argumentTypes[i] == null)
                {
                    // we don't support typed nulls; instead we have no argument type for null literals.
                    // since null literals can be converted to any type don't include them in the comparison
                    // TODO: revisit this once we support open types (where me might have null types for open properties as well)
                    continue;
                }

                int c = CompareConversions(argumentTypes[i], firstCandidate[i], secondCandidate[i]);
                if (c < 0)
                {
                    return false;
                }
                else if (c > 0)
                {
                    better = true;
                }
            }

            return better;
        }

        /// <summary>Checks which conversion is better.</summary>
        /// <param name="source">Source type.</param>
        /// <param name="targetA">First candidate type to convert to.</param>
        /// <param name="targetB">Second candidate type to convert to.</param>
        /// <returns>
        /// Return 1 if s -> t1 is a better conversion than s -> t2
        /// Return -1 if s -> t2 is a better conversion than s -> t1
        /// Return 0 if neither conversion is better
        /// </returns>
        private static int CompareConversions(IEdmTypeReference source, IEdmTypeReference targetA, IEdmTypeReference targetB)
        {
            // If both types are exactly the same, there is no preference.
            if (targetA.IsEquivalentTo(targetB))
            {
                return 0;
            }

            // Look for exact matches.
            if (source.IsEquivalentTo(targetA))
            {
                return 1;
            }

            if (source.IsEquivalentTo(targetB))
            {
                return -1;
            }

            // If one is compatible and the other is not, choose the compatible type.
            bool compatibleT1AndT2 = CanConvertTo(targetA, targetB);
            bool compatibleT2AndT1 = CanConvertTo(targetB, targetA);
            if (compatibleT1AndT2 && !compatibleT2AndT1)
            {
                return 1;
            }

            if (compatibleT2AndT1 && !compatibleT1AndT2)
            {
                return -1;
            }

            // Prefer to keep the original nullability.
            bool sourceNullable = source.IsNullable;
            bool typeNullableA = targetA.IsNullable;
            bool typeNullableB = targetB.IsNullable;
            if (sourceNullable == typeNullableA && sourceNullable != typeNullableB)
            {
                return 1;
            }

            if (sourceNullable != typeNullableA && sourceNullable == typeNullableB)
            {
                return -1;
            }

            // Prefer signed to unsigned.
            if (IsSignedIntegralType(targetA) && IsUnsignedIntegralType(targetB))
            {
                return 1;
            }

            if (IsSignedIntegralType(targetB) && IsUnsignedIntegralType(targetA))
            {
                return -1;
            }

            // If both decimal and double are possible or if both decimal and single are possible, then prefer decimal
            // since double and single should only be targets for single and double source if decimal is available.
            // And since neither single not double convert to decimal, we don't need to handle that case here.
            if (IsDecimalType(targetA) && IsDoubleOrSingle(targetB))
            {
                return 1;
            }

            if (IsDecimalType(targetB) && IsDoubleOrSingle(targetA))
            {
                return -1;
            }

            // Prefer non-object to object.
            // TODO: this is how the product treats open properties (as object-typed). Left here for now
            //       but likely to have to change when we start supporting open properties (since our open
            //       properties are not object-typed).
            if (!IsOpenPropertyType(targetA) && IsOpenPropertyType(targetB))
            {
                return 1;
            }

            if (!IsOpenPropertyType(targetB) && IsOpenPropertyType(targetA))
            {
                return -1;
            }

            return 0;
        }

        /// <summary>Checks whether the specified type is a signed integral type.</summary>
        /// <param name="typeReference">Type reference to check.</param>
        /// <returns>true if <paramref name="typeReference"/> is a signed integral type; false otherwise.</returns>
        private static bool IsSignedIntegralType(IEdmTypeReference typeReference)
        {
            return GetNumericTypeKind(typeReference) == NumericTypeKind.SignedIntegral;
        }

        /// <summary>Checks whether the specified type is an unsigned integral type.</summary>
        /// <param name="typeReference">Type to check.</param>
        /// <returns>true if <paramref name="typeReference"/> is an unsigned integral type; false otherwise.</returns>
        private static bool IsUnsignedIntegralType(IEdmTypeReference typeReference)
        {
            return GetNumericTypeKind(typeReference) == NumericTypeKind.UnsignedIntegral;
        }

        /// <summary>Checks if the specified type is a decimal or nullable decimal type.</summary>
        /// <param name="typeReference">Type to check.</param>
        /// <returns>true if <paramref name="typeReference"/> is either decimal or nullable decimal type; false otherwise.</returns>
        private static bool IsDecimalType(IEdmTypeReference typeReference)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = typeReference.AsPrimitiveOrNull();
            if (primitiveTypeReference != null)
            {
                return primitiveTypeReference.PrimitiveKind() == EdmPrimitiveTypeKind.Decimal;
            }

            return false;
        }

#endif
        /// <summary>Checks if the specified type reference is an open type.</summary>
        /// <param name="typeReference">Type to check.</param>
        /// <returns>true if <paramref name="typeReference"/> is an open type; false otherwise.</returns>
        private static bool IsOpenPropertyType(IEdmTypeReference typeReference)
        {
            // TODO: the product treats open properties as object-typed (System.Object). Using TypeKind.None for now
            //       but likely to have to change when we start supporting open properties.
            IEdmPrimitiveTypeReference primitiveTypeReference = typeReference.AsPrimitiveOrNull();
            if (primitiveTypeReference != null)
            {
                return primitiveTypeReference.PrimitiveKind() == EdmPrimitiveTypeKind.None;
            }

            return false;
        }

#if ODATALIB_QUERY
        /// <summary>Checks if the specified type is either double or single or the nullable variants.</summary>
        /// <param name="typeReference">Type to check.</param>
        /// <returns>true if <paramref name="typeReference"/> is double, single or nullable double or single; false otherwise.</returns>
        private static bool IsDoubleOrSingle(IEdmTypeReference typeReference)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = typeReference.AsPrimitiveOrNull();
            if (primitiveTypeReference != null)
            {
                EdmPrimitiveTypeKind primitiveTypeKind = primitiveTypeReference.PrimitiveKind();
                return primitiveTypeKind == EdmPrimitiveTypeKind.Double || primitiveTypeKind == EdmPrimitiveTypeKind.Single;
            }

            return false;
        }

        /// <summary>Gets a flag for the numeric kind of type.</summary>
        /// <param name="typeReference">Type to get numeric kind for.</param>
        /// <returns>The <see cref="NumericTypeKind"/> of the <paramref name="typeReference"/> argument.</returns>
        private static NumericTypeKind GetNumericTypeKind(IEdmTypeReference typeReference)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = typeReference.AsPrimitiveOrNull();

            if (primitiveTypeReference == null)
            {
                return NumericTypeKind.NotNumeric;
            }

            switch (primitiveTypeReference.PrimitiveDefinition().PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.Single:
                case EdmPrimitiveTypeKind.Decimal:
                case EdmPrimitiveTypeKind.Double:
                    return NumericTypeKind.NotIntegral;

                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                case EdmPrimitiveTypeKind.Int64:
                case EdmPrimitiveTypeKind.SByte:
                    return NumericTypeKind.SignedIntegral;

                case EdmPrimitiveTypeKind.Byte:
                    return NumericTypeKind.UnsignedIntegral;
                    
                default:
                    return NumericTypeKind.NotNumeric;
            }
        }
#endif
    }
}
