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

    /// <summary>
    /// This class is responsible for executing an action the first time dispose is called on it.
    /// </summary>
    internal class ActionOnDispose : IDisposable
    {
        /// <summary>The action to be executed on dispose</summary>
        private Action action;
        
        /// <summary>
        /// Constructs an instance of the ActonOnDispose object
        /// </summary>
        /// <param name="action">the action to be execute on dispose</param>
        public ActionOnDispose(Action action)
        {
            Util.CheckArgumentNull(action, "action");
            this.action = action;
        }

        /// <summary>
        /// The dipose method of the IDisposable insterface
        /// </summary>
        public void Dispose()
        {
            if (this.action != null)
            {
                action();
                action = null;
            }
        }
    }
}
