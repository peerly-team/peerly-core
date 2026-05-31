namespace Peerly.Core.Identifiers;

public struct RubricId
{
    private readonly long _value;

    public RubricId(long value) => _value = value;

    public int CompareTo(RubricId other) => _value.CompareTo(other._value);

    public bool Equals(RubricId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is RubricId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator RubricId(long value) => new(value);

    public static explicit operator long(RubricId value) => value._value;

    public static bool operator ==(RubricId left, RubricId right) => left.Equals(right);

    public static bool operator !=(RubricId left, RubricId right) => !left.Equals(right);

    public static bool operator <(RubricId left, RubricId right) => left._value < right._value;

    public static bool operator >(RubricId left, RubricId right) => left._value > right._value;

    public static bool operator <=(RubricId left, RubricId right) => left._value <= right._value;

    public static bool operator >=(RubricId left, RubricId right) => left._value >= right._value;

    public static RubricId Empty => default;
}
