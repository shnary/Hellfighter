using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Hellfighter;

public class Mesh {

    public enum TexType {
        Diffuse, Specular
    }

    public struct Texture {
        public int Id;
        public TexType TexType;
    }

    private int _vao;
    private int _vbo;
    private int _ebo;

    private List<Vector3> _vertPositions;
    private List<Vector3> _vertNormals;
    private List<Vector2> _vertTexCoords;
    
    private List<int> _indices;
    private List<Texture> _textures;

    public Mesh(List<Vector3> vertPositions, List<Vector3> vertNormals, List<Vector2> vertTexCoords, List<int> indices, List<Texture> textures) {
        _vertPositions = vertPositions;
        _vertNormals = vertNormals;
        _vertTexCoords = vertTexCoords;
        _indices = indices;
        _textures = textures;

        SetupMesh();
    }

    public void Draw(Shader shader) {
        int diffuseNr = 1;
        int specularNr = 1;
        for(int i = 0; i < _textures.Count; i++) {
            GL.ActiveTexture(TextureUnit.Texture0 + i);
            // retrieve texture number (the N in diffuse_textureN)
            string number = "";
            var type = _textures[i].TexType;
            if(type == TexType.Diffuse)
                number = (diffuseNr++).ToString();
            else if(type == TexType.Specular)
                number = (specularNr++).ToString();

            shader.SetInt(("material." + type.ToString().ToLower() + number), i);
            GL.BindTexture(TextureTarget.Texture2D, _textures[i].Id);
        }
        
        GL.ActiveTexture(TextureUnit.Texture0);

        // draw mesh
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0); 
    }
    
    private void SetupMesh() {
        GL.GenVertexArrays(1, out _vao);
        GL.GenBuffers(1, out _vbo);
        GL.GenBuffers(1, out _ebo);
  
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        // Vertex Positions
        float[] vertices = new float[_vertPositions.Count * 3 + _vertNormals.Count * 3 + _vertTexCoords.Count * 2];
        int index = 0;
        foreach (var vertPos in _vertPositions) {
            vertices[index] = vertPos.X;
            vertices[index + 1] = vertPos.Y;
            vertices[index + 2] = vertPos.Z;
            index += 3;
        }
        foreach (var vertNormal in _vertNormals) {
            vertices[index] = vertNormal.X;
            vertices[index + 1] = vertNormal.Y;
            vertices[index + 2] = vertNormal.Z;
            index += 3;
        }
        foreach (var texCoord in _vertTexCoords) {
            vertices[index] = texCoord.X;
            vertices[index + 1] = texCoord.Y;
            index += 2;
        }
        
        // Upload the data.
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw); 

        // Bind EBO
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(int), _indices.ToArray(), BufferUsageHint.StaticDraw);

        // vertex positions
        GL.EnableVertexAttribArray(0);	
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        
        // vertex normals
        GL.EnableVertexAttribArray(1);	
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        
        // vertex texture coords
        GL.EnableVertexAttribArray(2);	
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

        // Unbind after finish
        GL.BindVertexArray(0);
    }  

}