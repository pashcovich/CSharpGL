﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpGL
{
    public partial class CtrlLabel
    {
        private string text = string.Empty;
        private CtrlLabelModel labelModel;
        private VertexBuffer positionBuffer;
        private VertexBuffer uvBuffer;
        private VertexBuffer textureIndexBuffer;
        private ZeroIndexBuffer indexBuffer;

        public event EventHandler TextChanged;
        protected void DoTextChanged()
        {
            var textChanged = this.TextChanged;
            if (textChanged != null)
            {
                TextChanged(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public unsafe string Text
        {
            get { return text; }
            set
            {
                string v = value == null ? string.Empty : value;
                if (v != text)
                {
                    text = v;
                    ArrangeCharaters(v, GlyphServer.defaultServer);
                    DoTextChanged();
                }
            }
        }

        unsafe private void ArrangeCharaters(string text, GlyphServer server)
        {
            float totalWidth, totalHeight;
            PositionPass(text, server, out totalWidth, out totalHeight);
            UVPass(text, server);
            TextureIndexPass(text, server);

            this.indexBuffer.RenderingVertexCount = text.Length * 4; // each alphabet needs 4 vertexes.

            this.Width = (int)(totalWidth * this.Height / totalHeight); // auto size means auto width.
        }

        /// <summary>
        /// start from (0, 0) to the right.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="server"></param>
        unsafe private void TextureIndexPass(string text, GlyphServer server)
        {
            VertexBuffer buffer = this.textureIndexBuffer;
            var textureIndexArray = (float*)buffer.MapBuffer(MapBufferAccess.WriteOnly);
            int index = 0;
            foreach (var c in text)
            {
                GlyphInfo glyphInfo;
                if (server.GetGlyphInfo(c, out glyphInfo))
                {
                    textureIndexArray[index] = glyphInfo.textureIndex;
                }

                index++;
            }

            buffer.UnmapBuffer();
        }

        /// <summary>
        /// start from (0, 0) to the right.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="server"></param>
        unsafe private void UVPass(string text, GlyphServer server)
        {
            VertexBuffer buffer = this.uvBuffer;
            var uvArray = (QuadStruct*)buffer.MapBuffer(MapBufferAccess.WriteOnly);
            int index = 0;
            foreach (var c in text)
            {
                GlyphInfo glyphInfo;
                if (server.GetGlyphInfo(c, out glyphInfo))
                {
                    uvArray[index] = glyphInfo.quad;
                }

                index++;
            }

            buffer.UnmapBuffer();
        }

        /// <summary>
        /// start from (0, 0) to the right.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="server"></param>
        /// <param name="totalWidth"></param>
        /// <param name="totalHeight"></param>
        unsafe private void PositionPass(string text, GlyphServer server,
            out float totalWidth, out float totalHeight)
        {
            int textureWidth = server.TextureWidth;
            int textureHeight = server.TextureHeight;
            VertexBuffer buffer = this.positionBuffer;
            var positionArray = (QuadStruct*)buffer.MapBuffer(MapBufferAccess.ReadWrite);
            const float height = 2.0f; // let's say height is 2.0f.
            totalWidth = 0;
            totalHeight = height;
            int index = 0;
            foreach (var c in text)
            {
                GlyphInfo glyphInfo;
                float wByH = 0;
                if (server.GetGlyphInfo(c, out glyphInfo))
                {
                    float w = (glyphInfo.quad.rightBottom.x - glyphInfo.quad.leftBottom.x) * textureWidth;
                    float h = (glyphInfo.quad.rightBottom.y - glyphInfo.quad.rightTop.y) * textureHeight;
                    wByH = height * w / h;
                }
                else
                {
                    // put an empty glyph(square) here.
                    wByH = height * 1.0f / 1.0f;
                }

                var leftTop = new vec2(totalWidth, 1f);
                var leftBottom = new vec2(totalWidth, -1f);
                var rightBottom = new vec2(totalWidth + wByH, -1f);
                var rightTop = new vec2(totalWidth + wByH, 1f);
                positionArray[index++] = new QuadStruct(leftTop, leftBottom, rightBottom, rightTop);
                totalWidth += wByH;
            }

            // move to center.
            const float scale = 1f;
            for (int i = 0; i < text.Length; i++)
            {
                QuadStruct quad = positionArray[i];
                var newPos = new QuadStruct(
                    // y is already in [-1, 1], so just shrink x to [-1, 1]
                    new vec2(quad.leftTop.x / totalWidth * 2.0f - 1f, quad.leftTop.y) * scale,
                    new vec2(quad.leftBottom.x / totalWidth * 2.0f - 1f, quad.leftBottom.y) * scale,
                    new vec2(quad.rightBottom.x / totalWidth * 2.0f - 1f, quad.rightBottom.y) * scale,
                    new vec2(quad.rightTop.x / totalWidth * 2.0f - 1f, quad.rightTop.y) * scale
                    );

                positionArray[i] = newPos;
            }

            buffer.UnmapBuffer();
        }

        private vec3 color = new vec3(0, 0, 0);
        /// <summary>
        /// Text color.
        /// </summary>
        public vec3 Color
        {
            get { return color; }
            set
            {
                if (color != value)
                {
                    ModernRenderUnit unit = this.RenderUnit;
                    if (unit == null) { return; }
                    RenderMethod method = unit.Methods[0];
                    if (method == null) { return; }
                    ShaderProgram program = method.Program;
                    if (program == null) { return; }

                    program.SetUniform("textColor", value);

                    color = value;
                }
            }
        }
    }
}
