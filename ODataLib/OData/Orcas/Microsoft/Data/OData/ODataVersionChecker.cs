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

namespace Microsoft.Data.OData
{
    #region Namespaces
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Utility class to check feature availability in a certain version of OData.
    /// </summary>
    internal static class ODataVersionChecker
    {
        /// <summary>
        /// Check whether the inline count feature is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckCount(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V2)
            {
                throw new ODataException(Strings.ODataVersionChecker_InlineCountNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Check whether CollectionValue properties are supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        /// <param name="propertyName">The name of the property which holds the collection.</param>
        internal static void CheckCollectionValueProperties(ODataVersion version, string propertyName)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V3)
            {
                throw new ODataException(Strings.ODataVersionChecker_CollectionPropertiesNotSupported(propertyName, ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Check whether CollectionValue is supported in the specified version. 
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckCollectionValue(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V3)
            {
                throw new ODataException(Strings.ODataVersionChecker_CollectionNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Check whether the next link feature is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckNextLink(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V2)
            {
                throw new ODataException(Strings.ODataVersionChecker_NextLinkNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Check whether the named streams feature is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckStreamReferenceProperty(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V3)
            {
                throw new ODataException(Strings.ODataVersionChecker_StreamPropertiesNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Check whether the association links feature is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckAssociationLinks(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V3)
            {
                throw new ODataException(Strings.ODataVersionChecker_AssociationLinksNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Check whether the custom Type Scheme feature is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckCustomTypeScheme(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version > ODataVersion.V2)
            {
                throw new ODataException(Strings.ODataVersionChecker_PropertyNotSupportedForODataVersionGreaterThanX("TypeScheme", ODataUtils.ODataVersionToString(ODataVersion.V2)));
            }
        }

        /// <summary>
        /// Check whether the custom Data Namespace feature is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckCustomDataNamespace(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version > ODataVersion.V2)
            {
                throw new ODataException(Strings.ODataVersionChecker_PropertyNotSupportedForODataVersionGreaterThanX("DataNamespace", ODataUtils.ODataVersionToString(ODataVersion.V2)));
            }
        }

        /// <summary>
        /// Check whether parameters in the payload are supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckParameterPayload(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V3)
            {
                throw new ODataException(Strings.ODataVersionChecker_ParameterPayloadNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Check whether the EPM on the specified entity type is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        /// <param name="entityType">The entity type to check.</param>
        /// <param name="model">The model containing annotations for the entity type.</param>
        internal static void CheckEntityPropertyMapping(ODataVersion version, IEdmEntityType entityType, IEdmModel model)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityType != null, "entityType != null");
            Debug.Assert(model != null, "model != null");

            ODataEntityPropertyMappingCache epmCache = model.GetEpmCache(entityType);
            if (epmCache != null)
            {
                Debug.Assert(epmCache.EpmTargetTree != null, "If the EPM annotation is present the EPM tree must already be initialized.");
                if (version < epmCache.EpmTargetTree.MinimumODataProtocolVersion)
                {
                    throw new ODataException(
                        Strings.ODataVersionChecker_EpmVersionNotSupported(
                            entityType.ODataFullName(),
                            ODataUtils.ODataVersionToString(epmCache.EpmTargetTree.MinimumODataProtocolVersion),
                            ODataUtils.ODataVersionToString(version)));
                }
            }
        }

        /// <summary>
        /// Check whether the spatial value is supported in the specified version.
        /// </summary>
        /// <param name="version">The version to check.</param>
        internal static void CheckSpatialValue(ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();

            if (version < ODataVersion.V3)
            {
                throw new ODataException(Strings.ODataVersionChecker_GeographyAndGeometryNotSupported(ODataUtils.ODataVersionToString(version)));
            }
        }

        /// <summary>
        /// Checks that the version specified on the request or the response is supported by this library.
        /// </summary>
        /// <param name="version">The version to check.</param>
        /// <param name="messageReaderSettings">The message reader settings specified for the reader.</param>
        /// <remarks>In internal drops we currently do not support protocol version 3.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "version", Justification = "Parameter is used if #INTERNAL_DROP is set.")]
        internal static void CheckVersionSupported(ODataVersion version, ODataMessageReaderSettings messageReaderSettings)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(messageReaderSettings != null, "messageReaderSettings != null");

            if (version > messageReaderSettings.MaxProtocolVersion)
            {
                throw new ODataException(Strings.ODataVersionChecker_MaxProtocolVersionExceeded(
                    ODataUtils.ODataVersionToString(version),
                    ODataUtils.ODataVersionToString(messageReaderSettings.MaxProtocolVersion)));
            }

#if DISABLE_V3
            if (version >= ODataVersion.V3)
            {
                throw new ODataException(Strings.ODataVersionChecker_ProtocolVersion3IsNotSupported);
            }
#endif
        }
    }
}
