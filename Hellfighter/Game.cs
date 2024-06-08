using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;

namespace Hellfighter;

public class Game(int width, int height, string title) : GameWindow(GameWindowSettings.Default, new NativeWindowSettings { ClientSize = (width, height), Flags = ContextFlags.Debug, Title = title}) {
    
    private readonly Vector3[] _cubePositions = [
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(2.0f, 5.0f, -15.0f),
        new Vector3(-1.5f, -2.2f, -2.5f),
        new Vector3(-3.8f, -2.0f, -12.3f),
        new Vector3(2.4f, -0.4f, -3.5f),
        new Vector3(-1.7f, 3.0f, -7.5f),
        new Vector3(1.3f, -2.0f, -2.5f),
        new Vector3(1.5f, 2.0f, -2.5f),
        new Vector3(1.5f, 0.2f, -1.5f),
        new Vector3(-1.3f, 1.0f, -1.5f)
    ];
    
    private readonly Vector3[] _pointLightPositions = [
        new Vector3(0.7f, 0.2f, 2.0f),
        new Vector3(2.3f, -3.3f, -4.0f),
        new Vector3(-4.0f, 2.0f, -12.0f),
        new Vector3(0.0f, 0.0f, -3.0f)
    ];
    
    private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);
    private readonly float _moveSpeed = 1.5f;

    private VertexArrayObj _vaoCube;
    private VertexArrayObj _vaoLamp;
    // private int _elementBufferObject;

    private VertexBufferObj<float> _vertexBufferObject;

    private Shader _lightingShader;
    private Camera _camera;

    private Vector2 _mouseLastPos;
    private bool _firstMove;
    private Shader _lampShader;

    private Matrix4 view;
    private Matrix4 projection;

    private Texture _diffuseMap;
    private Texture _specularMap;

    protected override void OnLoad() {
        base.OnLoad();

        // Setup
        GL.Enable(EnableCap.DepthTest);
        CursorState = CursorState.Grabbed;
        DebugCallback.Init();
        
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        _vertexBufferObject = new VertexBufferObj<float>(Objs.Vertices, Objs.Vertices.Length * sizeof(float));

        // VAO CUBE
        {
            _vaoCube = new VertexArrayObj(_vertexBufferObject);
            _vaoCube.Bind();
            
            _vaoCube.SetAttribArray(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            _vaoCube.SetAttribArray(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            _vaoCube.SetAttribArray(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        }

        // VAO LAMP
        {
            _vaoLamp = new VertexArrayObj(_vertexBufferObject);
            _vaoLamp.Bind();
            
            _vaoLamp.SetAttribArray(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        }

        _diffuseMap = new Texture("container2.png");
        _specularMap = new Texture("container2_specular.png");

        _camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y);

        _lightingShader = new Shader("shader.vert", "lighting.frag");
        _lightingShader.Bind();

        _lampShader = new Shader("shader.vert", "lampShader.frag");
        _lampShader.Bind();
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        view = _camera.GetViewMatrix();
        projection = _camera.GetProjectionMatrix();

        _diffuseMap.Bind(TextureUnit.Texture0);
        _specularMap.Bind(TextureUnit.Texture1);
        
        SetupLightingShader();

        _vaoCube.Bind();
        for (var i = 0; i < _cubePositions.Length; i++) {
            var cubePos = _cubePositions[i];
            RenderCubeObject(i, cubePos);
        }

        for (int i = 0; i < _pointLightPositions.Length; i++) {
            var lightPos = _pointLightPositions[i];
            RenderLampObject(i, lightPos);
        }

        SwapBuffers();
    }

    private void SetupLightingShader() {
        _lightingShader.Bind();

        _lightingShader.SetMatrix4("view", view);
        _lightingShader.SetMatrix4("projection", projection);

        _lightingShader.SetVector3("viewPos", _camera.Position);

        // Materials
        _lightingShader.SetInt("material.diffuse", 0);
        _lightingShader.SetInt("material.specular", 1);
        // _lightingShader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
        _lightingShader.SetFloat("material.shininess", 32.0f);
        
        // Directional Light
        _lightingShader.SetVector3("dirLight.direction", new Vector3(-0.2f, -1.0f, -0.3f));
        _lightingShader.SetVector3("dirLight.ambient", new Vector3(0.0f, 0.0f, 0.0f));
        _lightingShader.SetVector3("dirLight.diffuse", new Vector3(0.05f, 0.05f, 0.05f));
        _lightingShader.SetVector3("dirLight.specular", new Vector3(0.2f, 0.2f, 0.2f));
        
        // Point Lights
        // TODO: string allocation every frame! Optimize using Span<char>
        for (int i = 0; i < _pointLightPositions.Length; i++) {
            _lightingShader.SetVector3($"pointLights[{i}].position", _pointLightPositions[i]);
            _lightingShader.SetVector3($"pointLights[{i}].ambient", new Vector3(0.01f, 0.01f, 0.01f));
            _lightingShader.SetVector3($"pointLights[{i}].diffuse", new Vector3(0.8f, 0.8f, 0.8f));
            _lightingShader.SetVector3($"pointLights[{i}].specular", new Vector3(1.0f, 1.0f, 1.0f));
            _lightingShader.SetFloat($"pointLights[{i}].constant", 1.0f);
            _lightingShader.SetFloat($"pointLights[{i}].linear", 0.14f);
            _lightingShader.SetFloat($"pointLights[{i}].quadratic", 0.07f);
        }
        
        // Spot Light
        _lightingShader.SetVector3("spotLight.position", _camera.Position);
        _lightingShader.SetVector3("spotLight.direction", _camera.Front);
        _lightingShader.SetVector3("spotLight.ambient", new Vector3(0.0f, 0.0f, 0.0f));
        _lightingShader.SetVector3("spotLight.diffuse", new Vector3(1.0f, 1.0f, 1.0f));
        _lightingShader.SetVector3("spotLight.specular", new Vector3(1.0f, 1.0f, 1.0f));
        _lightingShader.SetFloat("spotLight.constant", 1.0f);
        _lightingShader.SetFloat("spotLight.linear", 0.09f);
        _lightingShader.SetFloat("spotLight.quadratic", 0.032f);
        _lightingShader.SetFloat("spotLight.cutOff", MathF.Cos(MathHelper.DegreesToRadians(10.0f)));
        _lightingShader.SetFloat("spotLight.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(15.0f)));
    }

    private void RenderCubeObject(int index, Vector3 position) {

        // Matrix4.Identity is used as the matrix, since we just want to draw it at 0, 0, 0
        var cubeModel = Matrix4.Identity * Matrix4.CreateTranslation(position);

        float angle = index * 20f;

        cubeModel *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), angle);

        _lightingShader.SetMatrix4("model", cubeModel);

        // 2 triangles with 3 vertices on each side of the cube => 2 * 3 * 6 = 36
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    private void RenderLampObject(int index, Vector3 position) {
        // Draw the lamp, this is mostly the same as for the model cube
        
        _vaoLamp.Bind();
        
        _lampShader.Bind();

        Matrix4 lampMatrix = Matrix4.CreateScale(0.2f); // We scale the lamp cube down a bit to make it less dominant
        lampMatrix *= Matrix4.CreateTranslation(position);

        _lampShader.SetMatrix4("model", lampMatrix);
        _lampShader.SetMatrix4("view", view);
        _lampShader.SetMatrix4("projection", projection);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);
        if (!IsFocused) {
            return;
        }


        HandleInput((float)e.Time);
    }

    private void HandleInput(float deltaTime) {
        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape)) {
            Close();
        }

        const float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        if (input.IsKeyDown(Keys.W)) {
            _camera.Position += _camera.Front * cameraSpeed * deltaTime; // Forward
        }
        if (input.IsKeyDown(Keys.S)) {
            _camera.Position -= _camera.Front * cameraSpeed * deltaTime; // Backwards
        }
        if (input.IsKeyDown(Keys.A)) {
            _camera.Position -= _camera.Right * cameraSpeed * deltaTime; // Left
        }
        if (input.IsKeyDown(Keys.D)) {
            _camera.Position += _camera.Right * cameraSpeed * deltaTime; // Right
        }
        if (input.IsKeyDown(Keys.Space)) {
            _camera.Position += Vector3.UnitY * cameraSpeed * deltaTime; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift)) {
            _camera.Position -= Vector3.UnitY * cameraSpeed * deltaTime; // Down
        }

        var mouse = MouseState;

        if (_firstMove) {
            _mouseLastPos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
        }
        else {
            var deltaX = mouse.X - _mouseLastPos.X;
            var deltaY = mouse.Y - _mouseLastPos.Y;
            _mouseLastPos = new Vector2(mouse.X, mouse.Y);

            _camera.Yaw += deltaX * sensitivity;
            _camera.Pitch -= deltaY * sensitivity;
        }
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e) {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
        _camera.AspectRatio = Size.X / (float)Size.Y;

    }

    protected override void OnMouseMove(MouseMoveEventArgs e) {
        base.OnMouseMove(e);

        if (IsFocused) // check to see if the window is focused  
        {
            // MousePosition = (e.X + width / 2f, e.Y + height / 2f);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e) {
        base.OnMouseWheel(e);

        _camera.Fov -= e.OffsetY;
    }

    protected override void OnUnload() {
        base.OnUnload();

        _lightingShader.Dispose();
        _lampShader.Dispose();
    }
}
