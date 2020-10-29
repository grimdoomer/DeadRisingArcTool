using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc
{
    public class VertexStreamSplice<T>
    {
        private T[] array;

        private int baseIndex;
        private int elementCount;

        public int Length { get { return this.elementCount; } }

        public VertexStreamSplice(T[] array, int baseIndex, int length)
        {
            // Make sure the base index and length are valid.
            if (baseIndex < 0 || baseIndex + length >= array.Length)
                throw new ArgumentException("index + length are past the bounds of the array");

            // Initialize fields.
            this.array = array;
            this.baseIndex = baseIndex;
            this.elementCount = length;
        }

        public T this[int index]
        {
            get
            {
                // Make sure the index is valid.
                if (index >= this.elementCount)
                    throw new IndexOutOfRangeException("index is past the bounds of the array");

                // Return the element.
                return this.array[this.baseIndex + index];
            }
            set
            {
                // Make sure the index is valid.
                if (index >= this.elementCount)
                    throw new IndexOutOfRangeException("index is past the bounds of the array");

                // Set the element value.
                this.array[this.baseIndex + index] = value;
            }
        }
    }

    public class VertexStream<V, I>
    {
        public V[] Vertices { get; private set; }
        public I[] Indices { get; private set; }

        public VertexStream(int vertexCount, int indexCount)
        {
            // TODO: Allocate the vertex and index buffers on the pinned object heap and pin them by default.
            // this.Vertices = GC.AllocateArray<V>(vertexCount, pinned: true);
            // this.Indices = GC.AllocateArray<I>(indexCount, pinned: true);
            this.Vertices = new V[vertexCount];
            this.Indices = new I[indexCount];
        }

        ~VertexStream()
        {
            // TODO: Unpin and free the vertex and index buffers.
        }

        public VertexStreamSplice<V> SpliceVertexBuffer(int position, int length)
        {
            // Create a new vertex buffer splice that points to the specified elements in the array.
            return new VertexStreamSplice<V>(this.Vertices, position, length);
        }

        public VertexStreamSplice<I> SpliceIndexBuffer(int position, int length)
        {
            // Create a new index buffer splice that points to the specified elements in the array.
            return new VertexStreamSplice<I>(this.Indices, position, length);
        }
    }
}
