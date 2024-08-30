using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FixStuff
{
    public readonly partial struct Tag
    {
        /// <summary>
        /// Used to hold the tag information
        /// </summary>
        [InlineArray(8)]
        private struct EncodedData
        {
            public byte Data;
        }

        /// <summary>
        /// An enumerator optimized for foreach statements
        /// that will not box or allocate memory
        /// </summary>
        public ref struct Enumerator
        {
            private readonly Tag m_Tag;
            private int m_Index;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="span">The span to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Tag tag)
            {
                m_Tag = tag;
                m_Index = -1;
            }

            /// <summary>Advances the enumerator to the next element of the span.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int index = m_Index + 1;
                if (index < m_Tag.Length)
                {
                    m_Index = index;
                    return true;
                }

                return false;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            public byte Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get{return m_Tag.m_Data[m_Index + Tag.TextOffset];}
            }
        }
    }
}
