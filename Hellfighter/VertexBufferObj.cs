using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Hellfighter;

public class VertexBufferObj<T> : VertexBufferObj where T : unmanaged {

    public VertexBufferObj(T[] data, int length) {

        VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData<T>(BufferTarget.ArrayBuffer, length, data, BufferUsageHint.StaticDraw);
    }
}

public class VertexBufferObj {
    public int VBO { get; protected set; }

    public void Bind() {
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
    }

    public static void Unbind() {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
}
