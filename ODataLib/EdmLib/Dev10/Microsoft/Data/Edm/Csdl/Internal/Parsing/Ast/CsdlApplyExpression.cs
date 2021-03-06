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

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
    internal class CsdlApplyExpression : CsdlExpressionBase
    {
        private readonly string function;
        private readonly List<CsdlExpressionBase> arguments;

        public CsdlApplyExpression(string function, IEnumerable<CsdlExpressionBase> arguments, CsdlLocation location)
            : base(location)
        {
            this.function = function;
            this.arguments = new List<CsdlExpressionBase>(arguments);
        }

        public override Expressions.EdmExpressionKind ExpressionKind
        {
            get { return Expressions.EdmExpressionKind.FunctionApplication; }
        }

        public string Function
        {
            get { return this.function; }
        }

        public IEnumerable<CsdlExpressionBase> Arguments
        {
            get { return this.arguments; }
        }
    }
}
