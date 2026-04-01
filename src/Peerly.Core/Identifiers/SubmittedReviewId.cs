namespace Peerly.Core.Identifiers;

public struct SubmittedReviewId
{
    private readonly long _value;

    public SubmittedReviewId(long value) => _value = value;

    public int CompareTo(SubmittedReviewId other) => _value.CompareTo(other._value);

    public bool Equals(SubmittedReviewId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is SubmittedReviewId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator SubmittedReviewId(long value) => new(value);

    public static explicit operator long(SubmittedReviewId value) => value._value;

    public static bool operator ==(SubmittedReviewId left, SubmittedReviewId right) => left.Equals(right);

    public static bool operator !=(SubmittedReviewId left, SubmittedReviewId right) => !left.Equals(right);

    public static bool operator <(SubmittedReviewId left, SubmittedReviewId right) => left._value < right._value;

    public static bool operator >(SubmittedReviewId left, SubmittedReviewId right) => left._value > right._value;

    public static bool operator <=(SubmittedReviewId left, SubmittedReviewId right) => left._value <= right._value;

    public static bool operator >=(SubmittedReviewId left, SubmittedReviewId right) => left._value >= right._value;

    public static SubmittedReviewId Empty => default;
}
