using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    class Asteroid
    {
        private float size = .5f;

        private Vector2 position;

        public Vector2 Position { get { return position; } set { position = value; } }

        private Texture texture;

        private Device device;

        /// <summary>
        /// Background vertex buffer
        /// </summary>
        private VertexBuffer backgroundV = null;



        public Asteroid(Device device, float s, Vector2 pos, int texture)
        {
            this.device = device;
            this.size = s;
            this.position = pos;
            // Load the background texture image

            if(texture == 1) this.texture = TextureLoader.FromFile(device, "../../asteroid1.png");
            else if(texture == 2) this.texture = TextureLoader.FromFile(device, "../../asteroid2.png");
            else this.texture = TextureLoader.FromFile(device, "../../asteroid3.png");

            // Create a vertex buffer for the background image we will draw
            backgroundV = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), // Type
                4,      // How many
                device, // What device
                0,      // No special usage
                CustomVertex.PositionColoredTextured.Format,
                Pool.Managed);

            // Fill the vertex buffer with the corners of a rectangle that covers
            // the entire playing surface.
            GraphicsStream stm = backgroundV.Lock(0, 0, 0);     // Lock the background vertex list
            int clr = Color.Transparent.ToArgb();
            stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) +new Vector3(-1.0f, -1.0f, 0) * size, clr, 0, 1));
            stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) +new Vector3(-1.0f, 1.0f, 0) * size, clr, 0, 0));
            stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) + new Vector3(1.0f, 1.0f, 0) * size, clr, 1, 0));
            stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) + new Vector3(1.0f, -1.0f, 0) * size, clr, 1, 1));

            backgroundV.Unlock();
        }

        /// <summary>
        /// Render the background image
        /// </summary>
        public void Render()
        {
          
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

                GraphicsStream stm = backgroundV.Lock(0, 0, 0);     // Lock the background vertex list
                int clr = Color.Transparent.ToArgb();
                stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) + new Vector3(-1.0f, -1.0f, 0) * size, clr, 0, 1));
                stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) + new Vector3(-1.0f, 1.0f, 0) * size, clr, 0, 0));
                stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) + new Vector3(1.0f, 1.0f, 0) * size, clr, 1, 0));
                stm.Write(new CustomVertex.PositionColoredTextured(new Vector3(position.X, position.Y, 0) + new Vector3(1.0f, -1.0f, 0) * size, clr, 1, 1));
            

            device.SetTexture(0, texture);
            device.SetStreamSource(0, backgroundV, 0);
            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
            device.SetTexture(0, null);

            device.RenderState.AlphaBlendEnable = false;
        }
    }
}
