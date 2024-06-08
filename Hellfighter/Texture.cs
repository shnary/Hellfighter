using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Hellfighter;

public class Texture {

    public int Handle { get; private set; }

    public Texture(string textureFileName) {
        string texturePath = Path.Combine(TheUtils.CurrentPath, "Res", textureFileName);

        Handle = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, Handle);

        StbImage.stbi_set_flip_vertically_on_load(1);

        // load the image
        ImageResult image = ImageResult.FromStream(File.OpenRead(texturePath), ColorComponents.RedGreenBlueAlpha);

        // upload the texture
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
        // We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Bind(TextureUnit textureUnit) {
        GL.ActiveTexture(textureUnit); // activate the texture unit first before binding texture
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
}
