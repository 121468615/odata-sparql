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

namespace System.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Extensions to Formatters
    /// </summary>
    public static class FormatterExtensions
    {
        /// <summary>
        /// Writes the specified formatter.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="spatial">The spatial object.</param>
        /// <returns>A string value of the formatted object.</returns>
        public static String Write(this SpatialFormatter<TextReader, TextWriter> formatter, ISpatial spatial)
        {
            Util.CheckArgumentNull(formatter, "formatter");

            StringBuilder builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                formatter.Write(spatial, writer);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Writes the specified formatter.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="spatial">The spatial object.</param>
        /// <returns>A string value of the formatted object.</returns>
        public static String Write(this SpatialFormatter<XmlReader, XmlWriter> formatter, ISpatial spatial)
        {
            Util.CheckArgumentNull(formatter, "formatter");

            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                formatter.Write(spatial, writer);
            }

            return builder.ToString();
        }
    }
}
