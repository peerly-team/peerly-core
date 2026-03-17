namespace Peerly.Core.Identifiers;

public struct FileId
{
    private readonly long _value;

    public FileId(long value) => _value = value;

    public int CompareTo(FileId other) => _value.CompareTo(other._value);

    public bool Equals(FileId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is FileId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator FileId(long value) => new(value);

    public static explicit operator long(FileId value) => value._value;

    public static bool operator ==(FileId left, FileId right) => left.Equals(right);

    public static bool operator !=(FileId left, FileId right) => !left.Equals(right);

    public static bool operator <(FileId left, FileId right) => left._value < right._value;

    public static bool operator >(FileId left, FileId right) => left._value > right._value;

    public static bool operator <=(FileId left, FileId right) => left._value <= right._value;

    public static bool operator >=(FileId left, FileId right) => left._value >= right._value;

    public static FileId Empty => default;
}
