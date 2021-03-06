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

namespace Microsoft.Data.Edm
{
    /// <summary>
    /// Represents a definition of an EDM enumeration type.
    /// </summary>
    public interface IEdmEnumType : IEdmSchemaType
    {
        /// <summary>
        /// Gets the underlying type of this enumeration type.
        /// </summary>
        IEdmPrimitiveType UnderlyingType { get; }

        /// <summary>
        /// Gets the members of this enumeration type.
        /// </summary>
        IEnumerable<IEdmEnumMember> Members { get; }

        /// <summary>
        /// Gets a value indicating whether the enumeration type can be treated as a bit field.
        /// </summary>
        bool IsFlags { get; }
    }
}
