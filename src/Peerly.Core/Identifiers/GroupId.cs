namespace Peerly.Core.Identifiers;

public struct GroupId
{
    private readonly long _value;

    public GroupId(long value) => _value = value;

    public int CompareTo(GroupId other) => _value.CompareTo(other._value);

    public bool Equals(GroupId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is GroupId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator GroupId(long value) => new(value);

    public static explicit operator long(GroupId value) => value._value;

    public static bool operator ==(GroupId left, GroupId right) => left.Equals(right);

    public static bool operator !=(GroupId left, GroupId right) => !left.Equals(right);

    public static bool operator <(GroupId left, GroupId right) => left._value < right._value;

    public static bool operator >(GroupId left, GroupId right) => left._value > right._value;

    public static bool operator <=(GroupId left, GroupId right) => left._value <= right._value;

    public static bool operator >=(GroupId left, GroupId right) => left._value >= right._value;

    public static GroupId Empty => default;
}
