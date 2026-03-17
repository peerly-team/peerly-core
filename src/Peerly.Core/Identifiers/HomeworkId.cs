namespace Peerly.Core.Identifiers;

public struct HomeworkId
{
    private readonly long _value;

    public HomeworkId(long value) => _value = value;

    public int CompareTo(HomeworkId other) => _value.CompareTo(other._value);

    public bool Equals(HomeworkId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is HomeworkId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator HomeworkId(long value) => new(value);

    public static explicit operator long(HomeworkId value) => value._value;

    public static bool operator ==(HomeworkId left, HomeworkId right) => left.Equals(right);

    public static bool operator !=(HomeworkId left, HomeworkId right) => !left.Equals(right);

    public static bool operator <(HomeworkId left, HomeworkId right) => left._value < right._value;

    public static bool operator >(HomeworkId left, HomeworkId right) => left._value > right._value;

    public static bool operator <=(HomeworkId left, HomeworkId right) => left._value <= right._value;

    public static bool operator >=(HomeworkId left, HomeworkId right) => left._value >= right._value;

    public static HomeworkId Empty => default;
}
