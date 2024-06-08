using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Hellfighter;
public class Shader : IDisposable {

    public int Handle { get; private set; }

    private int _vertexShader;
    private int _fragmentShader;

    private bool _disposedValue;

    public Shader(string vertexFileName, string fragmentFileName) {

        string vertexPath = Path.Combine(TheUtils.CurrentPath, "Shaders", vertexFileName);
        string fragmentPath = Path.Combine(TheUtils.CurrentPath, "Shaders", fragmentFileName);

        string vertexShaderSource = File.ReadAllText(vertexPath);
        string fragmentShaderSource = File.ReadAllText(fragmentPath);

        _vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_vertexShader, vertexShaderSource);

        _fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_fragmentShader, fragmentShaderSource);

        GL.CompileShader(_vertexShader);

        GL.GetShader(_vertexShader, ShaderParameter.CompileStatus, out int success);
        if (success == 0) {
            string infoLog = GL.GetShaderInfoLog(_vertexShader);
            Console.WriteLine(infoLog);
        }

        GL.CompileShader(_fragmentShader);

        GL.GetShader(_fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0) {
            string infoLog = GL.GetShaderInfoLog(_fragmentShader);
            Console.WriteLine(infoLog);
        }

        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, _vertexShader);
        GL.AttachShader(Handle, _fragmentShader);

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0) {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }

        // Cleanup
        GL.DetachShader(Handle, _vertexShader);
        GL.DetachShader(Handle, _fragmentShader);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
    }

    public void Bind() {
        GL.UseProgram(Handle);
    }

    public int GetAttribLocation(string attribName) {
        return GL.GetAttribLocation(Handle, attribName);
    }

    public int GetUniformLocation(string name) {
        return GL.GetUniformLocation(Handle, name);
    }

    public void SetInt(string name, int value) {
        int location = GL.GetUniformLocation(Handle, name);

        GL.Uniform1(location, value);
    }

    public void SetMatrix4(string transform, Matrix4 mat) {
        int location = GL.GetUniformLocation(Handle, transform);
        GL.UniformMatrix4(location, true, ref mat);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (!_disposedValue) {
            GL.DeleteProgram(Handle);
            _disposedValue = true;
        }       
    }

    public void SetVector3(in string v, Vector3 vector3) {
        int location = GL.GetUniformLocation(Handle, v);
        GL.Uniform3(location, vector3.X, vector3.Y, vector3.Z);
    }

    public void SetFloat(in string v, float value) {
        int location = GL.GetUniformLocation(Handle, v);
        GL.Uniform1(location, value);
    }

    ~Shader() {
        if (_disposedValue == false) {
            Console.WriteLine("GPU Resource leak. Did you forget to call Dispose?");
        }
    }
}
