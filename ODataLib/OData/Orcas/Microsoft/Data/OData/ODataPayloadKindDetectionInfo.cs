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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Microsoft.Data.Edm;
    #endregion Namespaces

    /// <summary>
    /// Represents the set of information available for payload kind detection.
    /// </summary>
    /// <remarks>This class is used to represent the input to run payload kind detection using
    /// <see cref="ODataMessageReader.DetectPayloadKind"/>. See the documentation of that method for more 
    /// information.</remarks>
    internal sealed class ODataPayloadKindDetectionInfo
    {
        /// <summary>The parsed content type as <see cref="MediaType"/>.</summary>
        private readonly MediaType contentType;

        /// <summary>The encoding specified in the charset parameter of contentType or the default encoding from MediaType.</summary>
        private readonly Encoding encoding;

        /// <summary>The <see cref="ODataMessageReaderSettings"/> being used for reading the message.</summary>
        private readonly ODataMessageReaderSettings messageReaderSettings;

        /// <summary>The <see cref="IEdmModel"/> for the payload.</summary>
        private readonly IEdmModel model;

        /// <summary>The possible payload kinds based on content type negotiation.</summary>
        private readonly IEnumerable<ODataPayloadKind> possiblePayloadKinds;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contentType">The parsed content type as <see cref="MediaType"/>.</param>
        /// <param name="encoding">The encoding from the content type or the default encoding from <see cref="MediaType" />.</param>
        /// <param name="messageReaderSettings">The <see cref="ODataMessageReaderSettings"/> being used for reading the message.</param>
        /// <param name="model">The <see cref="IEdmModel"/> for the payload.</param>
        /// <param name="possiblePayloadKinds">The possible payload kinds based on content type negotiation.</param>
        internal ODataPayloadKindDetectionInfo(
            MediaType contentType,
            Encoding encoding,
            ODataMessageReaderSettings messageReaderSettings, 
            IEdmModel model, 
            IEnumerable<ODataPayloadKind> possiblePayloadKinds)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(contentType, "contentType");
            ExceptionUtils.CheckArgumentNotNull(messageReaderSettings, "readerSettings");
            ExceptionUtils.CheckArgumentNotNull(possiblePayloadKinds, "possiblePayloadKinds");

            this.contentType = contentType;
            this.encoding = encoding;
            this.messageReaderSettings = messageReaderSettings;
            this.model = model;
            this.possiblePayloadKinds = possiblePayloadKinds;
        }

        /// <summary>
        /// The <see cref="ODataMessageReaderSettings"/> being used for reading the message.
        /// </summary>
        public ODataMessageReaderSettings MessageReaderSettings
        {
            get { return this.messageReaderSettings; }
        }

        /// <summary>
        /// The <see cref="IEdmModel"/> for the payload.
        /// </summary>
        public IEdmModel Model
        {
            get { return this.model; }
        }

        /// <summary>
        /// The possible payload kinds based on content type negotiation.
        /// </summary>
        public IEnumerable<ODataPayloadKind> PossiblePayloadKinds
        {
            get { return this.possiblePayloadKinds; }
        }

        /// <summary>
        /// The <see cref="ODataMessageReaderSettings"/> being used for reading the message.
        /// </summary>
        internal MediaType ContentType
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.contentType;
            }
        }

        /// <summary>
        /// The encoding derived from the content type or the default encoding.
        /// </summary>
        /// <returns>The encoding derived from the content type or the default encoding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "There is computation needed to get the encoding from the content type; thus a method.")]
        public Encoding GetEncoding()
        {
            return this.encoding ?? this.contentType.SelectEncoding();
        }
    }
}
