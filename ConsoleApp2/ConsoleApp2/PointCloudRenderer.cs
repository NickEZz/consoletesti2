using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace ConsoleApp2
{
    internal class PointCloudRenderer 
    {
       
        private int programId;
        private int vboId;
        private int vaoId;
        public int PointS = 2;
        private Vector3[] points;
        private Camera _camera;
        Dictionary<string, int> _uniformLocations;

        public PointCloudRenderer()
        {
            // Initialize the Dictionary object
            _uniformLocations = new Dictionary<string, int>();

            // Get the location of the uniform variable named "model" in the shader program
            var modelLocation = GL.GetUniformLocation(programId, "model");

            // Add the key "model" and the location of the uniform variable to the Dictionary object
            _uniformLocations.Add("model", modelLocation);
        }
        public void LoadPointCloud(string fileName = "pistepilvi_1.xyz")
        {
            // Use a FileStream and a StreamReader to read the contents of the .xyz file.
            using (var stream = new FileStream(fileName, FileMode.Open))
            using (var reader = new StreamReader(stream))
          
            {
                var pointList = new List<Vector3>();


                // Parse the contents of the file and create a list of Vector3 objects, each representing a point in the point cloud.
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                    var point = new Vector3((float)Decimal.Parse(values[0], 
                        NumberStyles.Any | NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, culture), 
                        (float)Decimal.Parse(values[1],
                        NumberStyles.Any | NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, culture),
                        (float)Decimal.Parse(values[2], 
                        NumberStyles.Any | NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, culture));


                    pointList.Add(point);
                }

                points = pointList.ToArray();
            }
            
            GL.PointSize(PointS);
            GL.GenBuffers(1, out vboId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboId);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(points.Length * Vector3.SizeInBytes), points, BufferUsageHint.StaticDraw);

            // Create and compile the vertex shader.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, @"
           #version 420
    
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
            out float vZ;
         layout(location = 0) in vec3 aPosition;

        void main() {
          gl_Position = vec4(aPosition, 1.0) * model * view * projection;
         gl_PointSize = 10;
           vZ = aPosition.z;
                    }       
        ");
            GL.CompileShader(vertexShader);

            // Create and compile the fragment shader.
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, @"
            #version 420
            out vec4 fragColor;
            in float vZ;
            void main() {
             vec4 color;
              float lerpValue = (vZ+60.0) / 110.0;
              color = mix(vec4(0, 1, 0, 1), vec4(1, 0, 0, 1),lerpValue);
              fragColor = color;
              }
        ");
            GL.CompileShader(fragmentShader);


            // Use GL.CreateProgram to create a new program object, and attach the vertex and fragment shaders to it using GL.AttachShader.
            programId = GL.CreateProgram();
            GL.AttachShader(programId, vertexShader);
            GL.AttachShader(programId, fragmentShader);

            
            GL.Disable(EnableCap.Blend);

            // Use GL.LinkProgram to link the program object.
            GL.LinkProgram(programId);

            // Use GL.UseProgram to specify that the program object should be used for rendering.
            GL.UseProgram(programId);

          
            // Create a vertex array object (VAO)
            GL.GenVertexArrays(1, out vaoId);
            GL.BindVertexArray(vaoId);

            // Use GL.EnableVertexAttribArray to enable the vertex attribute array used to specify the point cloud data.
            GL.EnableVertexAttribArray(0);

            // Use GL.VertexAttribPointer to specify the layout of the point cloud data in the VBO.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.GetProgram(programId, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();
            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(programId, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(programId, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }
        }
      
        public int GetAttribLocation(string attribName)
        {
            var vertexLocation = GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            return GL.GetAttribLocation(programId, attribName);
        }
        public void SetInt(string name, int data)
        {

            int location = GL.GetUniformLocation(programId, name);
            GL.Uniform1(location, data);

        }
    
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(programId);

            GL.UniformMatrix4(_uniformLocations[name], true, ref data);


        }
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(programId);
            GL.Uniform1(_uniformLocations[name], data);
        }

        private bool disposedValue = false;

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(programId);
            GL.Uniform3(_uniformLocations[name], data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(programId);

                disposedValue = true;
            }
        }
       
        public void Render( )
        {

            // Use GL.UseProgram to specify that the program object should be used for rendering.
            GL.UseProgram(programId);
            
            // Use GL.BindVertexArray to specify the vertex array object to be used for rendering.
            GL.BindVertexArray(vaoId);


          GL.PointSize(PointS);

            // Use GL.DrawArrays to render the point cloud.
            GL.DrawArrays(PrimitiveType.Points, 0, points.Length);

            // In the RenderFrame method, call GL.Flush() to ensure that the point cloud is rendered on the screen.
            GL.Flush();
        }


     
    }


}


