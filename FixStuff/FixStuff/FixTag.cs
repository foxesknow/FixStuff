using NUnit.Framework.Constraints;
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FixStuff
{
    /// <summary>
    /// Models a FIX tag.
    /// It's assumed that FIX tags are from 0 to 99999 inclusives
    /// </summary>
    public readonly partial struct FixTag : IEquatable<FixTag>, IComparable<FixTag>, IEnumerable<byte>
    {
        private const int LengthIndex = 0;
        private const int TextOffset = 1;
        private const int Width = 5;

        private const byte AsciiZero = (byte)'0';
        private const byte AsciiNine= (byte)'9';

        public static readonly FixTag None = default;

        private readonly EncodedData m_Data;
        private readonly int m_Value;

        /// <summary>
        /// Creates a tag which is not valid
        /// </summary>
        public FixTag()
        {
        }

        /// <summary>
        /// Creates a tag from an ascii encoded byte stream.
        /// </summary>
        /// <param name="data"></param>
        public FixTag(ReadOnlySpan<byte> data)
        {
            if(data.Length > Width) throw new ArgumentException("only values 0 to 99999 supported");
            
            var length = data.Length;
            m_Data[LengthIndex] = (byte)length;

            for(var i = 0; i < length; i++)
            {
                var digit = data[i];
                ValidateAsciiByte(digit);

                m_Data[i + TextOffset] = digit;
                m_Value = (m_Value * 10) + (digit - AsciiZero);
            }
        }

        /// <summary>
        /// Creates a tag from a string
        /// </summary>
        /// <param name="text"></param>
        /// <exception cref="ArgumentException"></exception>
        public FixTag(string text)
        {
            ArgumentException.ThrowIfNullOrEmpty(text, nameof(text));

            var length = text.Length;
            if(length > Width) throw new ArgumentException("only values 0 to 99999 supported");

            var value = 0;
            Span<byte> buffer = stackalloc byte[text.Length];
            for(int i = 0; i < length; i++)
            {
                var b = (byte)text[i];
                ValidateAsciiByte(b);

                buffer[i] = b;
                value = (value * 10) + (b - AsciiZero);
            }

            Span<byte> destination = m_Data;
            destination[LengthIndex] = (byte)length;
            buffer.CopyTo(destination.Slice(TextOffset));
            m_Value = value;
        }

        /// <summary>
        /// Creates a tag from a integer
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public FixTag(int value)
        {
            if(value < 0 || value > 99999) throw new ArgumentException($"invalid tag value: {value}");

            m_Value = value;

            Span<byte> buffer = stackalloc byte[Width];

            var i = Width - 1;
            do
            {
                var x = value % 10;
                value /= 10;
                buffer[i] = (byte)(x + AsciiZero);
                i--;
            }while(value != 0);

            var slice = buffer.Slice(i + 1);
            Span<byte> destination = m_Data;
            destination[LengthIndex] = (byte)slice.Length;
            slice.CopyTo(destination.Slice(TextOffset));
        }

        /// <summary>
        /// Applied the ascii encoded buffer to an action
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <param name="action"></param>
        public void Apply<TState>(TState state, ReadOnlySpanAction<byte, TState> action)
        {
            ReadOnlySpan<byte> data = m_Data[TextRange];
            action(data, state);
        }

        /// <summary>
        /// Copies the ascii encoded bytes of the tag to a destination buffer
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>The number of bytes written</returns>
        public int CopyTo(Span<byte> destination)
        {
            ReadOnlySpan<byte> data = m_Data[TextRange];
            data.CopyTo(destination);
            return data.Length;
        }

        /// <summary>
        /// Efficiently creates a string representation of the tag.
        /// It is the callers job to make sure the tag is valid
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            // We can creates the integer directly into the string buffer
            return string.Create(m_Data[LengthIndex], this, static (span, state) =>
            {
                var length = state.Length;

                for(var i = 0; i < length; i++)
                {
                    span[i] = (char)state.m_Data[i + TextOffset];
                }
            });
        }

        /// <summary>
        /// Returns the ascii value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public byte this[int index]
        {
            get
            {
                if(index >= 0 && index < this.Length)
                {
                    return m_Data[TextOffset + index];
                }

                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns an enumerator for the ascii data in the tag.
        /// This method is optimized for the foreach statement.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new(this);
        }

        /// <inheritdoc/>
        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            for(var i = 0; i < this.Length; i++)
            {
                yield return m_Data[TextOffset + i];
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<byte> self = this;
            return self.GetEnumerator();
        }

        /// <summary>
        /// Indicates if the tag is valid
        /// </summary>
        public bool IsValid
        {
            get{return m_Data[LengthIndex] != 0;}
        }

        /// <summary>
        /// The number of ascii digits in the tag
        /// </summary>
        public int Length
        {
            get{return m_Data[LengthIndex];}
        }

        /// <summary>
        /// Returns the integer value for a tag, or zero if the tag is not valid
        /// </summary>
        public int Value
        {
            get{return m_Value;}
        }

        /// <inheritdoc/>
        public int CompareTo(FixTag other)
        {
            return (this.IsValid, m_Value).CompareTo((other.IsValid, other.Value));
        }

        /// <inheritdoc/>
        public bool Equals(FixTag other)
        {
            return this.IsValid == other.IsValid && m_Value == other.m_Value;
        }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is FixTag tag && Equals(tag);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (this.IsValid, m_Value).GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.IsValid ? AsString() : "<none>";
        }

        /// <summary>
        /// Compares 2 tags for equality
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator==(in FixTag lhs, in FixTag rhs)
        {
            return lhs.IsValid == rhs.IsValid && lhs.Value == rhs.Value;
        }

        /// <summary>
        /// Compares 2 tags for inequality
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator!=(in FixTag lhs, in FixTag rhs)
        {
            return !(lhs == rhs);
        }        

        /// <summary>
        /// Creates a tag from a string
        /// </summary>
        /// <param name="text"></param>
        public static implicit operator FixTag(string text)
        {
            return new(text);
        }

        /// <summary>
        /// Creates a tag from an integer
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator FixTag(int value)
        {
            return new(value);
        }

        private Range TextRange
        {
            get{return new Range(TextOffset, TextOffset + this.Length);}
        }

        private void ValidateAsciiByte(byte value)
        {
            if(value >= AsciiZero && value <= AsciiNine) return;

            throw new ArgumentException($"not a valid ascii number code: {value}");
        }
    }
}
