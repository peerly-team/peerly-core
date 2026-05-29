namespace Peerly.Core.Identifiers;

public struct RubricCriteriaId
{
    private readonly long _value;

    public RubricCriteriaId(long value) => _value = value;

    public int CompareTo(RubricCriteriaId other) => _value.CompareTo(other._value);

    public bool Equals(RubricCriteriaId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is RubricCriteriaId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator RubricCriteriaId(long value) => new(value);

    public static explicit operator long(RubricCriteriaId value) => value._value;

    public static bool operator ==(RubricCriteriaId left, RubricCriteriaId right) => left.Equals(right);

    public static bool operator !=(RubricCriteriaId left, RubricCriteriaId right) => !left.Equals(right);

    public static bool operator <(RubricCriteriaId left, RubricCriteriaId right) => left._value < right._value;

    public static bool operator >(RubricCriteriaId left, RubricCriteriaId right) => left._value > right._value;

    public static bool operator <=(RubricCriteriaId left, RubricCriteriaId right) => left._value <= right._value;

    public static bool operator >=(RubricCriteriaId left, RubricCriteriaId right) => left._value >= right._value;

    public static RubricCriteriaId Empty => default;
}
