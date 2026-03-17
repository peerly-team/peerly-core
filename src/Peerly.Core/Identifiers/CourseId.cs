namespace Peerly.Core.Identifiers;

public struct CourseId
{
    private readonly long _value;

    public CourseId(long value) => _value = value;

    public int CompareTo(CourseId other) => _value.CompareTo(other._value);

    public bool Equals(CourseId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is CourseId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator CourseId(long value) => new(value);

    public static explicit operator long(CourseId value) => value._value;

    public static bool operator ==(CourseId left, CourseId right) => left.Equals(right);

    public static bool operator !=(CourseId left, CourseId right) => !left.Equals(right);

    public static bool operator <(CourseId left, CourseId right) => left._value < right._value;

    public static bool operator >(CourseId left, CourseId right) => left._value > right._value;

    public static bool operator <=(CourseId left, CourseId right) => left._value <= right._value;

    public static bool operator >=(CourseId left, CourseId right) => left._value >= right._value;

    public static CourseId Empty => default;
}
