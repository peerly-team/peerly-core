namespace Peerly.Core.Identifiers;

public struct SubmittedReviewScoreId
{
    private readonly long _value;

    public SubmittedReviewScoreId(long value) => _value = value;

    public int CompareTo(SubmittedReviewScoreId other) => _value.CompareTo(other._value);

    public bool Equals(SubmittedReviewScoreId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is SubmittedReviewScoreId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator SubmittedReviewScoreId(long value) => new(value);

    public static explicit operator long(SubmittedReviewScoreId value) => value._value;

    public static bool operator ==(SubmittedReviewScoreId left, SubmittedReviewScoreId right) => left.Equals(right);

    public static bool operator !=(SubmittedReviewScoreId left, SubmittedReviewScoreId right) => !left.Equals(right);

    public static bool operator <(SubmittedReviewScoreId left, SubmittedReviewScoreId right) => left._value < right._value;

    public static bool operator >(SubmittedReviewScoreId left, SubmittedReviewScoreId right) => left._value > right._value;

    public static bool operator <=(SubmittedReviewScoreId left, SubmittedReviewScoreId right) => left._value <= right._value;

    public static bool operator >=(SubmittedReviewScoreId left, SubmittedReviewScoreId right) => left._value >= right._value;

    public static SubmittedReviewScoreId Empty => default;
}
