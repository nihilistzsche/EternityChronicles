// MetaObjectBuilder.cs in EternityChronicles/IronDragon
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Dynamic;
using System.Linq.Expressions;
using IronDragon.Expressions;

//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace IronDragon.Runtime
{
    internal class MetaObjectBuilder
    {
        private Expression _metaResult;
        private BindingRestrictions _restrictions;

        public MetaObjectBuilder(DragonMetaObject target, DynamicMetaObject[] args)
        {
            _restrictions = null;
            if (target.Restrictions != BindingRestrictions.Empty) _restrictions = target.Restrictions;
        }

        protected void AddRestrictions(DynamicMetaObject[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Restrictions != BindingRestrictions.Empty)
                    AddRestriction(arg.Restrictions);
                else // Runtime check!
                    AddRestriction(
                                   BindingRestrictions.GetExpressionRestriction(DragonExpression.Binary(arg.Expression,
                                                                                    Expression
                                                                                        .Constant(arg.Value),
                                                                                    ExpressionType.Equal)));
            }
        }

        public void SetMetaResult(Expression result)
        {
            _metaResult = result;
        }

        public void AddRestriction(BindingRestrictions restriction)
        {
            if (_restrictions == null)
                _restrictions = restriction;
            else
                _restrictions.Merge(restriction);
        }

        public DynamicMetaObject CreateMetaObject(IInteropBinder binder)
        {
            return new DynamicMetaObject(_metaResult, _restrictions);
        }
    }
}