using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Loretta.Generators.SyntaxKindGenerators
{
    internal class KindList : IReadOnlyList<KindInfo>
    {
        private readonly ImmutableArray<KindInfo> _kinds;
        private readonly IImmutableDictionary<String, KindInfo> _kindMap;

        public KindList ( ImmutableArray<KindInfo> kinds )
        {

            this._kinds = kinds;
            this._kindMap = kinds.ToImmutableDictionary ( kind => kind.Field.Name, StringComparer.Ordinal );
        }

        #region IReadOnlyList<KindInfo>
        public KindInfo this[Int32 index] => ( ( IReadOnlyList<KindInfo> ) this._kinds )[index];
        public Int32 Count => ( ( IReadOnlyCollection<KindInfo> ) this._kinds ).Count;
        public IEnumerator<KindInfo> GetEnumerator ( ) => ( ( IEnumerable<KindInfo> ) this._kinds ).GetEnumerator ( );
        IEnumerator IEnumerable.GetEnumerator ( ) => ( ( IEnumerable ) this._kinds ).GetEnumerator ( );
        #endregion IReadOnlyList<KindInfo>

        #region Dictionary
        public KindInfo this[String key] => this._kindMap[key];
        public Boolean ContainsKind ( String key ) => this._kindMap.ContainsKey ( key );
        public Boolean TryGetKind ( String key, out KindInfo value ) => this._kindMap.TryGetValue ( key, out value );
        #endregion Dictionary

        public Boolean IsToken ( String kindName ) =>
            this.TryGetKind ( kindName, out KindInfo kind ) && kind.TokenInfo is not null;

        public Boolean IsAutoCreatableToken ( String kindName ) =>
            this.TryGetKind ( kindName, out KindInfo kind )
            && !String.IsNullOrWhiteSpace ( kind.TokenInfo?.Text );
    }
}
