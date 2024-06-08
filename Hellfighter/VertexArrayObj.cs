using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hellfighter;
public class VertexArrayObj {

    public int VAO { get; private set; }

    private readonly VertexBufferObj _vertexBuffer;

    public VertexArrayObj(VertexBufferObj bufferObj) { 
        _vertexBuffer = bufferObj;
        bufferObj.Bind();
        VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);
    }

    public void Bind() {
        _vertexBuffer.Bind();
        GL.BindVertexArray(VAO);
    }

    public void SetAttribArray(int index, int size, VertexAttribPointerType type, bool normalized, int stride, int offset) {
        GL.EnableVertexAttribArray(index);
        GL.VertexAttribPointer(index, size, type, normalized, stride, offset);
    }

    public static void Unbind() {
        VertexBufferObj.Unbind();
        GL.BindVertexArray(0);
    }
}
