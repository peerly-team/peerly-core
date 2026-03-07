namespace Peerly.Core.Identifiers;

public struct TeacherId
{
    private readonly long _value;

    public TeacherId(long value) => _value = value;

    public int CompareTo(TeacherId other) => _value.CompareTo(other._value);

    public bool Equals(TeacherId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is TeacherId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator TeacherId(long value) => new(value);

    public static explicit operator long(TeacherId value) => value._value;

    public static bool operator ==(TeacherId left, TeacherId right) => left.Equals(right);

    public static bool operator !=(TeacherId left, TeacherId right) => !left.Equals(right);

    public static bool operator <(TeacherId left, TeacherId right) => left._value < right._value;

    public static bool operator >(TeacherId left, TeacherId right) => left._value > right._value;

    public static bool operator <=(TeacherId left, TeacherId right) => left._value <= right._value;

    public static bool operator >=(TeacherId left, TeacherId right) => left._value >= right._value;

    public static TeacherId Empty => default;
}
