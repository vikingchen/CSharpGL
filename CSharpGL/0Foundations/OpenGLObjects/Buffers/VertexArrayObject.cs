﻿using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace CSharpGL
{
    /// <summary>
    /// VAO是用来管理VBO的。可以进一步减少DrawCall。
    /// <para>VAO is used to reduce draw-call.</para>
    /// </summary>
    [Editor(typeof(PropertyGridEditor), typeof(UITypeEditor))]
    public sealed class VertexArrayObject : IDisposable
    {
        private const string strVertexArrayObject = "Vertex Array Object";

        /// <summary>
        /// vertex attribute buffers('in vec3 position;' in shader etc.)
        /// </summary>
        [Category(strVertexArrayObject)]
        [Description("vertex attribute buffers('in vec3 position;' in shader etc.)")]
        public VertexBuffer[] VertexAttributeBuffers { get; private set; }

        /// <summary>
        /// The one and only one index buffer used to indexing vertex attribute buffers.
        /// </summary>
        [Category(strVertexArrayObject)]
        [Description("The one and only one index buffer used to indexing vertex attribute buffers.)")]
        public IndexBuffer IndexBuffer { get; private set; }

        private uint[] ids = new uint[1];

        /// <summary>
        /// 此VAO的ID，由OpenGL给出。
        /// <para>Id generated by glGenVertexArrays().</para>
        /// </summary>
        [Category(strVertexArrayObject)]
        [Description("Id generated by glGenVertexArrays().")]
        public uint Id { get { return ids[0]; } }

        private static OpenGL.glGenVertexArrays glGenVertexArrays;
        private static OpenGL.glBindVertexArray glBindVertexArray;
        private static OpenGL.glDeleteVertexArrays glDeleteVertexArrays;

        /// <summary>
        /// VAO是用来管理VBO的。可以进一步减少DrawCall。
        /// <para>VAO is used to reduce draw-call.</para>
        /// </summary>
        /// <param name="indexBuffer">index buffer pointer that used to invoke draw command.</param>
        /// <param name="vertexAttributeBuffers">给出此VAO要管理的所有VBO。<para>All VBOs that are managed by this VAO.</para></param>
        public VertexArrayObject(IndexBuffer indexBuffer, params VertexBuffer[] vertexAttributeBuffers)
        {
            if (indexBuffer == null)
            {
                throw new ArgumentNullException("indexBuffer");
            }
            // Zero vertex attribute is allowed in GLSL.
            //if (vertexAttributeBuffers == null || vertexAttributeBuffers.Length == 0)
            //{
            //    throw new ArgumentNullException("vertexAttributeBuffers");
            //}

            this.IndexBuffer = indexBuffer;
            this.VertexAttributeBuffers = vertexAttributeBuffers;
        }

        /// <summary>
        /// 在OpenGL中创建VAO。
        /// 创建的过程就是执行一次渲染的过程。
        /// <para>Creates VAO and bind it to specified VBOs.</para>
        /// <para>The whole process of binding is also the process of rendering.</para>
        /// </summary>
        /// <param name="shaderProgram"></param>
        public void Initialize(ShaderProgram shaderProgram)
        {
            if (this.Id != 0)
            { throw new Exception(string.Format("Id[{0}] is already generated!", this.Id)); }

            if (glGenVertexArrays == null)
            {
                glGenVertexArrays = OpenGL.GetDelegateFor<OpenGL.glGenVertexArrays>();
                glBindVertexArray = OpenGL.GetDelegateFor<OpenGL.glBindVertexArray>();
                glDeleteVertexArrays = OpenGL.GetDelegateFor<OpenGL.glDeleteVertexArrays>();
            }

            glGenVertexArrays(1, ids);

            this.Bind();// this vertex array object will record all stand-by actions.
            VertexBuffer[] vertexAttributeBuffers = this.VertexAttributeBuffers;
            if (vertexAttributeBuffers != null)
            {
                foreach (VertexBuffer item in vertexAttributeBuffers)
                {
                    item.Standby(shaderProgram);
                }
            }
            this.Unbind();// this vertex array object has recorded all stand-by actions.
        }

        private void Bind()
        {
            glBindVertexArray(this.Id);
        }

        private void Unbind()
        {
            glBindVertexArray(0);
        }

        /// <summary>
        /// 执行一次渲染的过程。
        /// <para>Execute rendering command.</para>
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="shaderProgram"></param>
        /// <param name="temporaryIndexBuffer">render by a temporary index buffer</param>
        public void Render(RenderEventArgs arg, ShaderProgram shaderProgram, IndexBuffer temporaryIndexBuffer = null)
        {
            if (temporaryIndexBuffer != null)
            {
                this.Bind();
                temporaryIndexBuffer.Render(arg);
                this.Unbind();
            }
            else
            {
                this.Bind();
                this.IndexBuffer.Render(arg);
                this.Unbind();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override string ToString()
        {
            return string.Format("VAO Id: {0}", this.Id);
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        ~VertexArrayObject()
        {
            this.Dispose(false);
        }

        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (this.disposedValue == false)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Dispose unmanaged resources.
                IntPtr ptr = Win32.wglGetCurrentContext();
                if (ptr != IntPtr.Zero)
                {
                    {
                        glDeleteVertexArrays(1, this.ids);
                        this.ids[0] = 0;
                    }
                    {
                        VertexBuffer[] vertexAttributeBuffers = this.VertexAttributeBuffers;
                        if (vertexAttributeBuffers != null)
                        {
                            foreach (VertexBuffer item in vertexAttributeBuffers)
                            {
                                item.Dispose();
                            }
                        }
                    }
                    {
                        IndexBuffer indexBuffer = this.IndexBuffer;
                        indexBuffer.Dispose();
                    }
                }
            }

            this.disposedValue = true;
        }
    }
}