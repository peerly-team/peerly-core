namespace Peerly.Core.Identifiers;

public struct SubmittedHomeworkId
{
    private readonly long _value;

    public SubmittedHomeworkId(long value) => _value = value;

    public int CompareTo(SubmittedHomeworkId other) => _value.CompareTo(other._value);

    public bool Equals(SubmittedHomeworkId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is SubmittedHomeworkId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator SubmittedHomeworkId(long value) => new(value);

    public static explicit operator long(SubmittedHomeworkId value) => value._value;

    public static bool operator ==(SubmittedHomeworkId left, SubmittedHomeworkId right) => left.Equals(right);

    public static bool operator !=(SubmittedHomeworkId left, SubmittedHomeworkId right) => !left.Equals(right);

    public static bool operator <(SubmittedHomeworkId left, SubmittedHomeworkId right) => left._value < right._value;

    public static bool operator >(SubmittedHomeworkId left, SubmittedHomeworkId right) => left._value > right._value;

    public static bool operator <=(SubmittedHomeworkId left, SubmittedHomeworkId right) => left._value <= right._value;

    public static bool operator >=(SubmittedHomeworkId left, SubmittedHomeworkId right) => left._value >= right._value;

    public static SubmittedHomeworkId Empty => default;
}
