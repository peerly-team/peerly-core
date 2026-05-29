namespace Peerly.Core.Identifiers;

public struct RubricCriterionId
{
    private readonly long _value;

    public RubricCriterionId(long value) => _value = value;

    public int CompareTo(RubricCriterionId other) => _value.CompareTo(other._value);

    public bool Equals(RubricCriterionId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is RubricCriterionId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator RubricCriterionId(long value) => new(value);

    public static explicit operator long(RubricCriterionId value) => value._value;

    public static bool operator ==(RubricCriterionId left, RubricCriterionId right) => left.Equals(right);

    public static bool operator !=(RubricCriterionId left, RubricCriterionId right) => !left.Equals(right);

    public static bool operator <(RubricCriterionId left, RubricCriterionId right) => left._value < right._value;

    public static bool operator >(RubricCriterionId left, RubricCriterionId right) => left._value > right._value;

    public static bool operator <=(RubricCriterionId left, RubricCriterionId right) => left._value <= right._value;

    public static bool operator >=(RubricCriterionId left, RubricCriterionId right) => left._value >= right._value;

    public static RubricCriterionId Empty => default;
}
