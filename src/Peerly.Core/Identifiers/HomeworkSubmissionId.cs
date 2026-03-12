namespace Peerly.Core.Identifiers;

public struct HomeworkSubmissionId
{
    private readonly long _value;

    public HomeworkSubmissionId(long value) => _value = value;

    public int CompareTo(HomeworkSubmissionId other) => _value.CompareTo(other._value);

    public bool Equals(HomeworkSubmissionId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is HomeworkSubmissionId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator HomeworkSubmissionId(long value) => new(value);

    public static explicit operator long(HomeworkSubmissionId value) => value._value;

    public static bool operator ==(HomeworkSubmissionId left, HomeworkSubmissionId right) => left.Equals(right);

    public static bool operator !=(HomeworkSubmissionId left, HomeworkSubmissionId right) => !left.Equals(right);

    public static bool operator <(HomeworkSubmissionId left, HomeworkSubmissionId right) => left._value < right._value;

    public static bool operator >(HomeworkSubmissionId left, HomeworkSubmissionId right) => left._value > right._value;

    public static bool operator <=(HomeworkSubmissionId left, HomeworkSubmissionId right) => left._value <= right._value;

    public static bool operator >=(HomeworkSubmissionId left, HomeworkSubmissionId right) => left._value >= right._value;

    public static HomeworkSubmissionId Empty => default;
}
