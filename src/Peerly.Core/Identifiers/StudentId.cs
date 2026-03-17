namespace Peerly.Core.Identifiers;

public struct StudentId
{
    private readonly long _value;

    public StudentId(long value) => _value = value;

    public int CompareTo(StudentId other) => _value.CompareTo(other._value);

    public bool Equals(StudentId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is StudentId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator StudentId(long value) => new(value);

    public static explicit operator long(StudentId value) => value._value;

    public static bool operator ==(StudentId left, StudentId right) => left.Equals(right);

    public static bool operator !=(StudentId left, StudentId right) => !left.Equals(right);

    public static bool operator <(StudentId left, StudentId right) => left._value < right._value;

    public static bool operator >(StudentId left, StudentId right) => left._value > right._value;

    public static bool operator <=(StudentId left, StudentId right) => left._value <= right._value;

    public static bool operator >=(StudentId left, StudentId right) => left._value >= right._value;

    public static StudentId Empty => default;
}
