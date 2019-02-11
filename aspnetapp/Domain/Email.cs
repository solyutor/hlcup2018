using System;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using aspnetapp.Sys;

namespace aspnetapp
{
    public readonly struct Email : IEquatable<Email>, IComparable<Email>
    {
        public readonly Utf8String _email;
        public readonly ushort _domain;

        public bool IsEmpty => _email.IsEmpty;

        public Email(Utf8String email, ushort domain)
        {
            _email = email;
            _domain = domain;
        }

        public bool Equals(Email other) => _domain == other._domain && HOrdinalComparer.Instance.Equals(_email, other._email);

        public override bool Equals(object obj) => obj is Email other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (_email.GetHashCode() * 397) ^ _domain;
            }
        }

        public static bool operator ==(Email left, Email right) => left.Equals(right);

        public static bool operator !=(Email left, Email right) => !left.Equals(right);

        public int CompareTo(Prefix prefix)
        {
            return CompareTo(prefix.Span);
        }
        public int CompareTo(ReadOnlySpan<byte> other)
        {
            return _email.Span.SequenceCompareTo(other);
        }

        public int CompareTo(Email other)
        {
            var emailComparison = _email.Span.SequenceCompareTo(other._email.Span);
            if (emailComparison != 0)
            {
                return emailComparison;
            }

            return string.CompareOrdinal(StringIndexer.Domains[_domain], StringIndexer.Domains[other._domain]);
        }

        public static bool operator <(Email left, Email right) => left.CompareTo(right) < 0;

        public static bool operator >(Email left, Email right) => left.CompareTo(right) > 0;

        public static bool operator <=(Email left, Email right) => left.CompareTo(right) <= 0;

        public static bool operator >=(Email left, Email right) => left.CompareTo(right) >= 0;

        public override string ToString() => $"{_email}@{StringIndexer.Domains[_domain]}";

        public static unsafe implicit operator SpanEmail (Email value) => new SpanEmail(value._email.Pointer, (byte) value._email.Length, value._domain);

    }
}