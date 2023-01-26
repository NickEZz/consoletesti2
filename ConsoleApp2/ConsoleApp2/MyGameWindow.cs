using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK;
using OpenTK.Windowing;
using OpenTK.Input;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Reflection;

namespace ConsoleApp2
{
    class MyGameWindow : GameWindow
    {
         PointCloudRenderer pointCloudRenderer;
      
        private float yaw;
        private float pitch;
        private bool isMousePressed;
        private Vector2 lastMousePosition;

        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        private double _time;


        public MyGameWindow(int width, int height, string title) :
        base(GameWindowSettings.Default, new NativeWindowSettings()
        { Size = (width, height), Title = title })
 
        {
            
            pointCloudRenderer = new PointCloudRenderer();
           

        }

        protected override void OnLoad()
        {
            base.OnLoad();

                GL.ClearColor(Color4.Black);
                GL.Enable(EnableCap.DepthTest);

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            pointCloudRenderer.LoadPointCloud("pistepilvi_1.xyz");
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _time += 4.0 * e.Time;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


          
            var model = Matrix4.Identity;

            pointCloudRenderer.SetMatrix4("model", model);
           pointCloudRenderer.SetMatrix4("view", _camera.GetViewMatrix());
            pointCloudRenderer.SetMatrix4("projection", _camera.GetProjectionMatrix());

            pointCloudRenderer.Render();

            SwapBuffers();
        }


        private float lastYaw;
        private float lastPitch;
        private float lastWheel;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
           
            var input = KeyboardState;
            var mouse = MouseState;
            var scrollDelta = mouse.ScrollDelta.Y;

            float cameraSpeed = 150f;
            float sensitivity = 1.2f;
            float scrollSensitivity = 4f;
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if (scrollDelta != 0)
            {
                _camera.Position += _camera.Front * scrollDelta * scrollSensitivity;
                
            }

            if (mouse[MouseButton.Left])
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                if (deltaX > 0)
                    _camera.Yaw += sensitivity;
                else if (deltaX < 0)
                    _camera.Yaw -= sensitivity;

                if (deltaY < 0)
                    _camera.Pitch += sensitivity;
                else if (deltaY > 0)
                    _camera.Pitch -= sensitivity;
            }
            else
            {
                lastYaw = _camera.Yaw;
                lastPitch = _camera.Pitch;
            }

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            if (input.IsKeyPressed(Keys.Up))
            {
                pointCloudRenderer.PointS ++;
            }
            if (input.IsKeyPressed(Keys.Down))
            {
                pointCloudRenderer.PointS --;
            }
            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // eteen
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // taakse
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // vasen
            }
            if (input.IsKeyDown(Keys.D))
            {   
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // oikea
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // ylös
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // alas
            }
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }
        }


    }
}
