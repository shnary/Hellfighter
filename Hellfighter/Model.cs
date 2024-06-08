using Assimp;
using Assimp.Unmanaged;

namespace Hellfighter;

public class Model {

    private List<Mesh> _meshes;

    public Model(string path) {
        _meshes = new List<Mesh>();
    }

    public void Draw(Shader shader) {
        foreach (var mesh in _meshes) {
            mesh.Draw(shader);
        }
    }

    private void LoadModel(string path) {
        IntPtr scene = AssimpLibrary.Instance.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs, 0);
    }

    private void ProcessNode(AiNode node, AiScene scene) {
        
    }

    private Mesh ProcessMesh(AiMesh mesh, AiScene scene) {
        return null;
    }

    private List<Texture> LoadMaterialTextures(AiMaterial mat, AiTexture type, string typeName) {
        return null;
    }
}