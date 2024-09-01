using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FixStuff
{
    public readonly struct FixMonthYear
    {
        private readonly int m_Length;
        private readonly EncodedData m_Data;

        private FixMonthYear(bool _, int year, int month, int length)
        {
            m_Length = length;

            m_Data[3] = (byte)((year % 10) + '0');
            year /= 10;
            m_Data[2] = (byte)((year % 10) + '0');
            year /= 10;
            m_Data[1] = (byte)((year % 10) + '0');
            year /= 10;
            m_Data[0] = (byte)((year % 10) + '0');

            m_Data[5] = (byte)((month % 10) + '0');
            month /= 10;
            m_Data[4] = (byte)((month % 10) + '0');
        }

        public FixMonthYear(ReadOnlySpan<byte> data)
        {
            if(data.Length == 6 || data.Length == 8)
            {
                data.CopyTo(m_Data);
                m_Length = data.Length;
            }
            else
            {
                throw new ArgumentException($"invalid encoding for a {nameof(FixMonthYear)}");
            }
        }

        public FixMonthYear(int year, int month) : this(true, year, month, 6)
        {
            if(year < 0) throw new ArgumentException("invalid year");
            if(month < 1 || month > 12) throw new ArgumentException("invalid month");
        }

        public FixMonthYear(int year, int month, int dayOfMonth) : this(true, year, month, 8)
        {
            if(dayOfMonth < 1 || dayOfMonth > 31) throw new ArgumentException("invalid day of month");
            
            if(dayOfMonth == 31 && (month == 4 || month == 6 || month == 9 || month == 11))
            {
                throw new ArgumentException("month can only have 30 days");
            }

            // Let's give February the love it deserves!
            if(month == 2 && dayOfMonth > 29) throw new ArgumentException("invalid day of month");
            if(month == 2 && DateTime.IsLeapYear(year) == false && dayOfMonth == 29) throw new ArgumentException("invalid day of month for non leap year");

            m_Data[7] = (byte)((dayOfMonth % 10) + '0');
            dayOfMonth /= 10;
            m_Data[6] = (byte)((dayOfMonth % 10) + '0');
        }

        public FixMonthYear(int year, int month, WeekCode weekCode) : this(true, year, month, 8)
        {
            if(weekCode < 0 || weekCode > WeekCode.W5) throw new ArgumentException("invalid week code");

            var asInt = (int)weekCode;
            m_Data[7] = (byte)((asInt + 1) + '0');
            m_Data[6] = (byte)('w');
        }

        public int Length
        {
            get{return m_Length;}
        }

        public bool IsValid
        {
            get{return m_Length != 0;}
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
                    return m_Data[index];
                }

                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Copies the ascii encoded bytes to a destination buffer
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>The number of bytes written</returns>
        public int CopyTo(Span<byte> destination)
        {
            ReadOnlySpan<byte> data = m_Data[0..m_Length];
            data.CopyTo(destination);
            return data.Length;
        }

        public string AsString()
        {
            // We can creates the integer directly into the string buffer
            return string.Create(m_Length, this, static (span, state) =>
            {
                var length = state.m_Length;

                for(var i = 0; i < length; i++)
                {
                    span[i] = (char)state.m_Data[i];
                }
            });
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.IsValid ? AsString() : "<invalid>";
        }

        /// <summary>
        /// Used to hold the tag information
        /// </summary>
        [InlineArray(8)]
        private struct EncodedData
        {
            public byte Data;
        }
    }
}
