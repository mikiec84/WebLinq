// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace WebLinq.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Extensions.Internal;

    partial struct Strings
    {
        public static Strings Values(params string[] values) => new Strings(values);
    }

    // Source:
    // https://github.com/aspnet/Extensions/blob/7ce647cfa3287e31497b72643eee28531eed1b7f/src/Primitives/src/StringValues.cs
    //
    // This is a slightly modified version from the snapshot above with the
    // following changes:
    //
    // - Moved from namespace Microsoft.Extensions.Primitives to one belonging
    //   to this project.
    // - Renamed from StringValues to Strings.
    // - Marked partial.
    // - Re-styled to use project conventions.

    /// <summary>
    /// Represents zero/null, one, or many strings in an efficient way.
    /// </summary>

    public readonly partial struct Strings :
        IList<string>,
        IReadOnlyList<string>,
        IEquatable<Strings>,
        IEquatable<string>,
        IEquatable<string[]>
    {
        static readonly string[] EmptyArray = new string[0];
        public static readonly Strings Empty = new Strings(EmptyArray);

        readonly string _value;
        readonly string[] _values;

        public Strings(string value)
        {
            _value = value;
            _values = null;
        }

        public Strings(string[] values)
        {
            _value = null;
            _values = values;
        }

        public static implicit operator Strings(string value) =>
            new Strings(value);

        public static implicit operator Strings(string[] values) =>
            new Strings(values);

        public static implicit operator string (Strings values) =>
            values.GetStringValue();

        public static implicit operator string[] (Strings value) =>
            value.GetArrayValue();

        public int Count => _value != null ? 1 : (_values?.Length ?? 0);

        bool ICollection<string>.IsReadOnly => true;

        string IList<string>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public string this[int index]
            => _values != null ? _values[index]
             : index == 0 && _value != null ? _value
             : EmptyArray[0];

        public override string ToString() =>
            GetStringValue() ?? string.Empty;

        string GetStringValue()
        {
            if (_values == null)
                return _value;

            switch (_values.Length)
            {
                case 0: return null;
                case 1: return _values[0];
                default: return string.Join(",", _values);
            }
        }

        public string[] ToArray() =>
            GetArrayValue() ?? EmptyArray;

        string[] GetArrayValue() =>
            _value != null ? new[] { _value } : _values;

        int IList<string>.IndexOf(string item) =>
            IndexOf(item);

        int IndexOf(string item)
        {
            if (_values != null)
            {
                var values = _values;
                for (var i = 0; i < values.Length; i++)
                {
                    if (string.Equals(values[i], item, StringComparison.Ordinal))
                        return i;
                }

                return -1;
            }

            return _value != null
                 ? string.Equals(_value, item, StringComparison.Ordinal) ? 0 : -1
                 : -1;
        }

        bool ICollection<string>.Contains(string item) =>
            IndexOf(item) >= 0;

        void ICollection<string>.CopyTo(string[] array, int arrayIndex) =>
            CopyTo(array, arrayIndex);

        void CopyTo(string[] array, int arrayIndex)
        {
            if (_values != null)
            {
                Array.Copy(_values, 0, array, arrayIndex, _values.Length);
                return;
            }

            if (_value != null)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));

                if (array.Length - arrayIndex < 1)
                {
                    throw new ArgumentException(
                        $"'{nameof(array)}' is not long enough to copy all the items in the collection. Check '{nameof(arrayIndex)}' and '{nameof(array)}' length.");
                }

                array[arrayIndex] = _value;
            }
        }

        void ICollection<string>.Add(string item) => throw new NotSupportedException();
        void IList<string>.Insert(int index, string item) => throw new NotSupportedException();
        bool ICollection<string>.Remove(string item) => throw new NotSupportedException();
        void IList<string>.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection<string>.Clear() => throw new NotSupportedException();

        public Enumerator GetEnumerator() =>
            new Enumerator(_values, _value);

        IEnumerator<string> IEnumerable<string>.GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public static bool IsNullOrEmpty(Strings value)
        {
            if (value._values == null)
                return string.IsNullOrEmpty(value._value);

            switch (value._values.Length)
            {
                case 0: return true;
                case 1: return string.IsNullOrEmpty(value._values[0]);
                default: return false;
            }
        }

        public static Strings Concat(Strings values1, Strings values2)
        {
            var count1 = values1.Count;
            var count2 = values2.Count;

            if (count1 == 0)
                return values2;

            if (count2 == 0)
                return values1;

            var combined = new string[count1 + count2];
            values1.CopyTo(combined, 0);
            values2.CopyTo(combined, count1);
            return new Strings(combined);
        }

        public static Strings Concat(in Strings values, string value)
        {
            if (value == null)
                return values;

            var count = values.Count;
            if (count == 0)
                return new Strings(value);

            var combined = new string[count + 1];
            values.CopyTo(combined, 0);
            combined[count] = value;
            return new Strings(combined);
        }

        public static Strings Concat(string value, in Strings values)
        {
            if (value == null)
                return values;

            var count = values.Count;
            if (count == 0)
                return new Strings(value);

            var combined = new string[count + 1];
            combined[0] = value;
            values.CopyTo(combined, 1);
            return new Strings(combined);
        }

        public static bool Equals(Strings left, Strings right)
        {
            var count = left.Count;

            if (count != right.Count)
                return false;

            for (var i = 0; i < count; i++)
            {
                if (left[i] != right[i])
                    return false;
            }

            return true;
        }

        public static bool operator ==(Strings left, Strings right) =>
            Equals(left, right);

        public static bool operator !=(Strings left, Strings right) =>
            !Equals(left, right);

        public bool Equals(Strings other) =>
            Equals(this, other);

        public static bool Equals(string left, Strings right) =>
            Equals(new Strings(left), right);

        public static bool Equals(Strings left, string right) =>
            Equals(left, new Strings(right));

        public bool Equals(string other) =>
            Equals(this, new Strings(other));

        public static bool Equals(string[] left, Strings right) =>
            Equals(new Strings(left), right);

        public static bool Equals(Strings left, string[] right) =>
            Equals(left, new Strings(right));

        public bool Equals(string[] other) =>
            Equals(this, new Strings(other));

        public static bool operator ==(Strings left, string right) =>
            Equals(left, new Strings(right));

        public static bool operator !=(Strings left, string right) =>
            !Equals(left, new Strings(right));

        public static bool operator ==(string left, Strings right) =>
            Equals(new Strings(left), right);

        public static bool operator !=(string left, Strings right) =>
            !Equals(new Strings(left), right);

        public static bool operator ==(Strings left, string[] right) =>
            Equals(left, new Strings(right));

        public static bool operator !=(Strings left, string[] right) =>
            !Equals(left, new Strings(right));

        public static bool operator ==(string[] left, Strings right) =>
            Equals(new Strings(left), right);

        public static bool operator !=(string[] left, Strings right) =>
            !Equals(new Strings(left), right);

        public static bool operator ==(Strings left, object right) =>
            left.Equals(right);

        public static bool operator !=(Strings left, object right) =>
            !left.Equals(right);

        public static bool operator ==(object left, Strings right) =>
            right.Equals(left);

        public static bool operator !=(object left, Strings right) =>
            !right.Equals(left);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null: return Equals(this, Empty);
                case string b: return Equals(this, b);
                case string[] b: return Equals(this, b);
                case Strings b: return Equals(this, b);
                default: return false;
            }
        }

        public override int GetHashCode()
        {
            if (_values == null)
                return _value == null ? 0 : _value.GetHashCode();

            var hcc = new HashCodeCombiner();
            foreach (var v in _values)
                hcc.Add(v);
            return hcc.CombinedHash;
        }

        public struct Enumerator : IEnumerator<string>
        {
            readonly string[] _values;
            string _current;
            int _index;

            internal Enumerator(string[] values, string value)
            {
               _values = values;
               _current = value;
               _index = 0;
            }

            public Enumerator(ref Strings values)
            {
                _values = values._values;
                _current = values._value;
                _index = 0;
            }

            public bool MoveNext()
            {
                if (_index < 0)
                    return false;

                if (_values != null)
                {
                    if (_index < _values.Length)
                    {
                        _current = _values[_index];
                        _index++;
                        return true;
                    }

                    _index = -1;
                    return false;
                }

                _index = -1; // sentinel value
                return _current != null;
            }

            public string Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset() =>
                throw new NotSupportedException();

            public void Dispose() {}
        }
    }
}
